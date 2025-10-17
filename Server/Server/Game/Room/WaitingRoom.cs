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
        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        public void Init()
        {
            //TestTimer();
        }

        // 어디선가 주기적으로 호출해줘야 함
        public void Update()
        {
            //if (IsGameOver)
            //    return;
            //foreach (Projectile projectile in _projectiles.Values)
            //{
            //    projectile.Update();
            //}
            //foreach (Monster monster in _monsters.Values)
            //{

            //    monster.Update();
            //}
            Flush();
        }

        void TestTimer()
        {
            Console.WriteLine("TestTimer");
            PushAfter(TestTimer, 100);
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

            // 본인한테 맵안의 플레이어 정보 전송
            {
                S_Spawn spawnPacket = new S_Spawn();

                //나를 제외하고 접속한 플레이어를 spawnPacket에 저장
                foreach (Player p in _players.Values)
                {
                    if (p == null)
                        continue;

                    if (player != p)
                        spawnPacket.ObjectInfos.Add(p.ObjectInfo);
                }

                player.Session.Send(spawnPacket);
            }
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.ObjectInfos.Add(player.ObjectInfo);
                foreach (Player p in _players.Values)
                {
                    if (p.Id != player.Id)
                        p.Session.Send(spawnPacket);
                }
            }
        }

        public void LeaveGame(int objectId, bool isAttacked = true)
        {
            GameObjectType type = ObjectManager.Instance.GetObjectTypeById(objectId);

            //if (type == GameObjectType.Player)
            {
                Player player = null;
                if (_players.Remove(objectId, out player) == false)
                    return;

                player.WaitingRoom = null;
                
                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    leavePacket.PlayerCount = _players.Count;
                    player.Session.Send(leavePacket);
                }

                // 타인한테 정보 전송
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.ObjectIds.Add(objectId);
                    despawnPacket.PlayerCount = _players.Count;

                    foreach (Player p in _players.Values)
                    {
                        if (p.Id != objectId)
                            p.Session.Send(despawnPacket);
                    }
                }
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