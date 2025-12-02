using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.DB;
using Server.Game;
using ServerCore;

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();

		static void Main(string[] args)
		{
			// Json 데이터 역직렬화
			//DataManager.Instance.LoadAllData();
			//using (GameDbContext db = new GameDbContext())
			//{
			//	db.Accounts.Add(new AccountDb() 
			//	{ 
			//		AccountId = "TestAccount1",
			//		AccountPassword = "1234"
			//	});
			//	db.SaveChanges();
			//}

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
            //IPAddress ipAddr = ipHost.AddressList[1]; // for ec2
            IPAddress ipAddr = ipHost.AddressList[0]; // for test
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            MapManager.Instance.Init();
            LobbyManager.Instance.Init();

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			ConsoleLogManager.Instance.Log("Server Starting...");

			while (true)
			{
                Thread.Sleep(Timeout.Infinite);
            }
        }
	}
}
