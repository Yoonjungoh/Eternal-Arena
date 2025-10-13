using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatingRoomScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.WaitingRoom;
        Util.GetOrAddComponent<ConsoleController>(Camera.main.gameObject);
    }

    void Awake()
    {
        Init();
    }
}
