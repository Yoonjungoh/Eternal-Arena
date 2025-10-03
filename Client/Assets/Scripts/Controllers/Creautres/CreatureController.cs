using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
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
            _creatureState = value;

            //Animator anim = GetComponent<Animator>();
            switch (_creatureState)
            {
                case CreatureState.Die:
                    break;
                case CreatureState.Idle:
                    //anim.CrossFade("WAIT", 0.1f);
                    break;
                case CreatureState.Move:
                    //anim.CrossFade("RUN", 0.1f);
                    break;
                case CreatureState.Attack:
                    //anim.CrossFade("ATTACK", 0.1f, -1, 0);
                    break;
            }
        }
    }

    void Update()
    {
        switch (CreatureState)
        {
            case CreatureState.Die:
                UpdateDie();
                break;
            case CreatureState.Move:
                UpdateMoving();
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

    public virtual void Init() { }

    protected virtual void UpdateDie() { }
    protected virtual void UpdateMoving() { }
    protected virtual void UpdateIdle() { }
    protected virtual void UpdateAttack() { }
    protected virtual void UpdateSkill() { }
}
