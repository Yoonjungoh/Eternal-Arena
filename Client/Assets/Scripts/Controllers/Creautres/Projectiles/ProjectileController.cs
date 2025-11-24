using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : BaseController
{
    public override void Init()
    {
        base.Init();
    }

    public override void OnDead()
    {
        Managers.Resource.Destroy(gameObject);
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        if (Managers.GameRoomObject.IsDamageable(other.gameObject.layer))
        {
            CreatureController creature = other.gameObject.GetComponent<CreatureController>();
            if (creature != null)
            {
                // 서버에 데미지 요청
                C_Attack attackPacket = new C_Attack();
                attackPacket.AttackType = AttackType.RangedAttack;
                attackPacket.InstigatorId = Id;
                attackPacket.DamagedObjectId = creature.Id;
                Managers.Network.Send(attackPacket);
            }
            return;
        }
    }
}
