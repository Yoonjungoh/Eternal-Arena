using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class Player : GameObject
    {
        public Player()
        {
            Init();
        }

        public int LobbyId;	// 로비 나가면 초기화 해줘야 함
			
        public int RoomId;	// 방 나가면 초기화 해줘야 함
        public ClientSession Session { get; set; }

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
