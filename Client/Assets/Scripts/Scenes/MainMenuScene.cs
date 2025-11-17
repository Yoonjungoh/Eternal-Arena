using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        // 처음만 메인 메뉴 세팅
        Managers.Scene.CurrentScene = Define.Scene.MainMenu;
        Managers.UI.ShowSceneUI<UI_MainMenu>();
    }

    private void Awake()
    {
        Init();
    }
}
