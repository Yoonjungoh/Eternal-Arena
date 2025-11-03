using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerController : PlayerController
{
    [SerializeField] float _moveSpeed = 5.0f;    // 이동 속도
    [SerializeField] float _rotateSpeed = 10.0f; // 회전 속도

    private Vector3 _moveDir = Vector3.zero;
    private Transform _cameraTransform;
    public float RotateSpeed { get { return _rotateSpeed; } }

    public override void Init()
    {
        base.Init();

        _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        base.OnUpdate();
        OnKeyBoardUpdate();
    }

    private void OnKeyBoardUpdate()
    {
        _moveDir = Vector3.zero;

        // 기존에는 Input.anyKey로 조기 리턴하여 서버에 멈춤 상태가 전송되지 않는 경우가 있어 제거했습니다.

        // 카메라 기준 벡터
        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 이동 방향 설정
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

        // Move -> Idle
        // 움직이다 멈춘 경우
        if (_moveDir.sqrMagnitude < 0.01f)
        {
            // 멈춘 상태는 반드시 서버에 전송하여 다른 클라이언트가 올바른 상태를 보도록 함
            if (CreatureState != CreatureState.Idle)
            {
                CreatureState = CreatureState.Idle;
                SendMovePacket();
            }
            return;
        }

        // 이동 및 회전
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_moveDir), Time.deltaTime * _rotateSpeed);
        transform.position += _moveDir * Time.deltaTime * _moveSpeed;

        CreatureState = CreatureState.Move;

        // 데드 레커닝 정보 전송
        if (HasMoveInput())
        {
            SendMovePacket();
        }
    }

    private void SendMovePacket()
    {
        C_Move movePacket = new C_Move();
        movePacket.ObjectState = ObjectState;
        movePacket.ObjectState.Position = Position;
        movePacket.ObjectState.Rotation = Rotation;
        movePacket.ObjectState.Velocity = Velocity;
        movePacket.ObjectState.CreatureState = CreatureState;
        Managers.Network.Send(movePacket);
    }

    // 패킷 생성
    private C_Move MakeMovePacket(Vector3 curVelocity, Quaternion curRotation, long timestampMs)
    {
        C_Move movePacket = new C_Move();
        movePacket.ObjectState = ObjectState;
        movePacket.ObjectState.Position = Position;
        movePacket.ObjectState.Rotation = new ProtoQuaternion
        {
            X = curRotation.x,
            Y = curRotation.y,
            Z = curRotation.z,
            W = curRotation.w
        };
        movePacket.ObjectState.Velocity = new ProtoVector3
        {
            X = curVelocity.x,
            Y = curVelocity.y,
            Z = curVelocity.z
        };
        movePacket.ObjectState.Timestamp = timestampMs;
        movePacket.ObjectState.CreatureState = CreatureState;
        Debug.Log(CreatureState);

        return movePacket;
    }
    private void Start()
    {
        Init();
    }

    private bool HasMoveInput()
    {
        return Input.GetKey(KeyCode.W) ||
               Input.GetKey(KeyCode.A) ||
               Input.GetKey(KeyCode.S) ||
               Input.GetKey(KeyCode.D);
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
