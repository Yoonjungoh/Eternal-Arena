using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Server.DB;
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

        public Lobby Lobby;	// 로비 나가면 초기화 해줘야 함
			
        public WaitingRoom WaitingRoom;	// 방 나가면 초기화 해줘야 함
        public int PlayerId { get; set; }   // DB에 저장된 플레이어 고유 Id
        public int Jewel { get; set; }    // 보석 재화
        public ClientSession Session { get; set; }

        // 플레이어 정보 초기화
        public void Init()
        {
            ObjectType = GameObjectType.Player;
            Name = $"NameNull_Player_{ObjectState.ObjectId}";
            ObjectState.CreatureState = CreatureState.Idle;

            InitStat();
        }

        public void Init(int playerId, string name)
		{
			ObjectType = GameObjectType.Player;
            Name = name;
            PlayerId = playerId;
            ObjectState.CreatureState = CreatureState.Idle;

			InitStat();
		}

		// TODO JSON - PlayerType에 따른 stat 변경
		public void InitStat()
		{
            if (ObjectState.Stat == null)
            {
                ObjectState.Stat = new Stat();
            }

            ObjectState.Stat.MaxHp = 100.0f;
            ObjectState.Stat.Hp = ObjectState.Stat.MaxHp;
            ObjectState.Stat.CommonAttackDamage = 30.0f;
            ObjectState.Stat.Defense = 0.0f;
            ObjectState.Stat.MoveSpeed = 7.0f;
            ObjectState.Stat.CommonAttackCoolTime = 2.0f;
            ObjectState.Stat.AttackRange = 10.0f;
            ObjectState.Stat.AttackHalfAngleDeg = 30.0f;
            ObjectState.Stat.AttackHeight = 10.0f;
        }

        public override void OnDamaged(GameObject instigator, float damage)
        {
            base.OnDamaged(instigator, damage);

        }

        public void OnLeaveGame()
        {
            // 1) 서버 다운되면 아직 저장되지 않은 정보 날아감
            // 2) 코드 흐름을 다 막아버림...
            // 2-1) 계정, 캐릭터 생성에선 혼자 막으니 상관 없는데, 인게임은 안 됨
            // -> DB 작업은 비동기로 바꿔?
            // -> 다른 스레드로 DB 일감 던져?
            // -> 결과를 받아서 이어서 처리하는 경우 에러 발생
            // DB 완료 처리 되면 이어서 하게 콜백으로 가야하나?
            using (GameDbContext db = new GameDbContext())
            {
                //// 아래는 좀 더 비효율적
                //PlayerDb playerDb = db.Players.Find(PlayerId);
                //playerDb.Jewel = Jewel;
                //db.SaveChangeEx();

                //// 아래가 더 효율적
                //PlayerDb playerDb = new PlayerDb();
                //playerDb.PlayerDbId = PlayerId;
                //playerDb.Jewel = Jewel;

                //db.Entry(playerDb).State = EntityState.Unchanged;
                //db.Entry(playerDb).Property(nameof(PlayerDb.Jewel)).IsModified = true;
                //db.SaveChangesEx();

                // 아래 사용해서 db 저장 관리 부분 따로 호출하기
                DbTransaction.SavePlayerStatus(this, GameRoom);
            }
        }
    }
}
