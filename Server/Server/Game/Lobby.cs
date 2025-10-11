using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Timers;

namespace Server.Game
{
    public class Lobby : JobSerializer
    {
        public int LobbyId { get; set; }
        
        Dictionary<int, Player> _users = new Dictionary<int, Player>();

        public void Init()
        {
            // TODO
        }

        public void EnterLobby(Player user)
        {
            // 입장 처리
            _users.TryAdd(user.Id, user);

            // 들어온 유저에게 기존 유저들 알리고
            // 기존 유저들에게도 들어온 유저 알리기
            S_EnterLobby enterLobbyPacket = new S_EnterLobby();
            foreach (Player u in _users.Values)
            {
                if (u == null)
                    continue;
                
                enterLobbyPacket.UserIdList.Add(u.Id);
            }
            Broadcast(enterLobbyPacket);
        }

        public void LeaveLobby(int userId)
        {
            // 퇴장 처리
            if (_users.Remove(userId) == false)
            {
                ConsoleLogManager.Instance.Log($"Not Exist UserId: {userId}");
                return;
            }
            
            S_LeaveLobby leaveLobbyPacket = new S_LeaveLobby();
            leaveLobbyPacket.UserId = userId;
            Broadcast(leaveLobbyPacket);
        }

        public Player FindPlayer(Func<GameObject, bool> condition)
        {
            foreach (Player user in _users.Values)
            {
                if (condition.Invoke(user))
                    return user;
            }

            return null;
        }

        public void Broadcast(IMessage packet)
        {
            foreach (Player user in _users.Values)
            {
                user.Session.Send(packet);
            }
        }

        public void Update()
        {
            Flush();
        }
    }
}