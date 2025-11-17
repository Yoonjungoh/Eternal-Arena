using Google.Protobuf.Protocol;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Server.Game
{
	public class GameObject
	{
		public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
		public int Id
		{
			get { return ObjectState.ObjectId; }
			set { ObjectState.ObjectId = value; }
        }
        public GameRoom GameRoom { get; set; }

		public ObjectState ObjectState { get; set; }
        public ProtoVector3 Position { get { return ObjectState.Position; } set { ObjectState.Position = value; } }
        public ProtoVector3 Velocity { get { return ObjectState.Velocity; } set { ObjectState.Velocity = value; } }
		public Vector3 CurrentPosition
		{
			get
			{
				return MovementHelper.PredictPosition(
                MovementHelper.ProtoVec3ToVec3(Position),
                MovementHelper.ProtoVec3ToVec3(Velocity),
                ObjectState.ServerReceivedTime,
                Util.GetTimestampMs()
                );
            }
		}
        public CreatureState CreatureState { get { return ObjectState.CreatureState; } set { ObjectState.CreatureState = value; } }
		public Stat Stat { get { return ObjectState.Stat; } set { ObjectState.Stat = value; } }
        public MonsterType MonsterType { get { return ObjectState.MonsterType; } set { ObjectState.MonsterType = value; } }
        public GameObject()
        {
            ObjectState = new ObjectState();
            ObjectState.Position = new ProtoVector3();
            ObjectState.Velocity = new ProtoVector3();
            ObjectState.Rotation = new ProtoQuaternion();
            ObjectState.Stat = new Stat();
            ObjectState.CreatureState = new CreatureState();
        }

        public virtual void Update()
		{

		}

		public virtual void OnDamaged(GameObject instigator, float damage)
		{
			if (GameRoom == null)
				return;

			// 실제 데미지 계산
            float damageDifference = damage - ObjectState.Stat.Defense;
            float realDamage = Math.Clamp(damageDifference, 0.0f, DataManager.Instance.MaxDamage);

			// 남은 체력 계산
            ObjectState.Stat.Hp -= realDamage;
            ObjectState.Stat.Hp = Math.Clamp(ObjectState.Stat.Hp, 0.0f, DataManager.Instance.MaxHp);
			
            if (ObjectState.Stat.Hp <= 0.0f)
			{
                // 죽음 처리 부분
                OnDead(instigator);
			}
		}

		public virtual void OnDead(GameObject instigator)
		{
			if (GameRoom == null)
				return;

			ConsoleLogManager.Instance.Log($"Id: {Id}, Type: {ObjectType} is dead");
            CreatureState = CreatureState.Die;

            S_Die diePacket = new S_Die();
            diePacket.DamagedObjectId = Id;
            diePacket.InstigatorId = instigator.Id;

            if (GameRoom != null)
            {
                GameRoom.Push(GameRoom.Broadcast, diePacket);
                GameRoom.Push(GameRoom.LeaveGame, Id);
            }
            
        }
	}
}