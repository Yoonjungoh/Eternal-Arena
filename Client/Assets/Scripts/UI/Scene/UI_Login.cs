using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Login : UI_Scene
{
    enum Buttons
    {
        LoginButton,
        ExitGameButton,
    }

    enum Texts
    {
        IdText,
        PasswordText,
    }

    private TextMeshProUGUI _idText;
    private TextMeshProUGUI _passwordText;

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        GetButton((int)Buttons.LoginButton).onClick.AddListener(OnClickLoginButton);
        GetButton((int)Buttons.ExitGameButton).onClick.AddListener(OnClickExitGameButton);
        Managers.Input.RegisterKeyAction(KeyCode.Return, OnClickLoginButton);

        Bind<TextMeshProUGUI>(typeof(Texts));
        _idText = GetTextMeshProUGUI((int)Texts.IdText);
        _passwordText = GetTextMeshProUGUI((int)Texts.PasswordText);
    }

    private void OnClickLoginButton()
    {
        // TODO - 무한 패킷 발사 방지하기 위해 전송 주기 타이머 넣기
        C_Login loginPacket = new C_Login();
        loginPacket.Id = _idText.text;
        loginPacket.Password = _passwordText.text;
        Managers.Network.Send(loginPacket);
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
