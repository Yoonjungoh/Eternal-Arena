using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    protected Animator _anim;
    public ObjectState ObjectState { get; set; } = new ObjectState();
    public int Id { get { return ObjectState.ObjectId; } set { ObjectState.ObjectId = value; } }
    public CreatureState CreatureState 
    { 
        get { return ObjectState.CreatureState; } 
        set
        {
            if (ObjectState.CreatureState == value)
                return;

            ObjectState.CreatureState = value;
        }
    }
    public GameObjectType GameObjectType;

    protected ProtoVector3 _position = new ProtoVector3();
    public ProtoVector3 Position
    {
        get
        {
            _position.X = transform.position.x;
            _position.Y = transform.position.y;
            _position.Z = transform.position.z;
            return _position; 
        }
        set
        {
            _position = value;
            transform.position = new Vector3(_position.X, _position.Y, _position.Z);        
        }
    }
    
    protected ProtoQuaternion _rotation = new ProtoQuaternion();
    public ProtoQuaternion Rotation
    {
        get
        {
            _rotation.X = transform.rotation.x;
            _rotation.Y = transform.rotation.y;
            _rotation.Z = transform.rotation.z;
            _rotation.W = transform.rotation.w;
            return _rotation;
        }
        set
        {
            _rotation = value;
            transform.rotation = new Quaternion(_rotation.X, _rotation.Y, _rotation.Z, _rotation.W);
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
