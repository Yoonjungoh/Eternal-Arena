using Google.Protobuf.Protocol;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Server.Game
{
    public class Monster : GameObject
    {
        float _searchRange = 7.0f;
        float _chaseRange = 20.0f;
        float _commonAttackRange = 2.0f;
        Player _target;

        public Monster()
        {
            ObjectType = GameObjectType.Monster;

            Stat.Hp = 50f;
            Stat.MaxHp = 50;
            Stat.CommonAttackDamage = 5;
            Stat.Defense = 0;
            Stat.MoveSpeed = 5;

            CreatureState = CreatureState.Idle;
        }

        public override void Update()
        {
            switch (CreatureState)
            {
                case CreatureState.Idle: UpdateIdle(); break;
                case CreatureState.Move: UpdateMove(); break;
                case CreatureState.Attack: UpdateAttack(); break;
                case CreatureState.Die: UpdateDie(); break;
            }
        }

        long _nextSearchTick = 0;
        int _searchTick = 500;

        public void UpdateIdle()
        {
            if (_nextSearchTick > Environment.TickCount64)
                return;

            _nextSearchTick = Environment.TickCount64 + _searchTick;

            Player target = GameRoom.FindPlayer(p =>
            {
                Vector3 playerPos = p.CurrentPosition;
                Vector3 dir = playerPos - CurrentPosition;
                float cellDist = Math.Abs(dir.X) + Math.Abs(dir.Y);
                return cellDist <= _searchRange;
            });

            if (target == null)
                return;

            _target = target;
            CreatureState = CreatureState.Move;

            S_FindTarget find = new S_FindTarget();
            find.MonsterId = Id;
            find.TargetId = _target.Id;
            GameRoom.Broadcast(find);
        }

        long _nextMoveTick = 0;
        long _nextPathTick = 0;
        int _pathInterval = 500;
        List<Vector3> _cachedPath = null;

        Vector3 _lastDir = Vector3.Zero;

        protected virtual void UpdateMove()
        {
            if (_nextMoveTick > Environment.TickCount64)
                return;

            long moveTick = (long)(400 / 2f);
            _nextMoveTick = Environment.TickCount64 + moveTick;

            if (_target == null || _target.GameRoom == null)
            {
                _cachedPath = null;
                CreatureState = CreatureState.Idle;
                return;
            }

            Vector3 diff = _target.CurrentPosition - CurrentPosition;
            float dist = diff.Length();

            // 범위 내에 오면 공격
            if (dist <= _commonAttackRange)
            {
                //CreatureState = CreatureState.Attack;
                return;
            }

            // 범위 내에 오면 유저 바라보게 방향만 전환
            if (dist <= 4f)
            {
                Vector3 dir = Vector3.Normalize(diff);
                Vector3 smooth = SmoothDirection(dir);
                MoveByDirection(smooth, moveTick);
                return;
            }

            if (_cachedPath == null || Environment.TickCount64 >= _nextPathTick)
            {
                _cachedPath = GameRoom.Map.FindPath(CurrentPosition, _target.CurrentPosition);
                _nextPathTick = Environment.TickCount64 + _pathInterval;
            }

            if (_cachedPath == null || _cachedPath.Count < 2)
                return;

            Vector3 nextPos = _cachedPath[1];
            Vector3 moveDir = Vector3.Normalize(nextPos - CurrentPosition);

            Vector3 finalDir = SmoothDirection(moveDir);
            MoveByDirection(finalDir, moveTick);
        }

        private Vector3 SmoothDirection(Vector3 newDir)
        {
            if (_lastDir == Vector3.Zero)
                _lastDir = newDir;

            _lastDir = Vector3.Normalize(_lastDir * 0.8f + newDir * 0.2f);
            return _lastDir;
        }

        private void MoveByDirection(Vector3 dir, long moveTick)
        {
            float deltaSec = moveTick / 1000f;
            float moveDist = Stat.MoveSpeed * deltaSec;

            Vector3 newPos = CurrentPosition + dir * moveDist;

            float groundY = GameRoom.Map.GetHeight(newPos);
            if (groundY == -9999)
                return;

            newPos.Y = groundY;

            if (newPos.Y < CurrentPosition.Y - 1f)
                newPos.Y = CurrentPosition.Y;

            ObjectState.Position.X = newPos.X;
            ObjectState.Position.Y = newPos.Y;
            ObjectState.Position.Z = newPos.Z;

            ObjectState.Rotation = MovementHelper.LookAt(dir);

            S_Move move = new S_Move();
            move.ObjectState = ObjectState;
            move.ObjectState.ServerReceivedTime = Util.GetTimestampMs();
            GameRoom.Broadcast(move);
        }

        long _nextSkillTick = 0;

        protected virtual void UpdateAttack()
        {
            //if (_target == null || _target.GameRoom == null)
            //{
            //    CreatureState = CreatureState.Idle;
            //    return;
            //}

            //Vector3 diff = _target.CurrentPosition - CurrentPosition;
            //float dist = diff.Length();

            //if (dist > _skillRange)
            //{
            //    CreatureState = CreatureState.Move;
            //    return;
            //}

            //if (_nextSkillTick == 0)
            //{
            //    float damage = Stat.CommonAttackDamage - _target.Stat.Defense;
            //    if (damage < 0) damage = 0;

            //    _target.OnDamaged(this, damage);

            //    S_Skill skill = new S_Skill();
            //    skill.ObjectId = Id;
            //    skill.Info = new SkillInfo() { SkillId = 1 };
            //    skill.State = CreatureState.Skill;
            //    GameRoom.Broadcast(skill);

            //    float skillCool = 1f;
            //    _nextSkillTick = Environment.TickCount64 + (long)(skillCool * 1000);
            //    return;
            //}

            //if (_nextSkillTick > Environment.TickCount64)
            //    return;

            //_nextSkillTick = 0;
        }

        protected virtual void UpdateDie() { }
    }
}
