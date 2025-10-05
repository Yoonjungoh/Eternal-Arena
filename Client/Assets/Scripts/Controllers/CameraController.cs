using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Define.CameraMode Mode;

    [SerializeField] Vector3 _delta = new Vector3(0.0f, 0.0f, 0.0f);

    private Transform _target;

    void LateUpdate()
    {
        if (Mode == Define.CameraMode.FirstPersonView && _target != null)
        {
            transform.position = _target.position + _delta;
            transform.rotation = _target.rotation;
        }
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
