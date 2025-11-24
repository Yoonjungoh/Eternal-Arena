using Google.Protobuf.Protocol;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Server.Game
{
    public class Projectile : GameObject
    {
        public ProjectileType ProjectileType { get; set; }

        public Projectile()
        {
            ObjectType = GameObjectType.Projectile;

            CreatureState = CreatureState.Move;
        }
    }
}
