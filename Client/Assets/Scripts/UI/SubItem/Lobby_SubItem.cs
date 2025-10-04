using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public struct LobbySubItemData
{
    public int CurrentPlayerCount;
    public int MaxPlayerCount;
    public string RoomName;
}

public class Lobby_SubItem : UI_SubItem<LobbySubItemData>
{
    enum Buttons
    {
        EnterRoomButton,
    }

    enum Texts
    {
        RoomNameText,
        PlayerCountText,
    }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));

        GetButton((int)Buttons.EnterRoomButton).GetComponent<Button>().onClick.AddListener(OnClickEnterRoom);
        _initialized = true;
    }

    private void OnClickEnterRoom()
    {
        Debug.Log($"{_data.RoomName} πÊ ¿‘¿Â");
    }

    public override void SetData(LobbySubItemData data)
    {
        base.SetData(data);
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        GetTextMeshProUGUI((int)Texts.RoomNameText).text = _data.RoomName;
        GetTextMeshProUGUI((int)Texts.PlayerCountText).text = $"{_data.CurrentPlayerCount} / {_data.MaxPlayerCount}";
    }
}
