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

        public AccountDb AccountDb { get; set; }    // 캐싱용 DB 

        public ClientServerState ClientServerState { get; private set; } = ClientServerState.Login;

        public void HandleCreatePlayer(string name)
        {
            if (ClientServerState != ClientServerState.PlayerSelect)
            {
                ConsoleLogManager.Instance.Log($"[Warning] 캐릭터 선택창 상태가 아닌 곳에서 캐릭터 선택 시도. SessionId: {SessionId}");
                return;
            }

            using (GameDbContext db = new GameDbContext())
            {
                if (AccountDb == null)
                {
                    Console.WriteLine("[Error] 계정을 찾을 수 없음");
                    return;
                }

                // 1. 모든 계정들의 캐릭터 이름은 고유해야 함.
                // 따라서 모든 계정들의 캐릭터이름을 순회하면서 중복인지 알아내야 함
                PlayerDb existingPlayer = db.Players
                    .Where(p => p.Name == name)
                    .FirstOrDefault();

                S_CreatePlayer serverCreatePlayerPacket = new S_CreatePlayer();

                if (existingPlayer == null)
                {
                    // 1-1. 중복이라 생성 못한다고 패킷 전달
                    serverCreatePlayerPacket.CanCreate = false;
                    Send(serverCreatePlayerPacket);
                    return;
                }

                // 2. 캐릭터 생성
                // 2-1. TODO - 새로운 플레이어 아이디는 현재 db의 유저수 + 1
                List<PlayerDb> playerList = AccountDb.Players.ToList();
                int newPlayerId = playerList.Count + 1;

                // TODO - 이후에 메인 메뉴 생기면 분리
                //EnterLobby();
                PlayerDb newPlayerDb = new PlayerDb()
                {
                    AccountDbId = AccountDb.AccountDbId,
                    Name = name,
                    Gold = 1000,
                    PlayerId = newPlayerId // TODO - 임시로 Ticks 이용
                };
                

                // 3. 패킷 생성
                serverCreatePlayerPacket.CanCreate = true;
                // 3-1. 방금 생성한 플레이어를 playerList에 추가
                playerList.Add(newPlayerDb);
                db.Players.Add(newPlayerDb);
                db.SaveChanges();

                foreach (PlayerDb player in playerList)
                {
                    PlayerSelectInfo playerSelectInfo = new PlayerSelectInfo()
                    {
                        PlayerId = player.PlayerId,
                        Name = player.Name,
                        Gold = player.Gold
                    };

                    serverCreatePlayerPacket.PlayerInfoList.Add(playerSelectInfo);
                }

                Send(serverCreatePlayerPacket);
            }
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
                if (AccountDb == null)
                {
                    Console.WriteLine("[Error] 계정을 찾을 수 없음");
                    return;
                }

                // 1. 계정에 속한 Player 목록 로드
                List<PlayerDb> playerList = AccountDb.Players.ToList();

                // 2. 패킷 생성
                S_RequestPlayerList serverRequestPlayerList = new S_RequestPlayerList();

                foreach (PlayerDb player in playerList)
                {
                    PlayerSelectInfo playerSelectInfo = new PlayerSelectInfo()
                    {
                        PlayerId = player.PlayerId,
                        Name = player.Name,
                        Gold = player.Gold
                    };
                    
                    serverRequestPlayerList.PlayerInfoList.Add(playerSelectInfo);
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
                        AccountDb = findAccount;
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
                AccountDb = findAccount;
                serverLoginPacket.LoginStatus = LoginStatus.Success;
                Send(serverLoginPacket);
            }
        }

    }
}
