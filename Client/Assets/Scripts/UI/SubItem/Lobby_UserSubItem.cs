using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct LobbyUserSubItemData
{
    public int UserId;
}

public class Lobby_UserSubItem : UI_SubItem<LobbyUserSubItemData>
{
    enum Texts
    {
        UserIdText,
    }

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
    }


    public override void SetData(LobbyUserSubItemData data)
    {
        base.SetData(data);
        UpdateUI();
    }

    protected override void UpdateUI()
    {
        GetTextMeshProUGUI((int)Texts.UserIdText).text = $"´Ð³×ÀÓ: {_data.UserId}";
    }
}
