using System;
using System.Collections.Generic;
using UnityEngine;

public class InputManager
{
    // 키와 Action 매핑
    private Dictionary<KeyCode, Action> _keyActions = new Dictionary<KeyCode, Action>();
    public Action<Define.MouseEvent> MouseAction = null;
    private bool _pressed = false;
    
    public void RegisterKeyAction(KeyCode key, Action action)
    {
        if (_keyActions.ContainsKey(key))
            _keyActions[key] += action;
        else
            _keyActions[key] = action;
    }

    public void OnUpdate()
    {
        foreach (var pair in _keyActions)
        {
            if (Input.GetKeyDown(pair.Key))
            {
                pair.Value?.Invoke();
            }
        }
        if (MouseAction != null)
        {
            if (Input.GetMouseButton(0))
            {
                MouseAction.Invoke(Define.MouseEvent.Press);
                _pressed = true;
            }
            else if (Input.GetMouseButton(1))
            {
                if (_pressed)
                    MouseAction.Invoke(Define.MouseEvent.Click);
                _pressed = false;
            }
        }
    }

    public void Clear()
    {
        _keyActions.Clear();
    }
}

