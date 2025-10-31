using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_WaitingRoom : UI_Scene
{
    enum Buttons
    {
        ExitButton,
        StartGameButton,
    }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        GetButton((int)Buttons.ExitButton).onClick.AddListener(OnClickExitRoomButton);
        GetButton((int)Buttons.StartGameButton).onClick.AddListener(OnClickStartGameButton);
    }

    private void OnClickExitRoomButton()
    {
        C_ExitRoom exitRoomPacket = new C_ExitRoom();
        Managers.Network.Send(exitRoomPacket);
    }

    private void OnClickStartGameButton()
    {
        Debug.Log("Start Game Button Clicked");
    }
}
