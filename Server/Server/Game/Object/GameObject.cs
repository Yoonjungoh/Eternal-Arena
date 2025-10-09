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
			get { return ObjectInfo.ObjectId; }
			set { ObjectInfo.ObjectId = value; }
		}
		public GameRoom Room { get; set; }
		public GameRoom BackUpRoom { get; set; }

		public ObjectInfo ObjectInfo { get; set; } = new ObjectInfo();
		public PositionInfo PositionInfo { get; set; } = new PositionInfo();
		public Stat Stat { get; set; } = new Stat();
		public CreatureState CreatureState;
		public float Hp { get { return Stat.Hp; } set { Stat.Hp = Math.Clamp(value, 0, Stat.MaxHp); } }

		public GameObject()
		{
            ObjectInfo.PositionInfo = PositionInfo;
            ObjectInfo.Stat = Stat;
		}
		public virtual void Update()
		{

		}
		public virtual void OnDamaged(GameObject hitter, float damage)
		{
			if (Room == null)
				return;

			if (Stat.Hp <= 0)
			{
                // 죽음 처리 부분
                OnDead(hitter);
			}
		}

		public virtual void OnDead(GameObject hitter)
		{
			if (Room == null)
				return;

			//S_Die diePacket = new S_Die();
			//diePacket.ObjectId = Id;
			//diePacket.HitterId = hitter.Id;
		}
	}
}