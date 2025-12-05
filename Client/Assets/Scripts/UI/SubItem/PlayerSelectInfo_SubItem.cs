using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectInfo_SubItem : UI_SubItem<PlayerSelectInfo>
{
    enum Texts
    {
        PlayerIdText,
        PlayerNameText,
        GoldText,
    }

    enum Buttons
    {
        SelectButton,
    }

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        GetButton((int)Buttons.SelectButton).onClick.AddListener(OnClickSelectButton);
    }

    private void OnClickSelectButton()
    {
        // 해당 플레이어로 세팅 후, 로비로 이동
        Managers.Lobby.SetSelectedPlayerInfo(_data);
        if (_data == null)
        {
            Managers.UI.ShowToastPopup("플레이어 정보를 불러올 수 없습니다");
            return;
        }
        
        Managers.Scene.LoadScene(Define.Scene.Lobby);
    }


    public override void SetData(PlayerSelectInfo data)
    {
        base.SetData(data);
        UpdateUI();
    }

    protected override void UpdateUI()
    {
        GetTextMeshProUGUI((int)Texts.PlayerIdText).text = $"UId: {_data.PlayerId}";
        GetTextMeshProUGUI((int)Texts.PlayerIdText).text = $"닉네임: {_data.Name}";
        GetTextMeshProUGUI((int)Texts.PlayerIdText).text = $"골드: {_data.Gold}";
    }
}
