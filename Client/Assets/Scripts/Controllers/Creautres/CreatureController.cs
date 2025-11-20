using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    // 서버 데이터
    protected float _lerpSpeed = 10f; // 데드 레커닝 보간 속도 조절
    protected Vector3 _serverPosition;
    protected Quaternion _serverRotation;
    protected Vector3 _serverVelocity;
    protected double _serverReceivedTimeMs = 0.0;  // 서버에서 패킷을 보낸 시간

    protected Animator _anim;
    protected Rigidbody _rb;
    protected Collider _collider;
    protected string _commonAttackanimName;
    protected UI_HpBar _hpBar;
    protected Vector3 _hpBarPosOffset;

    protected float _commonAttackAnimLength;
    protected float _commonAttackAnimSpeedTime = 1.0f;
    protected string _commonAttackHitEffectName;
    protected Vector3 _commonAttackHitEffectOffset;

    protected string _dieEffectName;
    protected Vector3 _dieEffectOffset;

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
    public GameObjectType GameObjectType { get; set; } = GameObjectType.None;

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

    protected ProtoVector3 _velocity = new ProtoVector3();
    public ProtoVector3 Velocity
    {
        get
        {
            _velocity.X = _serverVelocity.x;
            _velocity.Y = _serverVelocity.y;
            _velocity.Z = _serverVelocity.z;
            return _velocity;
        }
        set
        {
            _velocity = value;
            _serverVelocity = new Vector3(_velocity.X, _velocity.Y, _velocity.Z);
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

    public Stat Stat { get { return ObjectState.Stat; } set { ObjectState.Stat = value; } }

    protected AttackType _attackType;   // 공격 타입
    protected WaitForSeconds _waitCommonAttackReturn;

    protected virtual void OnUpdate()
    {
        switch (CreatureState)
        {
            case CreatureState.Move:
                UpdateMove();
                break;
            case CreatureState.Idle:
                UpdateIdle();
                break;
            case CreatureState.Attack:
                UpdateAttack();
                break;
        }
    }

    public virtual void Init()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        // 애니메이션 관련 초기화
        _commonAttackanimName = $"{AttackType.Common}_{CreatureState.Attack}";
        _commonAttackAnimLength = _anim.GetAnimationClipLength($"{AttackType.Common}_{CreatureState.Attack}") / _commonAttackAnimSpeedTime;
        _waitCommonAttackReturn ??= new WaitForSeconds(_commonAttackAnimLength);

        // 체력바 소환 (투사체는 이후에 조절)
        _hpBar = Managers.UI.MakeWorldSpaceUI<UI_HpBar>(transform, worldPositionStays: false);
        _hpBarPosOffset = Vector3.up * _collider.bounds.size.y;
        _hpBar.SetData(_hpBarPosOffset);
        _hpBar.UpdateHpBar(Stat.Hp, Stat.MaxHp);

        // 이펙트 관련 초기화
        _commonAttackHitEffectName = $"{AttackType.Common}_{CreatureState.Attack}HitEffect";
        _commonAttackHitEffectOffset = new Vector3(0, _collider.bounds.size.y / 2, 0);

        _dieEffectName = $"{CreatureState.Die}Effect";
        _dieEffectOffset = new Vector3(0, _collider.bounds.size.y / 2, 0);
    }

    protected virtual void UpdateMove() { }
    protected virtual void UpdateIdle() { }
    protected virtual void UpdateAttack() { }

    protected virtual void UpdateDeadReckoning()
    {
        double serverNowMs = Managers.Network.GetServerNowMs();
        double deltaSec = Mathf.Max(0f, (float)((serverNowMs - _serverReceivedTimeMs) / 1000.0));

        // XZ만 예측
        Vector3 predicted = _serverPosition;
        predicted.x += _serverVelocity.x * (float)deltaSec;
        predicted.z += _serverVelocity.z * (float)deltaSec;
        predicted.y = _serverPosition.y; // Y는 서버 포지션 고정

        transform.position = Vector3.Lerp(transform.position, predicted, Time.deltaTime * _lerpSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, _serverRotation, Time.deltaTime * _lerpSpeed);
    }

    public void SetServerState(ProtoVector3 pos, ProtoQuaternion rot, ProtoVector3 vel, long serverReceivedTime)
    {
        _serverPosition = new Vector3(pos.X, pos.Y, pos.Z);
        _serverRotation = new Quaternion(rot.X, rot.Y, rot.Z, rot.W);
        _serverVelocity = new Vector3(vel.X, vel.Y, vel.Z);
        _serverReceivedTimeMs = serverReceivedTime;
    }

    protected IEnumerator CoReturnToIdleAfterAttack(WaitForSeconds waitAttackReturn)
    {
        if (this == null || _anim == null)
            yield break;

        yield return waitAttackReturn;

        if (this == null || _anim == null)
            yield break;

        CreatureState = CreatureState.Idle;
        
        // 상태 변화 패킷 전송
        C_ChangeCreatureState changeCreatureStatePacket = new C_ChangeCreatureState();
        changeCreatureStatePacket.CreatureState = CreatureState;
        Managers.Network.Send(changeCreatureStatePacket);
    }

    protected virtual void OnDestroy()
    {
        StopAllCoroutines();
    }

    public virtual float OnDamaged(float remainHp)
    {
        // 히트 이펙트
        ParticleSystem particleSystem = Managers.Resource.SpawnEffect(
            _commonAttackHitEffectName,
            _commonAttackHitEffectOffset,
            new Quaternion(0, 0, 0, 0),
            worldPositionStays: false,
            transform).GetComponent<ParticleSystem>();

        float duration = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
        Managers.Resource.Destroy(particleSystem.gameObject, duration);

        // 데미지 계산
        float damage = Stat.Hp - remainHp;
        Stat.Hp -= damage;
        
        // 체력바 갱신
        _hpBar.UpdateHpBar(Stat.Hp, Stat.MaxHp);
        
        return damage;
    }

    protected virtual bool IsDead()
    {
        return Stat.Hp <= 0.0f;
    }

    public virtual void OnDead()
    {
        // 죽는 이펙트
        ParticleSystem particleSystem = Managers.Resource.SpawnEffect(
            _dieEffectName,
            _dieEffectOffset,
            new Quaternion(0, 0, 0, 0),
            worldPositionStays: false,
            transform).GetComponent<ParticleSystem>();

        float duration = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
        Managers.Resource.Destroy(particleSystem.gameObject, duration);

        // 죽은 오브젝트가 방해 안하게 하기
        _collider.isTrigger = true;
        _rb.isKinematic = true;
    }
}