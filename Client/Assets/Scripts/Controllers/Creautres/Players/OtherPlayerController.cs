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
        base.UpdateDeadReckoning();
    }
}
