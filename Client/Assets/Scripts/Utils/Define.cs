using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum CameraMode
    {
        QuarterView
    }
    public enum UIEvent
    {
        Click,
        Drag,
    }

    public enum Scene
    {
        Unknown,
        Loading,
        MainMenu,
        Lobby,
        WaitingRoom,
        InGame,
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount,
    }

    public enum MouseEvent
    {
        Press,
        PointerDown,
        PointerUp,
        Click,
    }
}
