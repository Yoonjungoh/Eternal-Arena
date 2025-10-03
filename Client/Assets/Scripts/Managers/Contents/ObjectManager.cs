using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectManager
{
    public MyPlayerController MyPlayer { get; set; }
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> Objects { get { return _objects; } set { _objects = value; } }

    public GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0x7F;
        return (GameObjectType)type;
    }

    public void Add(ObjectInfo objectInfo, bool isMyPlayer = false)
    {
        GameObjectType objectType = GetObjectTypeById(objectInfo.ObjectId);
        if (isMyPlayer)
        {
            GameObject player = Managers.Resource.Instantiate($"Creatures/Players/MyPlayers/MyPlayer_1");
            MyPlayer = player.GetComponent<MyPlayerController>();

            PositionInfo positionInfo = objectInfo.PositionInfo;
            MyPlayer.PositionInfo = positionInfo;

            MyPlayer.Id = objectInfo.ObjectId;

            // 카메라 부착
            CameraController cameraController = Camera.main.AddComponent<CameraController>();
            cameraController.Player = MyPlayer.gameObject;

            _objects.Add(objectInfo.ObjectId, MyPlayer.gameObject);
        }
        else
        {
            GameObject player = Managers.Resource.Instantiate($"Creatures/Players/OtherPlayers/OtherPlayer_1");
            OtherPlayerController otherPlayer = player.GetComponent<OtherPlayerController>();

            PositionInfo positionInfo = objectInfo.PositionInfo;
            otherPlayer.PositionInfo = positionInfo;

            otherPlayer.Id = objectInfo.ObjectId;
            
            _objects.Add(objectInfo.ObjectId, otherPlayer.gameObject);
        }
    }

    public void Remove(int id)
    {
        GameObject go = FindById(id);
        if (go == null)
            return;

        _objects.Remove(id);
        Managers.Resource.Destroy(go);
    }

    public void RemoveAll()
    {
        Clear();
        //MyPlayer = null;
    }

    public GameObject FindById(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (GameObject obj in _objects.Values)
        {
            if (condition.Invoke(obj))
                return obj;
        }

        return null;
    }

    public void Clear()
    {
        foreach (GameObject obj in _objects.Values)
        {
            if (obj != null)
                Managers.Resource.Destroy(obj);
        }
        _objects.Clear();
    }
}