using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;

public class MyPlayerController : PlayerController
{
    [SerializeField] private float _moveSpeed = 5.0f;
    [SerializeField] private float _rotateSpeed = 10.0f;
    public float RotateSpeed { get { return _rotateSpeed; } }

    private Vector3 _moveDir = Vector3.zero;
    private Vector3 _prevPosition;
    
    [SerializeField] private float _stopThreshold = 0.01f;

    // 데드 레커닝 패킷 제한에 쓸 임계값
    private float _velocityChangeThreshold = 1.0f;
    private float _rotationChangeThreshold = 2.0f;

    private Vector3 _prevVelocity = Vector3.zero;
    private Quaternion _prevRotation = Quaternion.identity;

    private Transform _cameraTransform;

    public override void Init()
    {
        base.Init();
        _cameraTransform = Camera.main.transform;
        _prevPosition = transform.position;
        _prevRotation = transform.rotation;
        Managers.Input.RegisterKeyAction(KeyCode.K, () => Attack());
    }

    private void Update()
    {
        base.OnUpdate();
        HandleInput();
    }

    private void FixedUpdate()
    {
        HandlePhysicsMovement();    // 물리 기반 이동 처리
        ApplyMovement();    // 실제 인풋에 따른 이동 처리
        CheckMovePacket(); // 이동 감지 및 패킷 전송
    }

    private void Attack()
    {
        C_Attack attackPacket = new C_Attack();
        attackPacket.AttackType = AttackType.Common;
        Managers.Network.Send(attackPacket);
    }

    private void HandleInput()
    {
        _moveDir = Vector3.zero;

        // 카메라 방향 기준
        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();

        if (Input.GetKey(KeyCode.W)) _moveDir += camForward;
        if (Input.GetKey(KeyCode.S)) _moveDir -= camForward;
        if (Input.GetKey(KeyCode.A)) _moveDir -= camRight;
        if (Input.GetKey(KeyCode.D)) _moveDir += camRight;

        _moveDir.Normalize();

        // Idle 상태 (멈춤 전송 보장)
        if (_moveDir.sqrMagnitude < _stopThreshold)
        {
            if (CreatureState != CreatureState.Idle)
            {
                CreatureState = CreatureState.Idle;
                // 보낼 때는 명시적으로 0 벡터 전송
                SendMovePacket(Vector3.zero);
                // 업데이트 이전 상태
                _prevVelocity = Vector3.zero;
                _prevRotation = transform.rotation;
                _prevPosition = transform.position;
            }
            return;
        }

        // 회전 및 상태 처리
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_moveDir), Time.deltaTime * _rotateSpeed);
        CreatureState = CreatureState.Move;
    }

    private void ApplyMovement()
    {
        if (_rb == null)
            return;

        // 이동 방향이 없는 경우 무시
        if (_moveDir.sqrMagnitude < _stopThreshold)
            return;

        // 현재 위치 + 이동량 (FixedDeltaTime 사용!)
        Vector3 newPosition = _rb.position + _moveDir * _moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(newPosition);

        // 회전도 MoveRotation으로
        Quaternion targetRot = Quaternion.LookRotation(_moveDir);
        _rb.MoveRotation(Quaternion.Lerp(_rb.rotation, targetRot, _rotateSpeed * Time.fixedDeltaTime));

        CreatureState = CreatureState.Move;
    }

    // 속도가 의미 있을 때만 패킷 전송 (무언가에 의해서 밀리거나 낙하 중일 때)
    private void HandlePhysicsMovement()
    {
        Vector3 velocity = _rb.velocity;

        bool wasFalling = _prevVelocity.y < -0.01f; // 이전 프레임에서 낙하 중이었는지
        bool isNearlyStopped = velocity.sqrMagnitude <= 0.0001f; // 지금 거의 멈췄는지

        // 낙하 중인 경우 (속도 유의미)
        if (velocity.sqrMagnitude > 0.0001f)
        {
            SendMovePacket(velocity);
        }
        // 착지한 경우 (처음으로 멈춘 순간)
        else if (wasFalling && isNearlyStopped)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);  // Y축 속도 제거
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
        Vector3 pos = _rb.position;  // 실제 물리 위치 사용 (transform.position은 렌더링 프레임에서 보간된 값이므로 미세한 차이 존재)
        Quaternion rot = _rb.rotation;

        C_Move movePacket = new C_Move();
        movePacket.ObjectState = new ObjectState()
        {
            ObjectId = Id,
            ClientSendTime = Util.GetTimestampMs(),
            Position = new ProtoVector3 { X = pos.x, Y = pos.y, Z = pos.z },
            Velocity = new ProtoVector3 { X = velocity.x, Y = velocity.y, Z = velocity.z },
            Rotation = new ProtoQuaternion { X = rot.x, Y = rot.y, Z = rot.z, W = rot.w },
            CreatureState = CreatureState,
            Stat = Stat,
        };

        Managers.Network.Send(movePacket);
    }

    private void Start()
    {
        Init();
    }


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

    private void OnMouseClicked(Define.MouseEvent evt)
    {
        if (evt != Define.MouseEvent.Click)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(Camera.main.transform.position, ray.direction * 100.0f, Color.red, 1.0f);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            Debug.Log($"Raycast Camera @ {hit.collider.gameObject.name}");
        }
    }
}
