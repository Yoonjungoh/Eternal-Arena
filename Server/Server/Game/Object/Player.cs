using Google.Protobuf.Protocol;
using Microsoft.Identity.Client;
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
        
        public PlayerServerState PlayerServerState { get; set; }

        public string AccountId { get; set; }    // 로그인 Id

        public Lobby Lobby;	// 로비 나가면 초기화 해줘야 함
			
        public WaitingRoom WaitingRoom;	// 방 나가면 초기화 해줘야 함
        public ClientSession Session { get; set; }

        // 플레이어 정보 초기화
        public void Init()
		{
			ObjectType = GameObjectType.Player;

            // TODO - DB 에서 플레이어 정보 빼오기
            ObjectState.Name = $"Player_{ObjectState.ObjectId}";
            ObjectState.CreatureState = CreatureState.Idle;

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
            if (ObjectState.Stat == null)
            {
                ObjectState.Stat = new Stat();
            }

            ObjectState.Stat.MaxHp = 100.0f;
            ObjectState.Stat.Hp = ObjectState.Stat.MaxHp;
            ObjectState.Stat.CommonAttackDamage = 30.0f;
            ObjectState.Stat.Defense = 0.0f;
            ObjectState.Stat.MoveSpeed = 100.0f;
            ObjectState.Stat.CommonAttackCoolTime = 2.0f;
            ObjectState.Stat.AttackRange = 10.0f;
            ObjectState.Stat.AttackHalfAngleDeg = 30.0f;
            ObjectState.Stat.AttackHeight = 10.0f;
        }
    }
}
