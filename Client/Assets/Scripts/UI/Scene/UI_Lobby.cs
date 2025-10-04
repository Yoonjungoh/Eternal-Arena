using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Lobby : UI_Scene
{
    private GameObject _roomPanel;

    public override void Init()
    {
        base.Init();
        _roomPanel = Util.FindChild(gameObject, "Content", recursive: true);
        for (int i = 0; i < 10; ++i)
        {
            Lobby_SubItem lobbyItem = Managers.UI.MakeSubItem<Lobby_SubItem>(_roomPanel.transform);
            lobbyItem.SetData(new LobbySubItemData
            {
                RoomName = "πÊ ¿Ã∏ß " + i.ToString(),
                CurrentPlayerCount = 1,
                MaxPlayerCount = 5
            });
        }
    }
}
