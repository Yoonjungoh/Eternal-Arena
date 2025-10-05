using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CreatureController
{
    [SerializeField] protected float _transitionTime = 0.1f; // �ִϸ��̼� ��ȯ �ð�
    private CreatureState _lastAnimState = CreatureState.Idle;

    public override void Init()
    {
        base.Init();
        GameObjectType = GameObjectType.Player;
    }

    // �� ������ CrossFade ȣ�� ���� �뵵
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
