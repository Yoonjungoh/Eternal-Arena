using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Server.Game
{
	public class ObjectManager
	{
		public static ObjectManager Instance { get; } = new ObjectManager();

		private object _lock = new object();
        private Dictionary<int, Player> _players = new Dictionary<int, Player>();

        // [UNUSED(1)][TYPE(7)][ID(24)]
        private int _counter = 0;

		public T Add<T>() where T : GameObject, new()
		{
			T gameObject = new T();

			lock (_lock)
			{
                try
                {
                    gameObject.Id = GenerateId(gameObject.ObjectType);

					if (gameObject.ObjectType == GameObjectType.Player)
					{
						_players.Add(gameObject.Id, gameObject as Player);
                    }
					// 네이밍
                    gameObject.ObjectState.Name = $"{gameObject.ObjectType}_{gameObject.ObjectState.ObjectId}";

                }
				catch (Exception e)
                {
                    ConsoleLogManager.Instance.Log(e);
                    ConsoleLogManager.Instance.Log("Dictionary Key-Value Problem");	
                }
			}

			return gameObject;
		}

		public int GenerateId(GameObjectType type)
		{
			lock (_lock)
			{
				return ((int)type << 24) | (_counter++);
			}
		}

		public GameObjectType GetObjectTypeById(int id)
		{
			int type = (id >> 24) & 0x7F;
			return (GameObjectType)type;
		}

		public bool Remove(int objectId)
		{
			GameObjectType objectType = GetObjectTypeById(objectId);

			lock (_lock)
			{
				if (objectType == GameObjectType.Player)
					return _players.Remove(objectId);
			}

			return false;
		}

		public T Find<T>(int objectId) where T : GameObject
		{
			GameObjectType objectType = GetObjectTypeById(objectId);

			lock (_lock)
			{
				if (objectType == GameObjectType.Player)
				{
					Player player = null;
					if (_players.TryGetValue(objectId, out player))
						return player as T;
				}
			}

			return null;
		}
	}
}
