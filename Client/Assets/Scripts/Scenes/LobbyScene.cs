using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        
        // 로비 입장 처음에만 커넥팅 시도
        if (Managers.Network.IsInitialized == false)
        {
            StartCoroutine(Managers.Network.CoDownloadServerURL(() => Managers.UI.ShowSceneUI<UI_Lobby>()));
        }
        else
        {
            Managers.UI.ShowSceneUI<UI_Lobby>();
        }
    }

    private void Awake()
    {
        Init();
    }
}
