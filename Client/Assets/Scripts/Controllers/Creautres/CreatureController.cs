using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    protected Animator _anim;
    public int Id;
    protected CreatureState _creatureState = CreatureState.Idle;
    public GameObjectType GameObjectType;

    protected PositionInfo _positionInfo = new PositionInfo();
    public PositionInfo PositionInfo
    {
        get
        {
            _positionInfo.PosX = transform.position.x;
            _positionInfo.PosY = transform.position.y;
            _positionInfo.PosZ = transform.position.z;
            _positionInfo.RotY = transform.eulerAngles.y;
            return _positionInfo; 
        }
        set
        {
            _positionInfo = value;
            transform.position = new Vector3(_positionInfo.PosX, _positionInfo.PosY, _positionInfo.PosZ);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, _positionInfo.RotY, transform.eulerAngles.z);
        }
    }

    public virtual CreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            if (_creatureState == value)
                return;

            _creatureState = value;
        }
    }

    protected virtual void OnUpdate()
    {
        switch (CreatureState)
        {
            case CreatureState.Die:
                UpdateDie();
                break;
            case CreatureState.Move:
                UpdateMove();
                break;
            case CreatureState.Idle:
                UpdateIdle();
                break;
            case CreatureState.Attack:
                UpdateAttack();
                break;
            case CreatureState.Skill:
                UpdateSkill();
                break;
        }
    }

    public virtual void Init()
    {
        _anim = GetComponent<Animator>();
    }

    protected virtual void UpdateDie() { }
    protected virtual void UpdateMove() { }
    protected virtual void UpdateIdle() { }
    protected virtual void UpdateAttack() { }
    protected virtual void UpdateSkill() { }
}
