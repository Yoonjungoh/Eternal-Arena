using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using Server.Game.Room;

namespace Server.Game
{
	public class Player : GameObject
	{
		public ClientSession Session { get; set; }
		public bool IsWinner { get; set; }
		public Player()
		{
			Init();
		}
		public float BulletScaleBuff = 0f;

		public float BulletSpeedBuff = 0f;
		// 플레이어 정보 초기화
		public void Init()
		{
			ObjectType = GameObjectType.Player;

			Random rand = new Random();

            // DB 에서 플레이어 정보 빼오기
            ObjectInfo.Name = $"Player_{ObjectInfo.ObjectId}";
            ObjectInfo.CreatureState = CreatureState.Idle;

			InitStat();
		}

		// TODO JSON - PlayerType에 따른 stat 변경
		public void InitStat()
		{
			//Stat.MaxHp = DataManager.Instance.PlayerStatData[WeaponType].MaxHp;
			//Stat.Hp = DataManager.Instance.PlayerStatData[WeaponType].Hp;
			//Stat.Attack = DataManager.Instance.PlayerStatData[WeaponType].Attack;
			//Stat.Defense = DataManager.Instance.PlayerStatData[WeaponType].Defense;
			//Stat.Speed = DataManager.Instance.PlayerStatData[WeaponType].Speed;
			//Stat.CameraSize = DataManager.Instance.PlayerStatData[WeaponType].CameraSize;

		}
    }
}
