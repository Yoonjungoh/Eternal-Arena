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
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace Server
{
	public partial class ClientSession : PacketSession
	{
		public Player MyPlayer { get; set; }

		public int SessionId { get; set; }

        public string AccountId { get; set; } = string.Empty;    // 로그인 Id

        public ClientServerState ClientServerState { get; private set; } = ClientServerState.Login;

        public void HandleCreatePlayer(string name)
        {

        }

        public void HandleRequestPlayerList(C_RequestPlayerList requestPlayerListPacket)
        {
            if (ClientServerState != ClientServerState.PlayerSelect)
            {
                ConsoleLogManager.Instance.Log($"[Warning] 캐릭터 선택창 상태가 아닌 곳에서 캐릭터 선택 시도. SessionId: {SessionId}");
                return;
            }

            using (GameDbContext db = new GameDbContext())
            {
                // 1. 계정 조회
                AccountDb account = db.Accounts
                    .Where(a => a.AccountId == AccountId)
                    .FirstOrDefault();

                if (account == null)
                {
                    Console.WriteLine("[Error] 계정을 찾을 수 없음");
                    return;
                }

                // 2. 계정에 속한 Player 목록 로드
                List<PlayerDb> playerList = db.Players
                    .Where(p => p.AccountDbId == account.AccountDbId).ToList();

                // 3. 패킷 생성
                S_RequestPlayerList serverRequestPlayerList = new S_RequestPlayerList();

                foreach (PlayerDb player in playerList)
                {
                    PlayerSelectInfo playerSelectInfo = new PlayerSelectInfo()
                    {
                        PlayerId = player.PlayerId,
                        PlayerName = player.PlayerName,
                        Gold = player.Gold
                    };
                    
                    serverRequestPlayerList.PlayerList.Add(playerSelectInfo);
                }
                
                Send(serverRequestPlayerList);
            }
        }

        public void HandleLogin(C_Login loginPacket)
        {
            ConsoleLogManager.Instance.Log($"AccountId: {loginPacket.Id}, Password: {loginPacket.Password}");

            if (ClientServerState != ClientServerState.Login)
            {
                ConsoleLogManager.Instance.Log($"[Warning] 로그인 상태가 아닌 곳에서 로그인 시도. SessionId: {SessionId}");
                return;
            }

            using (GameDbContext db = new GameDbContext())
            {
                // 1. 아이디 존재하는지 먼저 확인
                AccountDb findAccount = db.Accounts
                    .Where(a => a.AccountId == loginPacket.Id)
                    .FirstOrDefault();

                S_Login serverLoginPacket = new S_Login();
                
                // 2. 계정이 존재하는 경우
                if (findAccount != null)
                {
                    // 2-1. 비밀번호 확인
                    if (findAccount.AccountPassword == loginPacket.Password)
                    {
                        // 2-2. 기존 아이디로 로그인 의미
                        ClientServerState = ClientServerState.PlayerSelect;
                        AccountId = findAccount.AccountId;
                        serverLoginPacket.LoginStatus = LoginStatus.Success;
                        Send(serverLoginPacket);
                        return;
                    }
                    else
                    {
                        // 2-3. 비밀번호만 틀림
                        serverLoginPacket.LoginStatus = LoginStatus.PasswordWrong;
                        Send(serverLoginPacket);
                        return;
                    }
                }

                // 3. TODO - 계정이 없으면 새로운 계정 생성 (개발 환경에선 편하지만 라이브 때는 공식 회원가입 시키기)
                AccountDb newAccount = new AccountDb()
                {
                    AccountId = loginPacket.Id,
                    AccountPassword = loginPacket.Password
                };

                db.Accounts.Add(newAccount);

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    // DB가 중복 계정을 감지했다는 뜻
                    ConsoleLogManager.Instance.Log($"[Error] DB Update Exception: {ex.Message}");
                    return;
                }

                // 4.회원 가입을 통한 로그인 성공 의미
                ClientServerState = ClientServerState.PlayerSelect;
                AccountId = findAccount.AccountId;
                serverLoginPacket.LoginStatus = LoginStatus.Success;
                Send(serverLoginPacket);
            }
        }

    }
}
