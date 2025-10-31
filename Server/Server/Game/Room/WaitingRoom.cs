using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Timers;

namespace Server.Game
{
    public class WaitingRoom : JobSerializer
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public int RoomOwnerId { get; set; }
        public int CurrentPlayerCount { get { return _players.Count; } }
        public bool CanEnterWaitingRoom { get { return CurrentPlayerCount < DataManager.Instance.MaxRoomPlayerCount; } }
        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        public event Action<int> OnEmptyRoom; // 방이 비었을 때 알림 (roomId)
        public event Action<int> OnRoomInfoChanged;  // 방 정보 바뀌었을 때 알림 (roomId)
        public void Init()
        {
            //TestTimer();
        }

        // 어디선가 주기적으로 호출해줘야 함
        public void Update()
        {
            Flush();
        }

        public void EnterRoom(Player player)
        {
            if (player == null)
                return;

            ObjectManager.Instance.Add<Player>();

            S_EnterWaitingRoom enterWaitingRoomPacket = new S_EnterWaitingRoom();

            // objectId
            enterWaitingRoomPacket.ObjectInfo = new ObjectInfo();
            enterWaitingRoomPacket.ObjectInfo.ObjectId = player.Id;
            enterWaitingRoomPacket.ObjectInfo.PositionInfo = new PositionInfo();

            // name
            player.WaitingRoom = this;
            player.ObjectInfo.Name = $"Player_{player.ObjectInfo.ObjectId}";
            enterWaitingRoomPacket.ObjectInfo.Name = player.ObjectInfo.Name;

            // positionInfo
            PositionInfo positionInfo = new PositionInfo();
            positionInfo.PosX = 0;
            positionInfo.PosY = 2;
            positionInfo.PosZ = 0;
            positionInfo.RotY = 0;
            enterWaitingRoomPacket.ObjectInfo.PositionInfo = positionInfo;

            // TODO - stat

            // creatureState
            player.ObjectInfo.CreatureState = CreatureState.Idle;
            enterWaitingRoomPacket.ObjectInfo.CreatureState = CreatureState.Idle;

            player.Session.Send(enterWaitingRoomPacket);

            _players.Add(player.Id, player);
            player.Init();

            // 방 정보가 업데이트 된 것을 로비의 유저들에게 알려야 함
            HandleRoomInfoChanged();

            // 본인한테 맵안의 플레이어 정보 전송
            S_Spawn spawnToMePacket = new S_Spawn();

            //나를 제외하고 접속한 플레이어를 spawnPacket에 저장
            foreach (Player p in _players.Values)
            {
                if (p == null)
                    continue;

                if (player != p)
                    spawnToMePacket.ObjectInfos.Add(p.ObjectInfo);
            }
            player.Session.Send(spawnToMePacket);

            // 다른 플레이어에게도 내가 접속한 걸 알려주기
            S_Spawn spawnToOthersPacket = new S_Spawn();
            spawnToOthersPacket.ObjectInfos.Add(player.ObjectInfo);
            foreach (Player p in _players.Values)
            {
                if (p.Id != player.Id)
                    p.Session.Send(spawnToOthersPacket);
            }
        }

        private void HandleRoomInfoChanged()
        {
            // 해당 방 안의 유저들에게 업데이트 정보 쏴주기
            S_UpdateWaitingRoomInfo updateWaitingRoomInfoPacket = new S_UpdateWaitingRoomInfo();
            updateWaitingRoomInfoPacket.RoomInfo = new RoomInfo();
            updateWaitingRoomInfoPacket.RoomInfo.RoomId = RoomId;
            updateWaitingRoomInfoPacket.RoomInfo.RoomName = RoomName;
            updateWaitingRoomInfoPacket.RoomInfo.RoomOwnerId = RoomOwnerId;
            updateWaitingRoomInfoPacket.RoomInfo.CurrentPlayerCount = CurrentPlayerCount;
            updateWaitingRoomInfoPacket.RoomInfo.MaxPlayerCount = DataManager.Instance.MaxRoomPlayerCount;
            Broadcast(updateWaitingRoomInfoPacket);

            // 로비의 유저들에겐 아래의 액션 함수 통해서 방 인원 변경 알리기
            OnRoomInfoChanged?.Invoke(RoomId);
        }

        public void LeaveRoom(int playerId)
        {
            Player player = null;
            if (_players.Remove(playerId, out player) == false)
                return;
            
            player.WaitingRoom = null;

            // 본인에겐 다시 로비로 가라고 하기
            ExitRoom(player);

            // 타인한테 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(playerId);
                despawnPacket.PlayerCount = _players.Count;

                foreach (Player p in _players.Values)
                {
                    if (p.Id != playerId)
                        p.Session.Send(despawnPacket);
                }
            }

            // 나간 사람이 방장인 경우 방 폭파
            if (RoomOwnerId == playerId)
            {
                ExitRoomAll();
                OnEmptyRoom?.Invoke(RoomId);
            }
        }

        public void ExitRoom(Player player)
        {
            if (player == null)
            {
                ConsoleLogManager.Instance.Log("Player is null");
                return;
            }
            S_ExitRoom exitRoomPacket = new S_ExitRoom();
            player.Session.Send(exitRoomPacket);

            // 방 정보가 업데이트 된 것을 로비의 유저들에게 알려야 함
            OnRoomInfoChanged?.Invoke(RoomId);
        }

        public void ExitRoomAll()
        {
            foreach (Player player in _players.Values)
            {
                ExitRoom(player);
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            // 서버에서 클라로 보낼 패킷 생성
            S_Move resMovePacket = new S_Move();
            resMovePacket.PositionInfo = new PositionInfo();

            // 움직였나 Moving 체크하는 부분
            // 클래스 취급이라 정보를 복사해오면 call by value가 아니라 call by refernce임
            // 서버에서 State 바꾸는 부분
            ObjectInfo objectInfo = player.ObjectInfo;

            if (objectInfo.PositionInfo.PosX == movePacket.PositionInfo.PosX &&
                objectInfo.PositionInfo.PosY == movePacket.PositionInfo.PosY &&
                objectInfo.PositionInfo.PosZ == movePacket.PositionInfo.PosZ &&
                objectInfo.PositionInfo.RotY == movePacket.PositionInfo.RotY)
                player.CreatureState = CreatureState.Idle;
            else
                player.CreatureState = CreatureState.Move;

            resMovePacket.CreatureState = player.CreatureState;

            // 서버에서 플레이어 좌표 이동 하는 부분
            objectInfo.PositionInfo = movePacket.PositionInfo;

            // 다른 플레이어들한테도 myPlayer가 움직이는 것을 알려준다
            resMovePacket.ObjectId = player.ObjectInfo.ObjectId;
            resMovePacket.PositionInfo = objectInfo.PositionInfo;
            Broadcast(resMovePacket);
        }

        public Player FindPlayer(Func<GameObject, bool> condition)
        {
            foreach (Player player in _players.Values)
            {
                if (condition.Invoke(player))
                    return player;
            }

            return null;
        }

        public void Broadcast(IMessage packet)
        {
            foreach (Player player in _players.Values)
            {
                player.Session.Send(packet);
            }
        }
    }
}