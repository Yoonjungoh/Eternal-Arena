using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        Util.GetOrAddComponent<ConsoleController>(Camera.main.gameObject);
    }
    void Awake()
    {
        Init();
    }
}
