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

        // 입력 체크
        if (Input.anyKey == false)
        {
            CreatureState = CreatureState.Idle;
            return;
        }

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
            CreatureState = CreatureState.Idle;
            SendMovePacket();
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
