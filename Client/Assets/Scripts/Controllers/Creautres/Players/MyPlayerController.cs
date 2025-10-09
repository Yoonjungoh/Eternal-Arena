using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerController : PlayerController
{
    [SerializeField] float _moveSpeed = 5.0f;
    [SerializeField] float _mouseRotationSpeed = 2.0f; // 마우스 회전 감도 // TODO-설정 매니저로 빼기

    private float _rotationX = 0f; // 상하 회전 (카메라)
    private float _rotationY = 0f; // 좌우 회전 (캐릭터)
    private Transform _headTransform; // 상하일 때는 머리만 돌아감

    public override void Init()
    {
        base.Init();
        Managers.Input.KeyAction -= OnKeyBoard;
        Managers.Input.KeyAction += OnKeyBoard;
        Managers.Input.MouseAction -= OnMouseClicked;
        Managers.Input.MouseAction += OnMouseClicked;

        _headTransform = Util.FindChild(gameObject, "Head", recursive: true).transform;
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
        // 이동 입력 체크
        bool hasInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
        
        // 이동 처리
        if (hasInput)
        {
            Vector3 moveDir = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
            {
                moveDir += transform.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                moveDir += -transform.forward;
            }
            if (Input.GetKey(KeyCode.A))
            {
                moveDir += -transform.right;
            }
            if (Input.GetKey(KeyCode.D))
            {
                moveDir += transform.right;
            }

            moveDir.Normalize();
            transform.position += moveDir * Time.deltaTime * _moveSpeed;

            CreatureState = CreatureState.Move;
        }
        else
        {
            CreatureState = CreatureState.Idle;
        }

        // 회전 처리
        if (Input.GetMouseButton(1) && _headTransform)
        {
            float mouseX = Input.GetAxis("Mouse X") * _mouseRotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseRotationSpeed;
            
            _rotationY += mouseX;
            transform.rotation = Quaternion.Euler(0f, _rotationY, 0f);

            _rotationX -= mouseY;
            _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);
            
            _headTransform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
        }
    }

    private void Update()
    {
        base.OnUpdate();
        SendMovePacket();
    }

    // TODO - 패킷 전송 주기 조절하기
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