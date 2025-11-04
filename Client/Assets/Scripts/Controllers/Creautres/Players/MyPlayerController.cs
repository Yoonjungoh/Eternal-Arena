using Google.Protobuf.Protocol;
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
    [SerializeField] private float _velocityChangeThreshold = 0.1f;
    [SerializeField] private float _rotationChangeThreshold = 2.0f;

    private Vector3 _prevVelocity = Vector3.zero;
    private Quaternion _prevRotation = Quaternion.identity;

    private Transform _cameraTransform;

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

        // 이동 처리
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_moveDir), Time.deltaTime * _rotateSpeed);
        transform.position += _moveDir * Time.deltaTime * _moveSpeed;
        CreatureState = CreatureState.Move;

        // 속도 및 회전 변화가 임계값을 넘을 때만 전송
        Vector3 curPos = transform.position;
        Vector3 curVelocity = (curPos - _prevPosition) / Time.deltaTime;
        Quaternion curRotation = transform.rotation;

        bool velocityChanged = (curVelocity - _prevVelocity).magnitude > _velocityChangeThreshold;
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
            // 여전히 이전 포지션만 업데이트하여 다음 프레임 비교에 사용
            _prevPosition = curPos;
        }
    }

    private void SendMovePacket(Vector3 velocity)
    {
        Debug.Log($"SendMovePacket: Pos({transform.position.x}, {transform.position.y}, {transform.position.z}) " +
            $"Vel({velocity.x}, {velocity.y}, {velocity.z}) " +
            $"Rot({transform.rotation.x}, {transform.rotation.y}, {transform.rotation.z}, {transform.rotation.w}) " +
            $"State({CreatureState})");
        C_Move movePacket = new C_Move();
        movePacket.ObjectState = new ObjectState()
        {
            ObjectId = Id,
            ClientSendTime = Util.GetTimestampMs(),
            Position = new ProtoVector3 { X = transform.position.x, Y = transform.position.y, Z = transform.position.z },
            Velocity = new ProtoVector3 { X = velocity.x, Y = velocity.y, Z = velocity.z },
            Rotation = new ProtoQuaternion { X = transform.rotation.x, Y = transform.rotation.y, Z = transform.rotation.z, W = transform.rotation.w },
            CreatureState = CreatureState
        };

        Managers.Network.Send(movePacket);
    }

    private void Start()
    {
        Init();
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
