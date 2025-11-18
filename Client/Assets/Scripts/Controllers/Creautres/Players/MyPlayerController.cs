using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;
using Unity.Profiling;


public class MyPlayerController : PlayerController
{
    [SerializeField] private float _rotateSpeed = 10.0f;
    public float RotateSpeed { get { return _rotateSpeed; } }

    private Vector3 _moveDir = Vector3.zero;
    private Vector3 _prevPosition;
    private Vector3 _prevVelocity = Vector3.zero;
    private Quaternion _prevRotation = Quaternion.identity;
    private Transform _cameraTransform;

    private float _velocityChangeThreshold = 1.0f;
    private float _rotationChangeThreshold = 2.0f;
    private float _stopThreshold = 0.01f;

    private float _lastAttackTime = -999f;


    #region 패킷 캐싱용 필드
    private readonly C_Move _movePacket = new C_Move();
    private readonly ObjectState _moveState = new ObjectState();
    private readonly ProtoVector3 _movePos = new ProtoVector3();
    private readonly ProtoVector3 _moveVel = new ProtoVector3();
    private readonly ProtoQuaternion _moveRot = new ProtoQuaternion();
    #endregion

    private void OnAttackInput() => Attack(AttackType.Common);

    public override void Init()
    {
        base.Init();
        _cameraTransform = Camera.main.transform;
        _prevPosition = transform.position;
        _prevRotation = transform.rotation;
    }

    private void Update()
    {
        base.OnUpdate();
        HandleInput();
    }

    private void FixedUpdate()
    {
        HandlePhysicsMovement();
        ApplyMovement();
        CheckMovePacket();
    }

    private void Attack(AttackType attackType)
    {
        if (Managers.Scene.CurrentScene != Define.Scene.GameRoom)
            return;
        
        // Idle일때만 공격 시전
        if (CreatureState != CreatureState.Idle)
            return;

        // 공격 시간 쿨타임 계산
        if (Time.time - _lastAttackTime < Stat.CommonAttackCoolTime)
            return;
        
        _lastAttackTime = Time.time;
        CreatureState = CreatureState.Attack;

        // 서버로 공격 패킷 전송
        C_Attack attackPacket = new C_Attack();
        attackPacket.AttackType = attackType;
        Managers.Network.Send(attackPacket);
        StartCoroutine(CoReturnToIdleAfterAttack(_waitCommonAttackReturn));
    }
    
