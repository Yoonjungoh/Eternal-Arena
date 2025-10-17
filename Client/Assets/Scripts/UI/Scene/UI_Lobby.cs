using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
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
        AddRoomButton,
    }

    private GameObject _roomScrollView;
    private GameObject _userScrollView;
    Dictionary<int, Lobby_RoomSubItem> _roomSubItemDict = new Dictionary<int, Lobby_RoomSubItem>();
    Dictionary<int, Lobby_UserSubItem> _userSubItemDict = new Dictionary<int, Lobby_UserSubItem>();

    public override void Init()
    {
        base.Init();
        
        Bind<Button>(typeof(Buttons));
        GetButton((int)Buttons.AddRoomButton).onClick.AddListener(OnClickAddRoomButton);

        _roomScrollView = Util.FindChild(gameObject, "RoomContent", recursive: true);
        _userScrollView = Util.FindChild(gameObject, "UserContent", recursive: true);
    }

    public void EnterLobby(RepeatedField<int> userIdList)
    {
        int userIdListCount = userIdList.Count;
        for (int i = 0; i < userIdListCount; i++)
        {
            if (_userSubItemDict.ContainsKey(userIdList[i])) continue;

            Lobby_UserSubItem lobbyUserSubItem = Managers.UI.MakeSubItem<Lobby_UserSubItem>(_userScrollView.transform);
            lobbyUserSubItem.SetData(new LobbyUserSubItemData
            {
                UserId = userIdList[i]
            });
            _userSubItemDict.TryAdd(userIdList[i], lobbyUserSubItem);
        }
    }

    public void LeaveLobby(int userId)
    {
        if (_userSubItemDict.ContainsKey(userId) == false)
        {
            Debug.Log($"UserId: {userId}가 로비에 존재하지 않습니다.");
            return;
        }
        Lobby_UserSubItem lobbyUserSubItem = null;
        _userSubItemDict.TryGetValue(userId, out lobbyUserSubItem);
        if (lobbyUserSubItem == null)
        {
            Debug.Log($"UserId: {userId}의 SubItem이 로비에 존재하지 않습니다.");
            return;
        }
        Destroy(lobbyUserSubItem.gameObject);
        _userSubItemDict.Remove(userId);
    }

    public void OnClickAddRoomButton()
    {
        UI_AddRoom addRoomUI = Managers.UI.ShowPopupUI<UI_AddRoom>();
        addRoomUI.SetData(new AddRoomPopupData
        {
            RoomName = string.Empty
        });
    }

    public void AddRoom(RepeatedField<RoomInfo> roomInfoList)
    {
        int roomInfoListCount = roomInfoList.Count;

        for (int i = 0; i < roomInfoListCount; i++)
        {
            int roomId = roomInfoList[i].RoomId;
            string roomName = roomInfoList[i].RoomName;
            if (_roomSubItemDict.ContainsKey(roomId))
            {
                Debug.Log($"같은 RoomId가 이미 존재합니다. RoomId: {roomId}, RoomName: {roomName}");
                return;
            }
            
            Lobby_RoomSubItem lobbyRoomSubItem = Managers.UI.MakeSubItem<Lobby_RoomSubItem>(_roomScrollView.transform);
            lobbyRoomSubItem.SetData(new LobbyRoomSubItemData
            {
                RoomId = roomId,
                RoomName = roomName,
                MaxPlayerCount = 4,  // TODO 
                CurrentPlayerCount = 1, // TODO - 본인
            });
            _roomSubItemDict.TryAdd(roomId, lobbyRoomSubItem);
        }
    }
}
