using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        // TODO - 원래 LoadScene에서 해주는데 지금은 여기서 하드코딩
        Managers.Scene.CurrentScene = Define.Scene.Lobby;
        // TODO - UI_Lobby 어드레서블로 불러오기
        Managers.UI.ShowSceneUI<UI_Lobby>();
        // TODO - TEST용
        Managers.Input.RegisterKeyAction(KeyCode.Space, () => Managers.Network.RequestServerTimeSync());
    }

    private void Awake()
    {
        Init();
    }
}
