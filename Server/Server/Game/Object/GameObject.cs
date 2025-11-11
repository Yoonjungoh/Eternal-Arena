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
		public CreatureState CreatureState { get { return ObjectState.CreatureState; } set { ObjectState.CreatureState = value; } }
		public float Hp { get { return ObjectState.Stat.Hp; } set { ObjectState.Stat.Hp = Math.Clamp(value, 0, ObjectState.Stat.MaxHp); } }
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
                OnDead(this);
			}
		}

		public virtual void OnDead(GameObject DamagedObject)
		{
			if (GameRoom == null)
				return;

			ConsoleLogManager.Instance.Log($"Id: {DamagedObject.Id}, Type: {DamagedObject.ObjectType} is dead");
			
			// 패킷처리도 LeaveGame에서 처리
            GameRoom.Push(() => GameRoom.LeaveGame(DamagedObject.Id));
        }
	}
}