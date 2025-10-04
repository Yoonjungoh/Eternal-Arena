using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Lobby;
        // TODO - UI_Lobby 어드레서블로 불러오기
    }

    private void Awake()
    {
        Init();
    }
}
