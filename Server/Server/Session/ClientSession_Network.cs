using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using Server.Game;
using Server.DB;
using System.Linq;

namespace Server
{
	public partial class ClientSession : PacketSession
	{
		public Player MyPlayer { get; set; }
		public int SessionId { get; set; }

        public void HandleCreatePlayer(string name)
        {

        }

        public void HandleLogin(C_Login loginPacket)
        {
            using (GameDbContext db = new GameDbContext())
            {
                AccountDb findAccount = db.Accounts
                    .Where(a => (a.AccountId == loginPacket.Id) && a.AccountPassword == loginPacket.Password).FirstOrDefault();

                if (findAccount != null)
                {
                    S_Login serverLoginPacket = new S_Login();
                    serverLoginPacket.LoginStatus = LoginStatus.Success;
                    Send(serverLoginPacket);
                }
                else
                {
                    // TODO - 우선은 회원가입 바로 시키기
                    AccountDb newAccount = new AccountDb();
                    newAccount.AccountId = loginPacket.Id;
                    newAccount.AccountPassword = loginPacket.Password;
                    db.Accounts.Add(newAccount);
                    db.SaveChanges();

                    S_Login serverLoginPacket = new S_Login();
                    serverLoginPacket.LoginStatus = LoginStatus.Success;
                    Send(serverLoginPacket);
                }
            }
        }
    }
}
