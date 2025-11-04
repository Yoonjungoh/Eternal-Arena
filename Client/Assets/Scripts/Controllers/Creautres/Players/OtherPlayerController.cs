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
        base.UpdateMove();
        base.UpdateDeadReckoningMove();
    }
}
