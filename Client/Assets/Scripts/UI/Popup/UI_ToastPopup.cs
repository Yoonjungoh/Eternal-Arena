using System.Collections;
using TMPro;
using UnityEngine;

public class UI_ToastPopup : UI_Popup
{
    [SerializeField] private float fadeLerpTime = 1.0f;  // 사라지는 속도
    private TextMeshProUGUI _toastPopupText;             // 노출되는 텍스트
    private Coroutine _fadeCoroutine;

    enum Texts
    {
        ToastPopupText,
    }

    public override void Init()
    {
        base.Init();
        Bind<TextMeshProUGUI>(typeof(Texts));
        _toastPopupText = GetTextMeshProUGUI((int)Texts.ToastPopupText);
    }

    public void ShowToastPopup(string message, float duration, Color? colorOverride = null)
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        _fadeCoroutine = StartCoroutine(ShowRoutine(message, duration, colorOverride));
    }

    private IEnumerator ShowRoutine(string message, float duration, Color? colorOverride)
    {
        // 색상 결정 (null이면 현재 텍스트 색상 유지)
        Color baseColor = colorOverride ?? _toastPopupText.color;

        // 초기 설정
        _toastPopupText.text = message;
        baseColor.a = 1f;
        _toastPopupText.color = baseColor;

        // 지정된 시간 동안 유지
        yield return new WaitForSeconds(duration);

        // Fade out 처리
        float currentTime = 0f;
        float percent = 0f;
        while (percent < 1f)
        {
            currentTime += Time.deltaTime;
            percent = currentTime / fadeLerpTime;

            baseColor.a = Mathf.Lerp(1f, 0f, percent);
            _toastPopupText.color = baseColor;

            yield return null;
        }

        _fadeCoroutine = null;
    }
}
