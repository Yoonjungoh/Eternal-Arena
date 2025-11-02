using UnityEngine;

public class OtherPlayerController : PlayerController
{
    public override void Init()
    {
        base.Init();
    }

    private void Start()
    {
        Init();
    }

    private void FixedUpdate()
    {
        base.OnUpdate();
    }

    protected override void UpdateMove()
    {
        UpdateDeadReckoningMove();
    }

    protected override void UpdateDeadReckoningMove()
    {
        float delta = Time.time - _lastReceiveTime;

        // 예측 위치 (Dead Reckoning)
        Vector3 predictedPos = _serverPosition + _lastReceivedVelocity * delta;

        transform.position = Vector3.Lerp(transform.position, predictedPos, Time.deltaTime * _lerpSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, _serverRotation, Time.deltaTime * _lerpSpeed);
    }
}
