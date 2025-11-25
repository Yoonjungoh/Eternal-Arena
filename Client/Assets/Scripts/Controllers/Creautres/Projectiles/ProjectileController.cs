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

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    protected override void UpdateMove()
    {
        base.UpdateMove();

        // 서버에서 받은 Velocity로 이동
        Vector3 moveVelocity = new Vector3(Velocity.X, Velocity.Y, Velocity.Z);
        transform.position += moveVelocity * Time.deltaTime;
        Debug.Log($"{moveVelocity.magnitude} => ({transform.position.x}, {transform.position.y} ,{transform.position.z} )");
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        int ownerId = Managers.GameRoomObject.GetProjectileOwnerId(Id);
        if (ownerId == -1)
            return;

        CreatureController creature = other.gameObject.GetComponent<CreatureController>();

        // 타깃이 주인이 아니고 데미지를 입는 레이어의 오브젝트여야 함
        if (creature != null && creature.Id != ownerId &&
            Managers.GameRoomObject.IsDamageable(other.gameObject.layer))
        {
            // 서버에 데미지 요청
            C_Attack attackPacket = new C_Attack();
            attackPacket.AttackType = AttackType.RangedAttack;
            attackPacket.InstigatorId = Id;
            attackPacket.DamagedObjectId = creature.Id;
            Managers.Network.Send(attackPacket);
            return;
        }

        // 다른 곳에 닿으면 그냥 삭제
        Managers.GameRoomObject.Remove(Id);
    }

    public override void OnDead()
    {
        Managers.Resource.Destroy(gameObject);
    }
}
