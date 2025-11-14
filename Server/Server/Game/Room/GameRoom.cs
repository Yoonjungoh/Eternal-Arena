using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Timers;
using static Server.Define;

namespace Server.Game
{
    public class GameRoom : JobSerializer
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public int RoomOwnerId { get; set; }
        Dictionary<int, GameObject> _gameObjects = new Dictionary<int, GameObject>();
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
            if (_players.Count > 0)
            {
                Player p = null;
                foreach (var player in _players.Values)
                {
                    p = player;
                    break;
                }
                ConsoleLogManager.Instance.Log($"Id: {p.Id}, CanGo: {MapManager.Instance.CanGo(p.Position.X, p.Position.Z)}");
            }
        }

        public void HandleAttack(Player instigator, AttackType attackType)
        {
            if (instigator == null) return;

            // 서버 기준 공격 시간 (플레이어 위치, 방향 예상하기 위함)
            long attackTimeMs = Util.GetTimestampMs();

            // 1. 공격자 위치 구하기
            // instigatorId.ObjectState.ServerReceivedTime 자주 갱신하면 더 정확해지더라 (당연한 말 -> HandleMove에서 업뎃 중임)
            Vector3 attackPos = instigator.CurrentPosition;

            // 2. 공격자 방향 구하기
            Vector3 attackForward = MovementHelper.ForwardFrom(instigator.ObjectState.Rotation);
            attackForward = Vector3.Normalize(attackForward);

            // 3. 공격 범위 알아내기
            float radius = instigator.ObjectState.Stat.AttackRange;
            float halfDeg = instigator.ObjectState.Stat.AttackHalfAngleDeg;
            float height = instigator.ObjectState.Stat.AttackHeight;

            // 3-1. 각도 안에 있는지 확인할 cos 구하기
            float cosLimit = (float)MathF.Cos(halfDeg * (MathF.PI / 180f));

            // 4. 후보 전부 검사하기
            List<int> damagedObjectList = new List<int>();

            foreach (Player target in _players.Values)
            {
                if (target == null) continue;
                if (target.Id == instigator.Id) continue;

                // 4-1. 대상 위치 예측하기
                Vector3 targetPos = target.CurrentPosition;

                // 4-2. 충돌 판정
                if (CollisionHelper.IsCollision(attackPos, attackForward, targetPos, radius, cosLimit, height))
                {
                    damagedObjectList.Add(target.Id);
                }
            }

            // 5. 데미지 처리
            S_Attack attackPacket = new S_Attack();
            foreach (int objectId in damagedObjectList) 
            {
                _gameObjects.TryGetValue(objectId, out GameObject damagedObject);
                if (damagedObject == null)
                    continue;

                damagedObject.OnDamaged(instigator, instigator.ObjectState.Stat.CommonAttackDamage);

                DamagedInfo damagedInfo = new DamagedInfo();
                damagedInfo.ObjectId = objectId;
                damagedInfo.RemainHp = damagedObject.ObjectState.Stat.Hp;
                attackPacket.DamagedObjectList.Add(damagedInfo);
            }

            // 6. 브로드캐스트
            Broadcast(attackPacket);
        }


        public void EnterGame(Player player)
        {
            if (player == null)
                return;

            player.GameRoom = this;
            player.ObjectState.Name = $"Player_{player.ObjectState.ObjectId}";

            S_EnterGame enteGamePacket = new S_EnterGame();
            enteGamePacket.ObjectState = new ObjectState();
            enteGamePacket.ObjectState.Position = new ProtoVector3();
            enteGamePacket.ObjectState.Velocity = new ProtoVector3();
            enteGamePacket.ObjectState.Rotation = new ProtoQuaternion();
            enteGamePacket.ObjectState.Stat = new Stat();

            // objectId 초기화
            enteGamePacket.ObjectState.ObjectId = player.Id;

            // name 초기화
            enteGamePacket.ObjectState.Name = player.ObjectState.Name;

            // position 초기화
            int spawnIndex = _players.Count % DataManager.Instance.MaxRoomPlayerCount;
            Vector3 startPos = DataManager.Instance.GetStartPosition(RoomType.GameRoom, spawnIndex);
            player.ObjectState.Position.X = startPos.X;
            player.ObjectState.Position.Y = startPos.Y;
            player.ObjectState.Position.Z = startPos.Z;
            enteGamePacket.ObjectState.Position.X = player.ObjectState.Position.X;
            enteGamePacket.ObjectState.Position.Y = player.ObjectState.Position.Y;
            enteGamePacket.ObjectState.Position.Z = player.ObjectState.Position.Z;

            // stat 초기화
            enteGamePacket.ObjectState.Stat = player.ObjectState.Stat;

            // creatureState 초기화
            player.ObjectState.CreatureState = CreatureState.Idle;
            enteGamePacket.ObjectState.CreatureState = CreatureState.Idle;

            if (player.Session != null)
            {
                player.Session.Send(enteGamePacket);
            }

            AddObject(player);

            // 본인한테 맵안의 플레이어 정보 전송
            S_Spawn spawnToMePacket = new S_Spawn();
            // 나를 제외하고 접속한 플레이어를 spawnPacket에 저장
            long serverReceivedTime = Util.GetTimestampMs();
            foreach (Player p in _players.Values)
            {
                if (p == null || player == p)
                    continue;

                p.ObjectState.ServerReceivedTime = serverReceivedTime;
                spawnToMePacket.ObjectStateList.Add(p.ObjectState);
            }
            if (player.Session != null)
            {
                player.Session.Send(spawnToMePacket);
            }

            // 다른 플레이어에게도 내가 접속한 걸 알려주기
            S_Spawn spawnToOthersPacket = new S_Spawn();
            spawnToOthersPacket.ObjectStateList.Add(player.ObjectState);
            foreach (Player p in _players.Values)
            {
                if (p == null || p.Session == null || player.Id == p.Id )
                    continue;
                
                p.ObjectState.ServerReceivedTime = serverReceivedTime;
                p.Session.Send(spawnToOthersPacket);
                ConsoleLogManager.Instance.Log($"[GameRoom Update] Player {p.Id} Pos({p.Position.X}, {p.Position.Y}, {p.Position.Z})");
            }
        }

        public void LeaveGame(int playerId)
        {

        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null || movePacket == null)
                return;

            // 서버에서 상태 업데이트
            player.ObjectState = movePacket.ObjectState;

            // 하드스냅 (일정 거리 이상 순간이동 방지)
            Vector3 serverPos = new Vector3(player.ObjectState.Position.X, player.ObjectState.Position.Y, player.ObjectState.Position.Z);
            Vector3 clientPos = new Vector3(movePacket.ObjectState.Position.X, movePacket.ObjectState.Position.Y, movePacket.ObjectState.Position.Z);

            float dist = Vector3.Distance(serverPos, clientPos);
            if (dist > 1.0f)
            {
                Console.WriteLine($"[Warning] Player {player.Id} position correction ({dist})");
                movePacket.ObjectState.Position = player.ObjectState.Position;
                movePacket.ObjectState.Velocity = new ProtoVector3 { X = 0, Y = 0, Z = 0 };
            }
            else
            {
                if (MapManager.Instance.CanGo(clientPos.X, clientPos.Z))
                {
                    // 이동 가능 
                    player.ObjectState.Position = movePacket.ObjectState.Position;
                }
                else
                {
                    // 이동 불가 (원래 서버 위치로 되돌림)
                    movePacket.ObjectState.Position = player.ObjectState.Position;
                    movePacket.ObjectState.Velocity = new ProtoVector3 { X = 0, Y = 0, Z = 0 };
                }
            }

            // 다른 유저들에게 브로드캐스트
            S_Move resMovePacket = new S_Move();
            resMovePacket.ObjectState = movePacket.ObjectState;
            resMovePacket.ObjectState.ServerReceivedTime = Util.GetTimestampMs();
            Broadcast(resMovePacket, player.Id);
        }

        public void HandleChangeCreatureState(int objectId, CreatureState creatureState)
        {
            S_ChangeCreatureState changeCreatureStatePacket = new S_ChangeCreatureState();
            changeCreatureStatePacket.ObjectId = objectId;
            changeCreatureStatePacket.CreatureState = creatureState;
            Broadcast(changeCreatureStatePacket, objectId);
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

        public void Broadcast(IMessage packet, int? exceptId = null)
        {
            foreach (Player p in _players.Values)
            {
                if (exceptId.HasValue && p.Id == exceptId.Value)
                    continue;

                if (p.Session == null)
                    continue;

                p.Session.Send(packet);
            }
        }

        private void AddObject(GameObject gameObject)
        {
            // 모든 오브젝트 관리하는 딕셔너리에 추가
            _gameObjects.Add(gameObject.Id, gameObject);
            
            // 분기별로 추가
            if (gameObject.ObjectType == GameObjectType.Player)
            {
                Player player = (Player)gameObject;
                _players.Add(player.Id, player);
            }
        }
    }
}