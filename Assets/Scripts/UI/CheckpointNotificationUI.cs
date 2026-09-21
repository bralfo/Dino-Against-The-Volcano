using System.Collections;
using TMPro;
using UnityEngine;

public class CheckpointNotificationUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField, Min(0f)] private float visibleDuration = 1.25f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.75f;

    private Coroutine notificationCoroutine;

    private void Awake()
    {
        SetAlpha(0f);
    }

    public void Initialize(CanvasGroup group, TextMeshProUGUI text)
    {
        canvasGroup = group;
        messageText = text;
        SetAlpha(0f);
    }

    public void ShowCheckpoint()
    {
        Show("CHECKPOINT");
    }

    public void ShowSaved()
    {
        Show("JOGO SALVO");
    }

    private void Show(string message)
    {
        if (messageText != null)
            messageText.text = message;

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationCoroutine = StartCoroutine(ShowAndFade());
    }

    private IEnumerator ShowAndFade()
    {
        SetAlpha(1f);

        yield return new WaitForSecondsRealtime(visibleDuration);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(1f - Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }

        SetAlpha(0f);
        notificationCoroutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = alpha;
    }
}
