using System.Collections;
using System.Collections.Generic;
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

    private void Update()
    {
        OnUpdate();
    }
}
