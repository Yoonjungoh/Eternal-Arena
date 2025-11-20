using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.Protocol;

namespace Server.Game.Object
{
    public class Bear : Monster
    {
        public Bear()
        {
            MonsterType = MonsterType.Bear;

            Stat.MaxHp = 50;
            Stat.Hp = Stat.MaxHp;
            Stat.CommonAttackDamage = 20;
            Stat.CommonAttackCoolTime = 3;
            Stat.AttackRange = 7;
            Stat.Defense = 0;
            Stat.MoveSpeed = 7;
        }
    }
}
