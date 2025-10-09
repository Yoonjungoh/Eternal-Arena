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
        
        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        public void Init()
        {
            // TODO
        }

        public void EnterLobby(Player player)
        {
            // 입장 처리
            _players.TryAdd(player.Id, player);

            // 들어온 유저에게 기존 유저들 알리고
            // 기존 유저들에게도 들어온 유저 알리기
            S_EnterLobby enterLobbyPacket = new S_EnterLobby();
            foreach (Player p in _players.Values)
            {
                if (p == null)
                    continue;
                
                enterLobbyPacket.UserIdList.Add(p.Id);
            }
            Broadcast(enterLobbyPacket);
        }

        public Player FindPlayer(Func<GameObject, bool> condition)
        {
            foreach (Player player in _players.Values)
            {
                if (condition.Invoke(player))
                    return player;
            }

            return null;
        }

        public void Broadcast(IMessage packet)
        {
            foreach (Player player in _players.Values)
            {
                player.Session.Send(packet);
            }
        }

        public void Update()
        {
            Flush();
        }
    }
}