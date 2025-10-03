using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Extension
{
    //public static PositionInfo AddVector(this PositionInfo positionInfo, Vector3 vector)
    //{
    //    positionInfo.PosX += vector.x;
    //    positionInfo.PosY += vector.y;
    //    positionInfo.PosZ += vector.z;

    //    return positionInfo;
    //}
    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
	{
		return Util.GetOrAddComponent<T>(go);
	}

	public static void BindEvent(this GameObject go, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
	{
		UI_Base.BindEvent(go, action, type);
	}

	public static bool IsValid(this GameObject go)
	{
		return go != null && go.activeSelf;
	}
}
