using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        StartCoroutine(Managers.Network.CoDownloadServerURL(() => Managers.UI.ShowSceneUI<UI_Lobby>()));
    }

    private void Awake()
    {
        Init();
    }
}
