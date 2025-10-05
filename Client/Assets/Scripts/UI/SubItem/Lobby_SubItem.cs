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
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        GetButton((int)Buttons.EnterRoomButton).onClick.AddListener(OnClickEnterRoom);
    }

    private void OnClickEnterRoom()
    {
        UI_Confirm confirmUI = Managers.UI.ShowPopupUI<UI_Confirm>();
        confirmUI.SetData(new ConfirmPopupData
        {
            RoomName = _data.RoomName,
        });
        Debug.Log($"{_data.RoomName} πÊ ¿‘¿Â");
    }

    public override void SetData(LobbySubItemData data)
    {
        base.SetData(data);
        UpdateUI();
    }

    protected override void UpdateUI()
    {
        GetTextMeshProUGUI((int)Texts.RoomNameText).text = _data.RoomName;
        GetTextMeshProUGUI((int)Texts.PlayerCountText).text = $"{_data.CurrentPlayerCount} / {_data.MaxPlayerCount}";
    }
}
