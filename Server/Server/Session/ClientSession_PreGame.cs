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
using Server.Session;

namespace Server
{
	public partial class ClientSession : PacketSession
	{
		public Player MyPlayer { get; set; }

		public int SessionId { get; set; }
        
        public int AccountId { get; set; }  // DB Id (Account 테이블의 AccountDbId임)

        private object _lock = new object();

        public ClientServerState ClientServerState { get; private set; } = ClientServerState.Login;

        public Player CreateMyPlayer(int playerId)
        {
            lock (_lock)
            {
                // enterLobbyPacket에 있는 PlayerId 활용해서 DB에서 찾은 다음 user 생성 후 넣기
                // 로비에 진입한 플레이어의 고유 아이디 (Player 테이블의 PlayerId)
                using (GameDbContext db = new GameDbContext())
                {
                    AccountDb account = db.Accounts
                        .Include(a => a.Players)
                        .Where(a => a.AccountDbId == AccountId)
                        .FirstOrDefault();

                    if (account == null)
                    {
                        ConsoleLogManager.Instance.Log("[Error] DB에서 계정 정보를 찾을 수 없음");
                        return null;
                    }

                    // 1. 플레이어 찾기
                    PlayerDb player = db.Players
                        .Where(p => p.PlayerId == playerId)
                        .FirstOrDefault();

                    // 2. 해당 플레이어 데이터를 바탕으로 현재 클라이언트 세션의 MyPlayer 생성
                    MyPlayer = ObjectManager.Instance.Add<Player>();
                    MyPlayer.Init(player.Name);
                    MyPlayer.Session = this;
                }

                return MyPlayer;
            }
        }

        public void HandleCreatePlayer(string name)
        {
            lock (_lock)
            {
                if (ClientServerState != ClientServerState.PlayerSelect)
                {
                    ConsoleLogManager.Instance.Log($"[Warning] 캐릭터 선택창 상태가 아닌 곳에서 캐릭터 생성 시도. SessionId: {SessionId}");
                    return;
                }

                using (GameDbContext db = new GameDbContext())
                {
                    AccountDb account = db.Accounts
                        .Include(a => a.Players)
                        .Where(a => a.AccountDbId == AccountId)
                        .FirstOrDefault();

                    if (account == null)
                    {
                        ConsoleLogManager.Instance.Log("[Error] DB에서 계정 정보를 찾을 수 없음");
                        return;
                    }

                    // 1. 캐릭터 이름 중복 검사
                    PlayerDb existingPlayer = db.Players
                        .Where(p => p.Name == name)
                        .FirstOrDefault();

                    S_CreatePlayer serverCreatePlayerPacket = new S_CreatePlayer();

                    if (existingPlayer != null)
                    {
                        // 중복이므로 생성 불가
                        serverCreatePlayerPacket.CanCreate = false;
                        Send(serverCreatePlayerPacket);
                        return;
                    }

                    // 2. PlayerId 자동 생성 (모든 계정의 전체 캐릭터 수 기반)
                    int newPlayerId = db.Players.Any() ? db.Players.Max(p => p.PlayerId) + 1 : 1;

                    // 3. 새 PlayerDb 생성
                    PlayerDb newPlayerDb = new PlayerDb()
                    {
                        AccountDbId = account.AccountDbId,
                        PlayerId = newPlayerId,
                        Name = name,
                        Gold = 1000
                    };

                    // 4. DB 저장
                    db.Players.Add(newPlayerDb);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateException ex)
                    {
                        ConsoleLogManager.Instance.Log($"[Error] DB Update Exception: {ex.Message}");
                        return;
                    }

                    // 혹시 모르니 최신 상태 다시 불러오기
                    account = db.Accounts
                        .Include(a => a.Players)
                        .Where(a => a.AccountDbId == account.AccountDbId)
                        .FirstOrDefault();

                    // 5. 생성 성공 결과 패킷 전송
                    serverCreatePlayerPacket.CanCreate = true;

                    foreach (PlayerDb player in account.Players)
                    {
                        PlayerSelectInfo info = new PlayerSelectInfo()
                        {
                            PlayerId = player.PlayerId,
                            Name = player.Name,
                            Gold = player.Gold
                        };

                        serverCreatePlayerPacket.PlayerInfoList.Add(info);
                    }

                    Send(serverCreatePlayerPacket);
                }
            }
        }

        public void HandleRequestPlayerList(C_RequestPlayerList requestPlayerListPacket)
        {
            lock (_lock)
            {
                if (ClientServerState != ClientServerState.PlayerSelect)
                {
                    ConsoleLogManager.Instance.Log($"[Warning] 캐릭터 선택창 상태가 아닌 곳에서 캐릭터 선택 시도. SessionId: {SessionId}");
                    return;
                }

                using (GameDbContext db = new GameDbContext())
                {
                    // Account + Players 데이터를 한 번에 로드해야 함
                    AccountDb account = db.Accounts
                        .Include(a => a.Players)
                        .Where(a => a.AccountDbId == AccountId)
                        .FirstOrDefault();

                    if (account == null)
                    {
                        Console.WriteLine("[Error] 계정을 찾을 수 없음");
                        return;
                    }

                    // 1. 계정의 Player 목록
                    List<PlayerDb> playerList = account.Players?.ToList() ?? new List<PlayerDb>();

                    // 2. 패킷 생성
                    S_RequestPlayerList serverRequestPlayerList = new S_RequestPlayerList();

                    foreach (PlayerDb player in playerList)
                    {
                        PlayerSelectInfo info = new PlayerSelectInfo()
                        {
                            PlayerId = player.PlayerId,
                            Name = player.Name,
                            Gold = player.Gold
                        };

                        serverRequestPlayerList.PlayerInfoList.Add(info);
                    }

                    // 3. 클라로 전송
                    Send(serverRequestPlayerList);
                }
            }
        }

        public void HandleLogin(C_Login loginPacket)
        {
            lock (_lock)
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
                        // 2-1. 비밀번호만 틀림
                        if (findAccount.AccountPassword != loginPacket.Password)
                        {
                            serverLoginPacket.LoginStatus = LoginStatus.PasswordWrong;
                            Send(serverLoginPacket);
                            return;
                        }

                        // 2-2. 이미 로그인 중인 아이디면 접속 못함
                        if (AccountManager.Instance.Add(findAccount.AccountDbId) == false)
                        {
                            serverLoginPacket.LoginStatus = LoginStatus.AlreadyLoggedIn;
                            Send(serverLoginPacket);
                            return;
                        }

                        // 2-3. 비밀번호 확인 성공 후, 로그인 처리
                        ClientServerState = ClientServerState.PlayerSelect;
                        AccountId = findAccount.AccountDbId;
                        serverLoginPacket.LoginStatus = LoginStatus.Success;
                        Send(serverLoginPacket);
                        return;
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

                    // 4. 회원 가입을 통한 로그인 성공 의미
                    AccountManager.Instance.Add(newAccount.AccountDbId);
                    ClientServerState = ClientServerState.PlayerSelect;
                    AccountId = newAccount.AccountDbId;
                    serverLoginPacket.LoginStatus = LoginStatus.Success;
                    Send(serverLoginPacket);
                }
            }
        }
    }
}
