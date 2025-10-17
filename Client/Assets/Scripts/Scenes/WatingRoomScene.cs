using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatingRoomScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        // 씬 로딩 되면 플레이어 아바타 생성
        Managers.Object.Add(Managers.Game.SpawnObjectInfo, isMyPlayer: true);
        Debug.Log($"(아이디: {Managers.Game.SpawnObjectInfo.ObjectId}, 이름: {Managers.Game.SpawnObjectInfo.Name})");
        Camera.main.GetComponent<CameraController>().SetCommonView();
    }

    private void Awake()
    {
        Init();
    }
}
