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

        Managers.Input.KeyAction -= OnKeyBoard;
        Managers.Input.KeyAction += OnKeyBoard;

        Managers.Input.MouseAction -= OnMouseClicked;
        Managers.Input.MouseAction += OnMouseClicked;

        _cameraTransform = Camera.main.transform;
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

    private void OnKeyBoard()
    {
        _moveDir = Vector3.zero;

        // 카메라 방향 기준 벡터 구하기
        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;

        // 수평 회전만 반영 (상하는 제외)
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 입력에 따라 이동 방향 결정
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            _moveDir += camForward;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            _moveDir -= camForward;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            _moveDir -= camRight;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            _moveDir += camRight;
        }

        _moveDir.Normalize();

        // 이동, 회전 처리
        if (_moveDir != Vector3.zero)
        {
            // 캐릭터가 바라보는 방향을 이동 방향으로 회전
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_moveDir), Time.deltaTime * _rotateSpeed);

            // 이동
            transform.position += _moveDir * Time.deltaTime * _moveSpeed;
            CreatureState = CreatureState.Move;
        }
        else
        {
            CreatureState = CreatureState.Idle;
        }
    }

    private void Update()
    {
        base.OnUpdate();
        SendMovePacket();
        Debug.Log(CreatureState);
    }

    // TODO - 패킷 주기 전송 최적화
    private void SendMovePacket()
    {
        C_Move movePacket = new C_Move();
        movePacket.PositionInfo = PositionInfo;
        Managers.Network.Send(movePacket);
    }

    private void Start()
    {
        Init();
    }
}
