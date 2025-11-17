using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : UI_Scene
{
    enum Buttons
    {
        EnterLobbyButton,
        ExitGameButton,
    }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        GetButton((int)Buttons.EnterLobbyButton).onClick.AddListener(OnClickEnterLobbyButton);
        GetButton((int)Buttons.ExitGameButton).onClick.AddListener(OnClickExitGameButton);
    }

    private void OnClickEnterLobbyButton()
    {
        Managers.Scene.LoadScene(Define.Scene.Lobby);
    }
    private void OnClickExitGameButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // 에디터 재생 종료
#else
    Application.Quit();                                // 빌드에서 게임 종료
#endif
    }
}
