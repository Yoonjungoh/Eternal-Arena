using Google.Protobuf.Protocol;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Server.Game
{
    public class Projectile : GameObject
    {
        public bool hasDealtDamage { get; set; } = false;
        public long SpawnTime { get; set; }
        public float LifeTime { get; set; } = 3000.0f; // Ms

        public Projectile()
        {
            ObjectType = GameObjectType.Projectile;

            CreatureState = CreatureState.Move;
        }
    }
}
