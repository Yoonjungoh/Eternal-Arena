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

		public virtual void OnDamaged(GameObject hitter, float damage)
		{
			if (GameRoom == null)
				return;

			if (ObjectState.Stat.Hp <= 0)
			{
                // 죽음 처리 부분
                OnDead(hitter);
			}
		}

		public virtual void OnDead(GameObject hitter)
		{
			if (GameRoom == null)
				return;

			//S_Die diePacket = new S_Die();
			//diePacket.ObjectId = Id;
			//diePacket.HitterId = hitter.Id;
		}
	}
}