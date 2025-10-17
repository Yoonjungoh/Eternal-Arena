using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class WaitingRoomManager : JobSerializer
    {
        object _lock = new object();
        Dictionary<int, WaitingRoom> _rooms = new Dictionary<int, WaitingRoom>();
        int _roomId = 1;
        public Dictionary<int, WaitingRoom> Rooms { get { return _rooms; } }
        List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();
        
        public WaitingRoomManager(int lobbyId)
        {
            ConsoleLogManager.Instance.Log($"RoomManager created for Lobby {lobbyId}");
        }

        public void TickRoom(WaitingRoom room, int tick = 50)
        {
            var timer = new System.Timers.Timer();
            timer.Interval = tick;
            timer.Elapsed += ((s, e) => { room.Update(); });
            timer.AutoReset = true;
            timer.Enabled = true;
            _timers.Add(timer);
        }

        public WaitingRoom Add(int roomOwnerId, string roomName)
        {
            lock (_lock)
            {
                WaitingRoom newRoom = new WaitingRoom();
                newRoom.Push(newRoom.Init);
                TickRoom(newRoom);

                if (_rooms.ContainsKey(_roomId))
                {
                    ConsoleLogManager.Instance.Log($"That room already exist {_roomId}");
                    return null;
                }
                
                newRoom.RoomId = _roomId;
                newRoom.RoomName = roomName;
                newRoom.RoomOwnerId = roomOwnerId;
                _rooms.Add(_roomId, newRoom);
                _roomId++;
                
                return newRoom;
            }
        }

        public bool Remove(int roomId)
        {
            lock (_lock)
            {
                if (_rooms.ContainsKey(roomId))
                    return _rooms.Remove(roomId);
                else
                {
                    ConsoleLogManager.Instance.Log($"That room already removed {roomId}");
                    return false;
                }
            }
        }

        public WaitingRoom Find(int roomId)
        {
            lock (_lock)
            {
                WaitingRoom room = null;
                if (_rooms.TryGetValue(roomId, out room))
                    return room;
                return null;
            }
        }

        public void DisposeTimer()
        {
            foreach (var timer in _timers)
            {
                timer.Stop();
                timer.Dispose();
            }
            _timers.Clear();
        }
    }
}
