using Microsoft.EntityFrameworkCore;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB
{
    // DB 작업 해주는 클래스
    public class DbTransaction : JobSerializer
    {
        public static DbTransaction Instance { get; } = new DbTransaction();

        // 게임룸에서 호출하는 DB 저장 요청 부분
        public static void SavePlayerStatus(Player player, GameRoom room)
        {
            if (player == null || room == null)
                return;

            // Me (GameRoom)
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerId;
            playerDb.Jewel = player.Jewel;
            Instance.Push<PlayerDb, GameRoom>(SavePlayerStatus_Db, playerDb, room);
        }

        // DB 저장 부분
        public static void SavePlayerStatus_Db(PlayerDb playerDb, GameRoom room)
        {
            using (GameDbContext db = new GameDbContext())
            {
                db.Entry(playerDb).State = EntityState.Unchanged;
                db.Entry(playerDb).Property(nameof(PlayerDb.Jewel)).IsModified = true;
                bool success = db.SaveChangesEx();
                if (success)
                {
                    room.Push(SavePlayerStatus_OnComplete, playerDb.Jewel);
                }
            }
        }

        // DB 저장 완료되면 사용할 콜백
        public static void SavePlayerStatus_OnComplete(int jewel)
        {
            Console.WriteLine($"Jewel Saved({jewel})");
        }
    }
}
