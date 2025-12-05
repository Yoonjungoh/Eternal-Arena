using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;
using Server.Game.Object;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
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
        public Map Map { get; set; } = new Map();
        private Dictionary<int, GameObject> _gameObjects = new Dictionary<int, GameObject>();
        private Dictionary<int, Player> _players = new Dictionary<int, Player>();
        private Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        private Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();
        
        public bool IsRoomFull { get { return _players.Count == DataManager.Instance.MaxRoomPlayerCount; } }

        public event Action<int> OnEmptyRoom; // 방이 비었을 때 알림 (roomId)
        public event Action OnPlayerInfoChanged;  // 방 정보 바뀌었을 때 알림 (roomId)

        public void Init()
        {
            //TestTimer();
            Map.MapData = MapManager.Instance.CreateCopy();
            OnPlayerInfoChanged -= CheckForWinner;
            OnPlayerInfoChanged += CheckForWinner;
            // TODO
            SpawnMonster(MonsterType.Bear, new Vector3(100, -26, 527));
            SpawnMonster(MonsterType.Bear, new Vector3(80, -27, 500));
            SpawnMonster(MonsterType.Bear, new Vector3(100, -26, 420));
            //SpawnMonster(MonsterType.Bear, new Vector3(100, -26, 480));
        }

        // 어디선가 주기적으로 호출해줘야 함
        public void Update()
        {
            Flush();
            UpdateMonsters();
            UpdateProjectiles();
        }

        private void UpdateMonsters()
        {
            foreach (Monster monster in _monsters.Values)
            {
                monster.Update();
            }
        }

        private void UpdateProjectiles()
        {
            if (_projectiles == null || _projectiles.Count == 0)
                return;

            long now = Util.GetTimestampMs();

            List<int> removeList = new List<int>();

            foreach (Projectile projectile in _projectiles.Values)
            {
                if (now - projectile.SpawnTime >= projectile.LifeTime)
                {
                    removeList.Add(projectile.Id);
                }
            }

            foreach (int id in removeList)
            {
                LeaveGame(id);
            }
        }
        
        public void SpawnMonster(MonsterType monsterType, Vector3 spawnPos)
        {
            Monster monster = MonsterFactory.Create(monsterType);

            monster.MonsterType = monsterType;
            monster.ObjectState.Name = $"{monsterType}_{monster.ObjectState.ObjectId}";
            monster.Position = MovementHelper.Vec3ToProtoVec3(spawnPos);

            Push(EnterGame, monster);
        }

        public void SpawnProjectile(int ownerId, ProjectileType projectileType)
        {
            Projectile projectile = ProjectileFactory.Create(projectileType);
            // 주인이 존재하지 않는 오브젝트거나 똑같은 투사체 존재하면 스폰 안 함  
            if (_gameObjects.ContainsKey(ownerId) == false || _projectiles.ContainsKey(projectile.Id))
            {
                ConsoleLogManager.Instance.Log($"[Warning] Cannot spawn projectile. OwnerId: {ownerId}, ProjectileId: {projectile.Id}");
                return;
            }

            // 주인 추가해주기
            projectile.OwnerId = ownerId;
            var owner = _gameObjects[ownerId];

            // 먼저 회전부터 세팅
            projectile.Rotation = owner.Rotation;

            // 회전에서 forward 뽑기
            Vector3 forward = MovementHelper.ForwardFrom(projectile.Rotation);

            // 정규화
            if (forward.LengthSquared() > 1e-6f)
            {
                forward = Vector3.Normalize(forward);
            }

            // 스폰 위치 = 플레이어 위치 + forward * 오프셋
            Vector3 ownerPos = MovementHelper.ProtoVec3ToVec3(owner.Position);
            Vector3 spawnPos = ownerPos + (forward * owner.ProjectileSpawnOffset) + Vector3.UnitY;   // 살짝 위에

            // 세팅
            projectile.Position = MovementHelper.Vec3ToProtoVec3(spawnPos);
            projectile.Velocity = MovementHelper.Vec3ToProtoVec3(forward * projectile.Stat.MoveSpeed);
            projectile.SpawnTime = Util.GetTimestampMs();

            Push(EnterGame, projectile);
        }

        public void HandleAttack(int InstigatorId, int damagedObjectId, AttackType attackType)
        {
            switch (attackType)
            {
                case AttackType.CommonAttack:
                    HandleCommonAttack(InstigatorId);
                    break;
                case AttackType.RangedAttack:
                    HandleProjectileAttack(InstigatorId, damagedObjectId);
                    break;
                default:
                    ConsoleLogManager.Instance.Log($"Unknown AttackType: {attackType}");
                    break;
            }
        }

        private void HandleProjectileAttack(int instigatorId, int damagedObjectId)
        {
            _gameObjects.TryGetValue(instigatorId, out GameObject instigator);
            if (instigator == null) 
                return;

            // 이미 데미지 입힌 투사체면 return
            Projectile projectile = instigator as Projectile;
            if (projectile == null || projectile.hasDealtDamage == true)
                return;

            _gameObjects.TryGetValue(damagedObjectId, out GameObject damagedObject);
            if (damagedObject == null)
                return;

            // 서버에서 예측한 투사체 위치랑 적 위치 비교해서 오차 심하지 않으면 데미지 허용
            Vector3 projectilePos = projectile.CurrentPosition;
            Vector3 damagedObjectPos = damagedObject.CurrentPosition;
            //float dist = Vector3.Distance(projectilePos, damagedObjectPos);
            //if (dist > DataManager.Instance.ProjectileDistanceErrorThreshold)
            //{
            //    // 너무 멀리 떨어져 있음
            //    ConsoleLogManager.Instance.Log($"[Warning] Projectile attack distance too far: {dist}");
            //    return;
            //}

            // 데미지 처리
            S_Attack attackPacket = new S_Attack();
            damagedObject.OnDamaged(projectile, projectile.ObjectState.Stat.MagicMissileAttakDamage);
            projectile.hasDealtDamage = true;   // 데미지 한 번 입혔으니 다시 요청들어 오면 거부

            DamagedInfo damagedInfo = new DamagedInfo();
            damagedInfo.ObjectId = damagedObjectId;
            damagedInfo.RemainHp = damagedObject.ObjectState.Stat.Hp;
            attackPacket.DamagedObjectList.Add(damagedInfo);

            // 디스폰도 같이 처리해줘야 함
            LeaveGame(projectile.Id);

            Broadcast(attackPacket);
        }

        private void HandleCommonAttack(int instigatorId)
        {
            _gameObjects.TryGetValue(instigatorId, out GameObject instigator);
            if (instigator == null) 
                return;

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

            foreach (GameObject target in _gameObjects.Values)
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

        public void EnterGame(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType objectType = gameObject.ObjectType;

            gameObject.GameRoom = this;

            S_EnterGame enteGamePacket = new S_EnterGame();
            enteGamePacket.ObjectState = new ObjectState();
            enteGamePacket.ObjectState.Position = new ProtoVector3();
            enteGamePacket.ObjectState.Velocity = new ProtoVector3();
            enteGamePacket.ObjectState.Rotation = new ProtoQuaternion();
            enteGamePacket.ObjectState.Stat = new Stat();

            // objectId 초기화
            enteGamePacket.ObjectState.ObjectId = gameObject.Id;

            // objectType 초기화
            enteGamePacket.ObjectState.ObjectType = objectType;

            // creatureState 초기화
            gameObject.CreatureState = CreatureState.Idle;
            enteGamePacket.ObjectState.CreatureState = CreatureState.Idle;

            // TODO - Type 관련 분기 초기화 (없어도 되지 않나..?)
            if (objectType == GameObjectType.Monster)
            {
                enteGamePacket.ObjectState.MonsterType = gameObject.MonsterType;
            }
            else if (objectType == GameObjectType.Projectile)
            {
                enteGamePacket.ObjectState.ProjectileType = gameObject.ProjectileType;
                enteGamePacket.ObjectState.OwnerId = gameObject.OwnerId;
                // 투사체는 Move로 변경해주기
                gameObject.CreatureState = CreatureState.Move;
                enteGamePacket.ObjectState.CreatureState = CreatureState.Move;
            }

            // name 초기화
            enteGamePacket.ObjectState.Name = gameObject.ObjectState.Name;

            // position 초기화
            Vector3 startPos = Vector3.Zero;

            // 플레이어 이외는 다른 곳에서 위치 미리 받고 옴
            if (objectType == GameObjectType.Player)
            {
                int spawnIndex = _players.Count % DataManager.Instance.MaxRoomPlayerCount;
                startPos = DataManager.Instance.GetStartPosition(RoomType.GameRoom, spawnIndex);
            }
            else
            {
                startPos = MovementHelper.ProtoVec3ToVec3(gameObject.Position);
            }
            gameObject.Position.X = startPos.X;
            gameObject.Position.Y = startPos.Y;
            gameObject.Position.Z = startPos.Z;
            enteGamePacket.ObjectState.Position.X = gameObject.ObjectState.Position.X;
            enteGamePacket.ObjectState.Position.Y = gameObject.ObjectState.Position.Y;
            enteGamePacket.ObjectState.Position.Z = gameObject.ObjectState.Position.Z;
            
            // stat 초기화
            enteGamePacket.ObjectState.Stat = gameObject.Stat;

            // 플레이어면 본인 입장 패킷 전송
            if (objectType == GameObjectType.Player)
            {
                Player player = gameObject as Player;
                if (player.Session != null)
                {
                    player.Session.Send(enteGamePacket);
                }
            }

            AddObject(gameObject);

            long serverReceivedTime = Util.GetTimestampMs();
            if (objectType == GameObjectType.Player)
            {
                Player player = gameObject as Player;
                // 본인한테 맵안의 플레이어 정보 전송
                S_Spawn spawnToMePacket = new S_Spawn();
                // 나를 제외하고 접속한 플레이어를 spawnPacket에 저장
                foreach (GameObject go in _gameObjects.Values)
                {
                    if (go == null || player == go)
                        continue;

                    go.ObjectState.ServerReceivedTime = serverReceivedTime;
                    spawnToMePacket.ObjectStateList.Add(go.ObjectState);
                }
                if (player.Session != null)
                {
                    player.Session.Send(spawnToMePacket);
                }
            }

            // 다른 플레이어에게 게임 오브젝트가 접속한 걸 알려주기
            S_Spawn spawnToOthersPacket = new S_Spawn();
            spawnToOthersPacket.ObjectStateList.Add(gameObject.ObjectState);
            foreach (Player p in _players.Values)
            {
                if (p == null || p.Session == null || gameObject.Id == p.Id)
                    continue;

                p.ObjectState.ServerReceivedTime = serverReceivedTime;
                p.Session.Send(spawnToOthersPacket);
                ConsoleLogManager.Instance.Log($"[GameRoom Update] Player {p.Id} Pos({p.Position.X}, {p.Position.Y}, {p.Position.Z})");
            }
        }

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.Instance.GetObjectTypeById(objectId);

            if (type == GameObjectType.Player)
            {
                Player player = null;
                if (_players.TryGetValue(objectId, out player) == false)
                    return;

                player.GameRoom = null;

                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    leavePacket.RoomExitReason = RoomExitReason.GameLose;
                    player.Session.Send(leavePacket);
                }
            }
            RemoveObject(objectId);

            // 타인한테 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIdList.Add(objectId);
                despawnPacket.PlayerCount = _players.Count;

                foreach (Player p in _players.Values)
                {
                    if (p.Id != objectId)
                    {
                        p.Session.Send(despawnPacket);
                    }
                }
            }
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
                if (Map.CanGo(clientPos.X, clientPos.Z))
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

        private void CheckForWinner()
        {
            // 플레이어가 있었던 방인데 혼자 남거나 동시에 다 나가서 터진방일 때
            // 게임룸에선 승리 처리
            if (_players.Count <= 1)
            {
                foreach (Player p in _players.Values)
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    leavePacket.RoomExitReason = RoomExitReason.GameWin;
                    p.Session.Send(leavePacket);
                }
                Console.WriteLine($"Dont enough player so Room {RoomId} Delete!");
                OnEmptyRoom?.Invoke(RoomId);
            }
        }

        public void HandleChangeCreatureState(int objectId, CreatureState creatureState)
        {
            S_ChangeCreatureState changeCreatureStatePacket = new S_ChangeCreatureState();
            changeCreatureStatePacket.ObjectId = objectId;
            changeCreatureStatePacket.CreatureState = creatureState;
            Broadcast(changeCreatureStatePacket, objectId);
        }

        public void HandleStartCountdown()
        {
            S_StartCountdown startCountdownPacket = new S_StartCountdown();

            // 게임 시작 시간 초기화
            startCountdownPacket.GameStartCountdownTime = DataManager.Instance.GameStartCountdownTime;
            Broadcast(startCountdownPacket);
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
            foreach (Player p in _players.Values)
            {
                if (p.Session == null)
                    continue;

                p.Session.Send(packet);
            }
        }

        public void Broadcast(IMessage packet, int exceptId)
        {
            foreach (Player p in _players.Values)
            {
                if (p.Id == exceptId)
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
            else if (gameObject.ObjectType == GameObjectType.Monster)
            {
                Monster monster = (Monster)gameObject;
                _monsters.Add(monster.Id, monster);
            }
            else if (gameObject.ObjectType == GameObjectType.Projectile)
            {
                Projectile projectile = (Projectile)gameObject;
                _projectiles.Add(projectile.Id, projectile);
            }
        }

        private bool RemoveObject(int id)
        {
            if (_gameObjects.ContainsKey(id))
            {
                GameObjectType gameObjectType = ObjectManager.Instance.GetObjectTypeById(id);
                _gameObjects.Remove(id);
                if (gameObjectType == GameObjectType.Player)
                {
                    _players.Remove(id);
                    OnPlayerInfoChanged?.Invoke();
                }
                else if (gameObjectType == GameObjectType.Monster)
                {
                    _monsters.Remove(id);
                }
                else if (gameObjectType == GameObjectType.Projectile)
                {
                    _projectiles.Remove(id);
                }
                return true;
            }

            return false;
        }
    }
}