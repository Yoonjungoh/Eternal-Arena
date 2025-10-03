using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CreatureController
{
    public override void Init()
    {
        base.Init();
        GameObjectType = GameObjectType.Player;
    }
}
