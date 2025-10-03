using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using UnityEngine;
using System;
using System.IO;

class PacketHandler
{
    // 내가 게임에 입장할 때 패킷
    // 만들고 보니까 클라 쪽에서 세션은 굳이 필요 없을듯 함
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;
        Managers.Object.Add(enterGamePacket.ObjectInfo, isMyPlayer: true);
        Debug.Log($"(아이디: {enterGamePacket.ObjectInfo.ObjectId}, 이름: {enterGamePacket.ObjectInfo.Name})");
    }
    // 게임에서 죽었을 때
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        //S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
        //if (leaveGamePacket.IsAttacked && Managers.Object.MyPlayer.IsWinner == false)
        //    Managers.Resource.InstantiateResources("UI_Dead");
        //Managers.Object.RemoveAll();
    }
    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;

        foreach (ObjectInfo player in spawnPacket.ObjectInfos)
        {
            Managers.Object.Add(player, isMyPlayer: false);
        }
    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
        foreach (int id in despawnPacket.ObjectIds)
        {
            Managers.Object.Remove(id);
        }
    }
    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;
        //TimeSpan latency = DateTime.Now - Managers.Game.PrevLatency;

        //Debug.Log($"Latency: {latency.TotalMilliseconds} ms");
        //Managers.Game.NowLatency = latency;

        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null)
        {
            Debug.Log($"Cant find GameObject {movePacket.ObjectId}");
            return;
        }

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc == null)
        {
            Debug.Log("Cant find CreatureController");
            return;
        }
        cc.CreatureState = movePacket.CreatureState;
        // 서버에서 이동하라고 한 부분
        // 클라에서 움직였지만 서버 요청을 준 후 다시 움직이는 코드임
        GameObjectType type = Managers.Object.GetObjectTypeById(cc.Id);
        if (type == GameObjectType.Player)
        {
            OtherPlayerController otherPlayer = go.GetComponent<OtherPlayerController>();
            if (otherPlayer != null)
            {
                otherPlayer.PositionInfo = movePacket.PositionInfo;
            }

            //// 움직인게 내 플레이어가 아니고 다른 플레이어일 때는 스르르 움직이게 보여주기
            //// 내껏도 스르르 움직이게 서버에서 조종하면 트레이서 일어남
            //MyPlayerController mc = go.GetComponent<MyPlayerController>();
            //if (mc == null && movePacket.UseTeleport == false)
            //{
            //    Vector3 destPos = new Vector3(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY, 0f);
            //    cc.IsMoving = true;
            //    cc.DestPos = destPos;
            //    cc.PosInfo.RotZ = movePacket.PosInfo.RotZ;
            //    cc.transform.eulerAngles = new Vector3(0, 0, movePacket.PosInfo.RotZ);
            //}
            //// 텔레 포트중
            //else if (movePacket.UseTeleport == true)
            //{
            //    pc.PosInfo = movePacket.PosInfo;
            //    pc.PosInfo.RotZ = movePacket.PosInfo.RotZ;
            //    pc.transform.eulerAngles = new Vector3(0, 0, movePacket.PosInfo.RotZ);
            //    cc.UseTeleport = false;
            //    Debug.Log($"{cc.Id}가 텔레포트중");
            //}
        }
        //else if (type == GameObjectType.Monster)
        //{
        //    MonsterController mc = cc.GetComponent<MonsterController>();
        //    Vector3 destPos = new Vector3(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY, 0f);

        //    cc.IsMoving = true;
        //    cc.DestPos = destPos;

        //    // 각도 수정
        //    cc.transform.eulerAngles = new Vector3(0, 0, movePacket.PosInfo.RotZ);
        //    cc.PosInfo.RotZ = movePacket.PosInfo.RotZ;
        //}
        //else if (type == GameObjectType.Projectile)
        //{
        //    BulletController bc = cc.GetComponent<BulletController>();
        //    // 각도 수정
        //    bc.PosInfo.RotZ = movePacket.PosInfo.RotZ;
        //    bc.transform.eulerAngles = new Vector3(0, 0, cc.PosInfo.RotZ);
        //    // 크기 수정
        //    bc.transform.localScale += new Vector3(movePacket.BulletScaleBuff, movePacket.BulletScaleBuff, 0);
        //    // 방향 벡터 수정
        //    //Debug.Log(movePacket.PosInfo);
        //    bc.Dir = new Vector3(movePacket.PosInfo.PosX, movePacket.PosInfo.PosY, 0f);
        //    bc.IsMoving = true;
        //}
    }
}