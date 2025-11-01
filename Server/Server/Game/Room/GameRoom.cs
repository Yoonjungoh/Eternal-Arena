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
    public class GameRoom : JobSerializer
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

        public void EnterGame(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.Instance.GetObjectTypeById(gameObject.Id);
            

            if (type == GameObjectType.Player)
            {
                Player player = gameObject as Player;
                ObjectManager.Instance.Add<Player>();

                S_EnterGame enterPacket = new S_EnterGame();

                // objectId
                enterPacket.ObjectState = new ObjectState();
                enterPacket.ObjectState.ObjectId = player.Id;
                enterPacket.ObjectState.Position = new ProtoVector3();

                // name
                player.GameRoom = this;
                player.ObjectState.Name = $"Player_{player.ObjectState.ObjectId}";
                enterPacket.ObjectState.Name = player.ObjectState.Name;

                // positionInfo
                ProtoVector3 position = new ProtoVector3();
                position.X = 1;
                position.Y = 0.53f;
                position.Z = 0;
                enterPacket.ObjectState.Position = position;

                // TODO - stat

                // creatureState
                player.ObjectState.CreatureState = CreatureState.Idle;
                enterPacket.ObjectState.CreatureState = CreatureState.Idle;

                player.Session.Send(enterPacket);

                // 가끔 중복 키 들어가는 경우 해결하기 위함
                if (_players.ContainsKey(gameObject.Id))
                {
                    Player newPlayer = ObjectManager.Instance.Add<Player>();
                    _players.Add(newPlayer.Id, newPlayer);
                }
                else
                {
                    _players.Add(gameObject.Id, player);
                }
                player.Init();

                // 본인한테 맵안의 플레이어, 몬스터 정보 전송
                {
                    //S_EnterGame enterGamePacket = new S_EnterGame();
                    //enterGamePacket.ObjectInfo = player.Info;
                    //player.Session.Send(enterGamePacket);

                    S_Spawn spawnPacket = new S_Spawn();

                    //나를 제외하고 접속한 플레이어를 spawnPacket에 저장
                    foreach (Player p in _players.Values)
                    {
                        if (p == null)
                            continue;

                        if (player != p)
                            spawnPacket.ObjectStates.Add(p.ObjectState);
                    }
                    ////맵의 몬스터를 spawnPacket에 저장
                    //foreach (Monster m in _monsters.Values)
                    //{
                    //    spawnPacket.Objects.Add(m.Info);
                    //}
                    ////맵의 Projectile을 spawnPacket에 저장
                    //foreach (Projectile projectile in _projectiles.Values)
                    //{
                    //    spawnPacket.Objects.Add(projectile.Info);
                    //}
                    player.Session.Send(spawnPacket);
                }
            }
            //else if (type == GameObjectType.Monster)
            //{
            //    Monster monster = gameObject as Monster;
            //    _monsters.Add(gameObject.Id, monster);
            //    monster.Room = this;
            //    monster.BackUpRoom = monster.Room;
            //}
            // 타인한테 정보 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.ObjectStates.Add(gameObject.ObjectState);
                foreach (Player p in _players.Values)
                {
                    if (p.Id != gameObject.Id)
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

                player.GameRoom = null;
                
                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    leavePacket.PlayerCount = _players.Count;
                    leavePacket.IsAttacked = isAttacked;
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
            resMovePacket.ObjectState = new ObjectState();

            // 움직였나 Moving 체크하는 부분
            // 클래스 취급이라 정보를 복사해오면 call by value가 아니라 call by refernce임
            // 서버에서 State 바꾸는 부분
            ObjectState objectState = player.ObjectState;

            resMovePacket.ObjectState.CreatureState = player.CreatureState;

            // 서버에서 플레이어 좌표 이동 하는 부분
            objectState.Position = movePacket.ObjectState.Position;

            // 다른 플레이어들한테도 myPlayer가 움직이는 것을 알려준다
            resMovePacket.ObjectState.ObjectId = player.ObjectState.ObjectId;
            resMovePacket.ObjectState.Position = objectState.Position;
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