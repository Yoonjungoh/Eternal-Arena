using Google.Protobuf.Protocol;
using Server.Game;
using System;
using System.Collections.Generic;
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

		public ObjectState ObjectState { get; set; } = new ObjectState();
		public ProtoVector3 Position { get { return ObjectState.Position; } set { ObjectState.Position = value; } }
		public Stat Stat { get { return ObjectState.Stat; } set { ObjectState.Stat = value; } }
		public CreatureState CreatureState { get { return ObjectState.CreatureState; } set { ObjectState.CreatureState = value; } }
		public float Hp { get { return Stat.Hp; } set { Stat.Hp = Math.Clamp(value, 0, Stat.MaxHp); } }

		public GameObject()
		{
            ObjectState.Position = Position;
            ObjectState.Stat = Stat;
		}
		public virtual void Update()
		{

		}
		public virtual void OnDamaged(GameObject hitter, float damage)
		{
			if (GameRoom == null)
				return;

			if (Stat.Hp <= 0)
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