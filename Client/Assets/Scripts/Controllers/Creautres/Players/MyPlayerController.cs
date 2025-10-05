using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerController : PlayerController
{
    [SerializeField] float _moveSpeed = 5.0f;
    [SerializeField] float _mouseRotationSpeed = 2.0f; // ���콺 ȸ�� ���� // TODO-���� �Ŵ����� ����

    private float _rotationX = 0f; // ���� ȸ�� (ī�޶�)
    private float _rotationY = 0f; // �¿� ȸ�� (ĳ����)
    private Transform _headTransform; // ������ ���� �Ӹ��� ���ư�

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
        // �̵� �Է� üũ
        bool hasInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
        
        // �̵� ó��
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

        // ȸ�� ó��
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

    // TODO - ��Ŷ ���� �ֱ� �����ϱ�
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