using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GalleryTrigger : MonoBehaviour
{
    public CanvasGroup targetCanvas; // 보여줄 캔버스
    public float fadeDuration = 1.5f;
    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        // 플레이어 태그 확인 (Player 또는 MainCamera)
        if (other.CompareTag("Player") || other.name.Contains("Head"))
        {
            hasTriggered = true;
            StartCoroutine(FadeInCanvas());
        }
    }

    IEnumerator FadeInCanvas()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            targetCanvas.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        targetCanvas.alpha = 1f;
    }
}