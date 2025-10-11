using Google.Protobuf.Collections;
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
    int i = 0;  // TODO
    private GameObject _roomScrollView;
    private GameObject _userScrollView;
    Dictionary<int, Lobby_UserSubItem> _userSubItemDict = new Dictionary<int, Lobby_UserSubItem>();

    public override void Init()
    {
        base.Init();
        
        Bind<Button>(typeof(Buttons));
        GetButton((int)Buttons.MakeRoomButton).onClick.AddListener(OnClickMakeRoomButton);

        _roomScrollView = Util.FindChild(gameObject, "RoomContent", recursive: true);
        _userScrollView = Util.FindChild(gameObject, "UserContent", recursive: true);
    }

    public void EnterLobby(RepeatedField<int> userIdList)
    {
        for (int i = 0; i < userIdList.Count; i++)
        {
            if (_userSubItemDict.ContainsKey(userIdList[i])) continue;

            Lobby_UserSubItem lobbyUserSubItem = Managers.UI.MakeSubItem<Lobby_UserSubItem>(_userScrollView.transform);
            lobbyUserSubItem.SetData(new LobbyUserSubItemData
            {
                UserId = userIdList[i],
            });
            _userSubItemDict.TryAdd(userIdList[i], lobbyUserSubItem);
        }
    }

    public void LeaveLobby(int userId)
    {
        if (_userSubItemDict.ContainsKey(userId) == false)
        {
            Debug.Log($"UserId: {userId}�� �κ� �������� �ʽ��ϴ�.");
            return;
        }
        Lobby_UserSubItem lobbyUserSubItem = null;
        _userSubItemDict.TryGetValue(userId, out lobbyUserSubItem);
        if (lobbyUserSubItem == null)
        {
            Debug.Log($"UserId: {userId}�� SubItem�� �κ� �������� �ʽ��ϴ�.");
            return;
        }
        Destroy(lobbyUserSubItem);  // TODO - Ǯ��
        _userSubItemDict.Remove(userId);
    }

    private void OnClickMakeRoomButton()
    {
        Lobby_RoomSubItem lobbyRoomSubItem = Managers.UI.MakeSubItem<Lobby_RoomSubItem>(_roomScrollView.transform);
        lobbyRoomSubItem.SetData(new LobbyRoomSubItemData
        {
            RoomName = "�� �̸� " + i++.ToString(),
            CurrentPlayerCount = 1,
            MaxPlayerCount = 5
        });
    }
}
