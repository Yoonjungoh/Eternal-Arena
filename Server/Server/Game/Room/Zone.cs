using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Zone
    {
        public int IndexX { get; private set; }
        
        public int IndexZ { get; private set; }

        public HashSet<Player> Players { get; private set; } = new HashSet<Player>();
        public HashSet<Monster> Monsters { get; private set; } = new HashSet<Monster>();
        public HashSet<Projectile> Projectiles { get; private set; } = new HashSet<Projectile>();

        public Zone(int x, int z) 
        {
            IndexX = x;
            IndexZ = z;
        }
    }
}
