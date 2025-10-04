using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Lobby : UI_Scene
{
    enum Buttons
    {
        MakeRoomButton,
    }
    int i = 0;
    private GameObject _roomPanel;

    public override void Init()
    {
        base.Init();
        
        Bind<Button>(typeof(Buttons));
        GetButton((int)Buttons.MakeRoomButton).onClick.AddListener(OnClickMakeRoomButton);

        _roomPanel = Util.FindChild(gameObject, "Content", recursive: true);
    }

    private void OnClickMakeRoomButton()
    {
        Lobby_SubItem lobbyItem = Managers.UI.MakeSubItem<Lobby_SubItem>(_roomPanel.transform);
        lobbyItem.SetData(new LobbySubItemData
        {
            RoomName = "πÊ ¿Ã∏ß " + i++.ToString(),
            CurrentPlayerCount = 1,
            MaxPlayerCount = 5
        });
    }
}
