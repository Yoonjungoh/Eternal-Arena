using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Define.CameraMode Mode= Define.CameraMode.CommonView;

    [Header("View Settings - QuarterView")]
    public Vector3 QuarterViewOffset = new Vector3(0, 10, -8);  // 쿼터뷰 시점 거리
    public float QuarterViewFollowSpeed = 5f;                   // 따라가는 속도
    public Vector3 QuarterViewRotation = new Vector3(0, 0, 0);                // 고정용 특정 각도

    [Header("View Settings - FirstPersonView")]
    public Vector3 FirstPersonViewOffset = new Vector3(0, 10, -8);  // 쿼터뷰 시점 거리
    public float FirstPersonViewFollowSpeed = 5f;                   // 따라가는 속도
    public float FirstPersonViewLookDownAngle = 45f;                // 아래로 내려보는 각도

    private Transform _target;

    void LateUpdate()
    {
        if (Mode == Define.CameraMode.CommonView && _target != null)
        {
            // 특정 좌표에서 가만히 있을 예정
        }
        else if (Mode == Define.CameraMode.QuarterView && _target != null)
        {
            Vector3 desiredPosition = _target.position + QuarterViewOffset;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, QuarterViewFollowSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(QuarterViewRotation);
        }
        else if (Mode == Define.CameraMode.FirstPersonView && _target != null)
        {
            transform.position = _target.position + FirstPersonViewOffset;
            transform.rotation = _target.rotation;
        }
    }

    public void SetCommonView()
    {
        if (_target == null)
        {
            _target = Util.FindChild(Managers.Object.MyPlayer.gameObject, "Head", recursive: true).transform;
        }
        Mode = Define.CameraMode.CommonView;
    }

    public void SetQuarterView()
    {
        if (_target == null)
        {
            _target = Util.FindChild(Managers.Object.MyPlayer.gameObject, "Head", recursive: true).transform;
        }
        Mode = Define.CameraMode.QuarterView;
    }

    public void SetFirstPersonView()
    {
        if (_target == null)
        {
            _target = Util.FindChild(Managers.Object.MyPlayer.gameObject, "Head", recursive: true).transform;
        }
        Mode = Define.CameraMode.FirstPersonView;
    }
}
