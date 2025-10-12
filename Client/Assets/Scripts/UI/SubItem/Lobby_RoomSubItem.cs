using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public struct LobbyRoomSubItemData
{
    public int RoomId;
    public string RoomName;
    public int CurrentPlayerCount;
    public int MaxPlayerCount;
}

public class Lobby_RoomSubItem : UI_SubItem<LobbyRoomSubItemData>
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
        Debug.Log($"{_data.RoomName} 방 입장");
        // TODO - 패킷
    }
    
    public override void SetData(LobbyRoomSubItemData data)
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
