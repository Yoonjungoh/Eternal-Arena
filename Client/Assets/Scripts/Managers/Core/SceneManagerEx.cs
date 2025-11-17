using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public Define.Scene _currentScene;
    public Define.Scene CurrentScene
    {
        get { return _currentScene; }
        set
        {
            if (_currentScene == value)
                return;
            _currentScene = value;
            Managers.Sound.ChangeBgmWhenSceneLoaded();
        }
    }

    public void LoadScene(string sceneName)
    {
        Define.Scene sceneEnumValue = Define.Scene.Unknown;

        if (Enum.TryParse(sceneName, out sceneEnumValue))
        {
            Managers.Clear();
            UI_Loading.Instance.LoadScene(sceneName);
            Debug.Log($"Loading {sceneName} Scene");
        }
        else
        {
            Debug.Log($"Dont exist {sceneName} Scene");
        }
        CurrentScene = sceneEnumValue;
    }

    public void LoadScene(Define.Scene type)
    {
        Managers.Clear();
        CurrentScene = type;
        UI_Loading.Instance.LoadScene(GetSceneName(type));
    }

    string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
    }
}