    private void HandleInput()
    {
        // 카운트다운 중일 때는 입력 거부
        if (Managers.Scene.CurrentScene == Define.Scene.GameRoom && Managers.GameRoom.IsCountdownFinished == false)
            return;

        // 공격 중일 때는 인풋 받는 거 불가
        if (CreatureState == CreatureState.Attack)
            return;

        _moveDir = Vector3.zero;

        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();

        if (Input.GetKey(KeyCode.W))
        {
            _moveDir += camForward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            _moveDir -= camForward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            _moveDir -= camRight;
        }
        if (Input.GetKey(KeyCode.D))
        {
            _moveDir += camRight;
        }

        _moveDir.Normalize();

        if (_moveDir.sqrMagnitude < _stopThreshold)
        {
            if (CreatureState != CreatureState.Idle)
            {
                CreatureState = CreatureState.Idle;
                SendMovePacket(Vector3.zero);
                _prevVelocity = Vector3.zero;
                _prevRotation = transform.rotation;
                _prevPosition = transform.position;
            }
            return;
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_moveDir), Time.deltaTime * _rotateSpeed);
        CreatureState = CreatureState.Move;
    }

    private void ApplyMovement()
    {
        if (_rb == null)
            return;

        if (_moveDir.sqrMagnitude < _stopThreshold)
            return;

        Vector3 newPosition = _rb.position + _moveDir * Stat.MoveSpeed * Time.fixedDeltaTime;
        // 충돌 추가 검사
        if (Managers.Map.CanGo(newPosition.x, newPosition.z))
        {
            _rb.MovePosition(newPosition);
        }

        Quaternion targetRot = Quaternion.LookRotation(_moveDir);
        _rb.MoveRotation(Quaternion.Lerp(_rb.rotation, targetRot, _rotateSpeed * Time.fixedDeltaTime));

        CreatureState = CreatureState.Move;
    }

    private void HandlePhysicsMovement()
    {
        Vector3 velocity = _rb.velocity;

        bool wasFalling = _prevVelocity.y < -0.01f;
        bool isNearlyStopped = velocity.sqrMagnitude <= 0.0001f;

        if (velocity.sqrMagnitude > 0.0001f)
        {
            SendMovePacket(velocity);
        }
        else if (wasFalling && isNearlyStopped)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            SendMovePacket(Vector3.zero);
        }
    }

    private void CheckMovePacket()
    {
        Vector3 curPos = _rb.position;
        Vector3 curVelocity = (curPos - _prevPosition) / Time.fixedDeltaTime;
        Quaternion curRotation = _rb.rotation;

        bool velocityChanged = (curVelocity - _prevVelocity).sqrMagnitude > (_velocityChangeThreshold * _velocityChangeThreshold);
        bool rotationChanged = Quaternion.Angle(curRotation, _prevRotation) > _rotationChangeThreshold;

        if (velocityChanged || rotationChanged)
        {
            SendMovePacket(curVelocity);
            _prevVelocity = curVelocity;
            _prevRotation = curRotation;
            _prevPosition = curPos;
        }
        else
        {
            _prevPosition = curPos;
        }
    }

    private void SendMovePacket(Vector3 velocity)
    {
        Vector3 pos = _rb.position;
        Quaternion rot = _rb.rotation;

        _movePos.X = pos.x; _movePos.Y = pos.y; _movePos.Z = pos.z;
        _moveVel.X = velocity.x; _moveVel.Y = velocity.y; _moveVel.Z = velocity.z;
        _moveRot.X = rot.x; _moveRot.Y = rot.y; _moveRot.Z = rot.z; _moveRot.W = rot.w;

        _moveState.ObjectId = Id;
        _moveState.ClientSendTime = Util.GetTimestampMs();
        _moveState.Position = _movePos;
        _moveState.Velocity = _moveVel;
        _moveState.Rotation = _moveRot;
        _moveState.CreatureState = CreatureState;
        _moveState.Stat = Stat;

        _movePacket.ObjectState = _moveState;

        Managers.Network.Send(_movePacket);
    }

    private void Start()
    {
        Init();
    }

    public void OnStartGame()
    {
        // 인게임에서만 공격 기능 활성화
        if (Managers.Scene.CurrentScene == Define.Scene.GameRoom)
        {
            Managers.Input.RegisterMouseAction(Define.MouseEvent.LeftClick, Managers.GameRoomObject.MyPlayer.OnAttackInput);
            _commonAttackAnimSpeedTime = 2.0f;  // 에디터에선 동적으로 가져올 수 있으나 런타임에선 불가능해서 하드코딩
            _commonAttackAnimLength = _anim.GetAnimationClipLength($"{AttackType.Common}_{CreatureState.Attack}") / _commonAttackAnimSpeedTime;
            _waitCommonAttackReturn = new WaitForSeconds(_commonAttackAnimLength);

            
        }
    }

    #region Gizmos 코드
    private void OnDrawGizmos()
    {
        if (Stat == null)
            return;
        Color gizmoColor = new Color(1f, 0.3f, 0f, 0.25f);
        Gizmos.color = gizmoColor;

        float range = Stat.AttackRange;
        float halfAngle = Stat.AttackHalfAngleDeg;
        float height = Stat.AttackHeight;

        if (range <= 0f)
            return;

        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        DrawCommonAttackCollision(origin, forward, range, halfAngle, height);
    }

    private void DrawCommonAttackCollision(Vector3 origin, Vector3 forward, float radius, float halfAngle, float height)
    {
        int segments = 30;
        float step = halfAngle * 2f / segments;
        Quaternion leftRot = Quaternion.AngleAxis(-halfAngle, Vector3.up);
        Vector3 prev = origin + leftRot * forward * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngle + step * i;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 next = origin + rot * forward * radius;
            Gizmos.DrawLine(origin, next);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }

        Gizmos.DrawLine(origin + Vector3.up * (height * 0.5f), origin - Vector3.up * (height * 0.5f));
    }
    #endregion

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
