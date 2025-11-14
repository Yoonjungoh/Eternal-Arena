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

            // 실제 스피드 - 이게 업데이트 틱이라고 봐도 됨
            long _moveTick = (long)(400 / 2f);
            //long _moveTick = (long)(1000 / Stat.Speed);
            _nextMoveTick = Environment.TickCount64 + _moveTick;

            // 타겟이나 타겟의 방이 없을 때
            if (_target == null || _target.GameRoom == null)
            {
                _target = null;
                CreatureState = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            // 도달했거나 쫓는 범위 밖일 때
            Vector3 dir = (_target.CurrentPosition - CurrentPosition);
            float dist = Math.Abs(dir.X) + Math.Abs(dir.Y);
            if (dist == 0 || dist > _chaseRange)
            {
                _target = null;
                CreatureState = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            Vector3 startCellPos = CurrentPosition;
            Vector3 destCellPos = _target.CurrentPosition;
            //List<vec> path = Room.Map.FindPath(startCellPos, destCellPos, true);
            //// 쫓는 본인 좌표도 포함이라 0번쨰는 본인 1번째는 타겟이라면 2 보다는 커야 한칸이라도 움직이겠단 거임
            //// 길이 없거나 너무 멀리 있다			

            //// 스킬 사정 거리 안쪽이지만 path.count가 3보다 작을 때 공격 안함을 방지하기 위함
            //if (path.Count < 3 || path.Count > (int)_chaseRange)
            //{
            //    if (dist > _skillRange)
            //    {
            //        _target = null;
            //        State = CreatureState.Idle;
            //        BroadcastMove();
            //        return;
            //    }
            //}

            //// 몬스터 이동 패킷 조립 부분
            //S_Move movePacket = new S_Move();

            //// 서버에서 이동 가능한지 체크
            //if (path.Count >= 3 && Room.Map.CanGo(path[1], true) == true)
            //{
            //    {
            //        //// 몬스터 각도 갱신(길 방향)
            //        //PosInfo.RotZ = -(float)
            //        //	(Math.Atan2(PosInfo.PosX - path[1].x,
            //        //	PosInfo.PosY - path[1].y) * (180.0f / Math.PI));  
            //        // 몬스터 각도 갱신 (플레이어 방향)
            //        PosInfo.RotZ = -(float)(Math.Atan2(PosInfo.PosX - _target.PosInfo.PosX, PosInfo.PosY - _target.PosInfo.PosY) * (180.0f / Math.PI));
            //        S_ChangeRotz rotZ = new S_ChangeRotz();
            //        rotZ.RotZ = PosInfo.RotZ;
            //        rotZ.ObjectId = Id;
            //        Room.Broadcast(rotZ);

            //        // 실제 좌표 이동
            //        PosInfo.PosX = path[1].x;
            //        PosInfo.PosY = path[1].y;
            //    }
            //}

            //// 패킷 조립
            //if (path.Count >= 3)
            //{
            //    movePacket.PosInfo = new PositionInfo();
            //    movePacket.ObjectId = Id;
            //    movePacket.PosInfo.PosX = path[1].x;
            //    movePacket.PosInfo.PosY = path[1].y;
            //    movePacket.PosInfo.RotZ = PosInfo.RotZ;
            //    movePacket.PosInfo.State = State;
            //    Room.Broadcast(movePacket);
            //}

            //// 몬스터 이동 패킷 조립 부분
            //S_Move movePacket = new S_Move();
            //Vector2 playerPos = new Vector2(_target.PosInfo.PosX, _target.PosInfo.PosY);
            //Vector2 monsterPos = new Vector2(PosInfo.PosX, PosInfo.PosY);
            //Vector2 dirVector = Vector2.Normalize(monsterPos - playerPos);
            //DirVector.x = -dirVector.X;
            //DirVector.y = -dirVector.Y;
            //State = CreatureState.Moving;

            ////Console.WriteLine($"Monster_{Id}: DirVector ({DirVector.x}, {DirVector.y})");
            //Vector2Float destPos = new Vector2Float
            //	(PosInfo.PosX + (DirVector.x * deltaTime * Stat.Speed), PosInfo.PosY + (DirVector.y * deltaTime * Stat.Speed));
            //// 서버에서 목표 좌표로 이동
            //PosInfo.PosX = destPos.x;
            //PosInfo.PosY = destPos.y;
            //// 방향 전환
            //PosInfo.RotZ = -(float)(Math.Atan2(PosInfo.PosX - _target.PosInfo.PosX, PosInfo.PosY - _target.PosInfo.PosY) * (180.0f / Math.PI));
            //// 패킷 조립
            //movePacket.PosInfo = new PositionInfo();
            //movePacket.ObjectId = Id;
            //movePacket.PosInfo.PosX = destPos.x;
            //movePacket.PosInfo.PosY = destPos.y;
            //movePacket.PosInfo.RotZ = PosInfo.RotZ;
            //movePacket.PosInfo.State = State;
            //Room.Broadcast(movePacket);

            // 스킬로 넘어갈지 체크 하는 부분
            if (dist <= _skillRange)
            {
                //_nextSkillTick = 0;
                CreatureState = CreatureState.Attack;
                BroadcastMove();
                return;
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
