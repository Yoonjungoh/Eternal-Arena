using Google.Protobuf.Protocol;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Server.Game
{
    public class Monster : GameObject
    {
        float _searchRange = 7.0f;
        float _chaseRange = 20.0f;
        float _skillRange = 2.0f;
        Player _target;

        public Monster()
        {
            ObjectType = GameObjectType.Monster;
            // TODO- JSON으로 빼오기
            Stat.Hp = 50f;
            Stat.MaxHp = 50;
            Stat.CommonAttackDamage = 5;
            Stat.Defense = 0;
            Stat.MoveSpeed = 5;
            //Exp = 50;
            //Gold = 10;
            CreatureState = CreatureState.Idle;
        }

        public override void Update()
        {
            switch (CreatureState)
            {
                case CreatureState.Idle:
                    UpdateIdle();
                    break;
                case CreatureState.Move:
                    UpdateMove();
                    break;
                case CreatureState.Attack:
                    UpdateAttack();
                    break;
                case CreatureState.Die:
                    UpdateDie();
                    break;
            }
        }

        long _nextSearchTick = 0;
        int _searchTick = 500;

        public void UpdateIdle()
        {
            if (_nextSearchTick > Environment.TickCount64)
                return;
            // 패트롤 로직
            _nextSearchTick = Environment.TickCount64 + _searchTick;
            Player target = GameRoom.FindPlayer(p =>
            {
                Vector3 playerPos = p.CurrentPosition;
                Vector3 dir = playerPos - CurrentPosition;
                float cellDistFromZero = Math.Abs(dir.X) + Math.Abs(dir.Y);
                return cellDistFromZero <= _searchRange;
            });

            if (target == null)
                return;

            _target = target;
            CreatureState = CreatureState.Move;

            S_FindTarget findPacket = new S_FindTarget();
            findPacket.MonsterId = Id;
            findPacket.TargetId = _target.Id;
            GameRoom.Broadcast(findPacket);
        }

        long _nextMoveTick = 0; 
        
        protected virtual void UpdateMove()
        {
            if (_nextMoveTick > Environment.TickCount64)
                return;

            long _moveTick = (long)(400 / 2f);
            _nextMoveTick = Environment.TickCount64 + _moveTick;

            // 타겟이 없거나 방을 나감
            if (_target == null || _target.GameRoom == null)
            {
                _target = null;
                CreatureState = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            // 범위 벗어남
            Vector3 dirCheck = _target.CurrentPosition - CurrentPosition;
            float dist = Math.Abs(dirCheck.X) + Math.Abs(dirCheck.Y);
            if (dist == 0 || dist > _chaseRange)
            {
                _target = null;
                CreatureState = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            // 1. A*로 경로 찾기
            List<Vector3> path = GameRoom.Map.FindPath(CurrentPosition, _target.CurrentPosition);

            // path가 너무 짧으면 이동 불가
            if (path == null || path.Count < 2)
            {
                // 혹시 스킬 사거리 안이면 공격
                if (dist <= _skillRange)
                {
                    //CreatureState = CreatureState.Attack;
                    BroadcastMove();
                }
                return;
            }

            // 2) 다음 목적지는 path[1]
            Vector3 nextPos = path[1];

            // 방향
            Vector3 dir = Vector3.Normalize(nextPos - CurrentPosition);

            // 이동량 = 속도 * 시간
            float delta = _moveTick / 1000f;
            float moveDist = Stat.MoveSpeed * delta;

            Vector3 newPos;

            if (Vector3.Distance(CurrentPosition, nextPos) <= moveDist)
                newPos = nextPos;
            else
                newPos = CurrentPosition + dir * moveDist;

            // 3. 서버 좌표 갱신
            ObjectState.Position.X = newPos.X;
            ObjectState.Position.Y = newPos.Y;
            ObjectState.Position.Z = newPos.Z;

            // 4. 회전도 갱신
            ObjectState.Rotation = MovementHelper.LookAt(dir);

            // 5. 브로드캐스트
            S_Move movePacket = new S_Move();
            movePacket.ObjectState = ObjectState;
            movePacket.ObjectState.ServerReceivedTime = Util.GetTimestampMs();
            GameRoom.Broadcast(movePacket);
            ConsoleLogManager.Instance.Log($"({Position.X}, {Position.Y}, {Position.Z})");
            // 6. 공격 사거리 체크
            if (dist <= _skillRange)
            {
                //CreatureState = CreatureState.Attack;
            }
        }


        long _nextSkillTick = 0;
        protected virtual void UpdateAttack()
        {
            //// 스킬 사용 가능 체크
            //if (_nextSkillTick == 0)
            //{
            //    // 유효 타겟인가?
            //    if (_target == null || _target.Room != Room || _target.Hp == 0)
            //    {
            //        _target = null;
            //        State = CreatureState.Moving;
            //        BroadcastMove();
            //        return;
            //    }

            //    // 스킬이 아직 사용 가능한가?
            //    Vector2Float dir = CurrentPos - _target.CurrentPos;
            //    float dist = dir.cellDistFromZero;
            //    bool canUseSkill = (dist <= _skillRange);
            //    if (canUseSkill == false)
            //    {
            //        State = CreatureState.Moving;
            //        BroadcastMove();
            //        return;
            //    }
            //    //            // 때릴때만 타겟팅 방향 보기
            //    //            // 방향 전환
            //    //            PosInfo.RotZ = -(float)(Math.Atan2(PosInfo.PosX - _target.PosInfo.PosX, PosInfo.PosY - _target.PosInfo.PosY) * (180.0f / Math.PI));
            //    //S_ChangeRotz rotZ = new S_ChangeRotz();
            //    //rotZ.RotZ = PosInfo.RotZ;
            //    //rotZ.ObjectId = Id;
            //    //Room.Broadcast(rotZ);
            //    // 데미지 판정
            //    _target.OnDamaged(this, Stat.Attack - _target.Stat.Defense);
            //    if (_target.Hp == 0)
            //        Gold += _target.Gold;
            //    //Console.WriteLine($"{_target.Id}에게 {Id}가 {Stat.Attack}의 대미지를 줌");
            //    //Console.WriteLine($"플레이어의 남은 체력: {_target.Hp}");

            //    // 스킬 사용 Broadcast
            //    S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            //    skill.ObjectId = Id;
            //    // TODO - 몬스터의 기본 공격은 1로 하기
            //    skill.Info.SkillId = 1;
            //    skill.State = CreatureState.Skill;
            //    Room.Broadcast(skill);

            //    // 스킬 쿨타임 적용
            //    float skillCool = 1f;
            //    int coolTick = (int)skillCool * 1000;
            //    _nextSkillTick = Environment.TickCount64 + coolTick;
            //}

            //// 준비 안 됨
            //if (_nextSkillTick > Environment.TickCount64)
            //{
            //    return;
            //}

            //// 스킬 쓴 후에 초기화
            //_nextSkillTick = 0;
        }

        protected virtual void UpdateDie()
        {

        }
        public override void OnDamaged(GameObject hitter, float damage)
        {
            base.OnDamaged(hitter, damage);
            // 맞으면 거리 멀어도 따라가게 함
            if (ObjectManager.Instance.GetObjectTypeById(hitter.Id) == GameObjectType.Player)
            {
                _target = hitter as Player;
                CreatureState = CreatureState.Move;
                Console.WriteLine($"new target Id: {hitter.Id}");
            }
        }
        public override void OnDead(GameObject hitter)
        {
            base.OnDead(hitter);
            //if (_canRespawn)
            //{
            //	// 리스폰 해주기
            //	Room.SpawnMonster(this.MonsterType, RespawnPos.x, RespawnPos.y);
            //}
        }

        // 타겟을 더이상 쫓지 않을 때의 몬스터 상태를 서버에 반영후 Broadcast
        void BroadcastMove()
        {
            //S_Move movePacket = new S_Move();
            //movePacket.ObjectId = Id;
            //movePacket.PosInfo = new PositionInfo();
            //movePacket.PosInfo.PosX = PosInfo.PosX;
            //movePacket.PosInfo.PosY = PosInfo.PosY;
            //movePacket.PosInfo.RotZ = PosInfo.RotZ;
            //movePacket.PosInfo.State = State;
            //Room.Broadcast(movePacket);
        }
    }
}
