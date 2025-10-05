using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CreatureController
{
    [SerializeField] protected float _transitionTime = 0.1f; // 애니메이션 전환 시간
    private CreatureState _lastAnimState = CreatureState.Idle;

    public override void Init()
    {
        base.Init();
        GameObjectType = GameObjectType.Player;
    }

    // 매 프레임 CrossFade 호출 방지 용도
    protected override void UpdateIdle()
    {
        base.UpdateIdle();
        if (_lastAnimState != CreatureState.Idle)
        {
            _anim.CrossFade("Idle", _transitionTime);
            _lastAnimState = CreatureState.Idle;
        }
    }

    protected override void UpdateMove()
    {
        base.UpdateMove();
        if (_lastAnimState != CreatureState.Move)
        {
            _anim.CrossFade("Move", _transitionTime);
            _lastAnimState = CreatureState.Move;
        }
    }

}
