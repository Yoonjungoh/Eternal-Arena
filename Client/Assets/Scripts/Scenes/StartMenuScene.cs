using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuScene : BaseScene
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
