using Google.Protobuf.Protocol;
using UnityEngine;

public class MyPlayerController : PlayerController
{
    private const float ROT_THRESHOLD = 2.0f;
    private const float MOVE_THRESHOLD = 0.05f;

    [SerializeField] private float _rotateSpeed = 10.0f;
    public float RotateSpeed { get { return _rotateSpeed; } }

    private Vector3 _moveDir = Vector3.zero;
    private Vector3 _prevVelocity;
    private Quaternion _prevRotation;
    private Transform _cameraTransform;

    private float _lastAttackTime = -999f;

    private readonly C_Move _movePacket = new C_Move();
    private readonly ObjectState _moveState = new ObjectState();
    private readonly ProtoVector3 _movePos = new ProtoVector3();
    private readonly ProtoVector3 _moveVel = new ProtoVector3();
    private readonly ProtoQuaternion _moveRot = new ProtoQuaternion();
    
    private void OnAttackInput() => Attack(AttackType.Common);

    public override void Init()
    {
        base.Init();
        _cameraTransform = Camera.main.transform;

        _prevRotation = transform.rotation;
        _prevVelocity = Vector3.zero;

        if (Managers.Scene.CurrentScene == Define.Scene.GameRoom)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Start()
    {
        Init();
    }

    public void OnStartGame()
    {
        if (Managers.Scene.CurrentScene != Define.Scene.GameRoom)
            return;

        Managers.Input.RegisterMouseAction(
            Define.MouseEvent.LeftClick,
            Managers.GameRoomObject.MyPlayer.OnAttackInput
        );

        _commonAttackAnimSpeedTime = 2.0f;
        _commonAttackAnimLength =
            _anim.GetAnimationClipLength($"{AttackType.Common}_{CreatureState.Attack}") /
            _commonAttackAnimSpeedTime;

        _waitCommonAttackReturn = new WaitForSeconds(_commonAttackAnimLength);
    }

    private void Update()
    {
        base.OnUpdate();
        HandleInput();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckMovePacket();
    }

    private void HandleInput()
    {
        if (Managers.Scene.CurrentScene == Define.Scene.GameRoom &&
            Managers.GameRoom.IsCountdownFinished == false)
            return;

        if (CreatureState == CreatureState.Die || CreatureState == CreatureState.Attack)
            return;

        _moveDir = Vector3.zero;

        Vector3 f = _cameraTransform.forward;
        Vector3 r = _cameraTransform.right;
        f.y = 0; r.y = 0;
        f.Normalize(); r.Normalize();

        if (Input.GetKey(KeyCode.W)) _moveDir += f;
        if (Input.GetKey(KeyCode.S)) _moveDir -= f;
        if (Input.GetKey(KeyCode.A)) _moveDir -= r;
        if (Input.GetKey(KeyCode.D)) _moveDir += r;

        _moveDir.Normalize();

        if (_moveDir.sqrMagnitude < MOVE_THRESHOLD)
        {
            if (CreatureState != CreatureState.Idle)
            {
                CreatureState = CreatureState.Idle;
                SendMovePacket(Vector3.zero);
            }
            return;
        }

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.LookRotation(_moveDir),
            Time.deltaTime * _rotateSpeed
        );

        CreatureState = CreatureState.Move;
    }

    private void ApplyMovement()
    {
        if (_rb == null)
            return;

        if (_moveDir.sqrMagnitude < MOVE_THRESHOLD)
            return;

        Vector3 newPos = _rb.position + _moveDir * Stat.MoveSpeed * Time.fixedDeltaTime;

        if (Managers.Map.CanGo(newPos.x, newPos.z))
            _rb.MovePosition(newPos);

        Quaternion targetRot = Quaternion.LookRotation(_moveDir);
        _rb.MoveRotation( Quaternion.Lerp(_rb.rotation, targetRot, _rotateSpeed * Time.fixedDeltaTime));

        CreatureState = CreatureState.Move;
    }

    private void CheckMovePacket()
    {
        Vector3 curPos = _rb.position;
        Quaternion curRot = _rb.rotation;

        Vector3 curVelocity = _moveDir * Stat.MoveSpeed;

        bool rotChanged = Quaternion.Angle(curRot, _prevRotation) > ROT_THRESHOLD;
        bool velChanged = (curVelocity - _prevVelocity).sqrMagnitude > MOVE_THRESHOLD;

        if (rotChanged || velChanged)
        {
            SendMovePacket(curVelocity);
            _prevRotation = curRot;
            _prevVelocity = curVelocity;
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

    private void Attack(AttackType attackType)
    {
        if (Managers.Scene.CurrentScene != Define.Scene.GameRoom)
            return;

        if (CreatureState != CreatureState.Idle)
            return;

        if (Time.time - _lastAttackTime < Stat.CommonAttackCoolTime)
            return;

        _lastAttackTime = Time.time;
        CreatureState = CreatureState.Attack;

        C_Attack p = new C_Attack();
        p.AttackType = attackType;
        Managers.Network.Send(p);

        StartCoroutine(CoReturnToIdleAfterAttack(_waitCommonAttackReturn));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
