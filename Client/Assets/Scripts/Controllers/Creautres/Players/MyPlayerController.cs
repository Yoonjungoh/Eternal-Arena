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

        Managers.Input.MouseAction -= OnMouseClicked;
        Managers.Input.MouseAction += OnMouseClicked;

        _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        base.OnUpdate();
        OnKeyBoardUpdate();
        SendMovePacket();
    }


    private void OnKeyBoardUpdate()
    {
        _moveDir = Vector3.zero;

        // 카메라 기준 벡터
        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 입력 체크
        if (Input.anyKey == false)
        {
            CreatureState = CreatureState.Idle;
            return;
        }

        if (Input.GetKey(KeyCode.W)) _moveDir += camForward;
        if (Input.GetKey(KeyCode.S)) _moveDir -= camForward;
        if (Input.GetKey(KeyCode.A)) _moveDir -= camRight;
        if (Input.GetKey(KeyCode.D)) _moveDir += camRight;

        _moveDir.Normalize();

        if (_moveDir.sqrMagnitude < 0.01f)
        {
            CreatureState = CreatureState.Idle;
            return;
        }

        // 이동 및 회전
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_moveDir), Time.deltaTime * _rotateSpeed);
        transform.position += _moveDir * Time.deltaTime * _moveSpeed;

        CreatureState = CreatureState.Move;
    }

    private void SendMovePacket()
    {
        C_Move movePacket = new C_Move();
        movePacket.ObjectState = ObjectState;
        movePacket.ObjectState.Position = Position; // 복사 현상 때문에 이렇게 넣어주기
        movePacket.ObjectState.Rotation = Rotation; // 복사 현상 때문에 이렇게 넣어주기
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
