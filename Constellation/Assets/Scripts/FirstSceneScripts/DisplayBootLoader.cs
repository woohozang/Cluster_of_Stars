using UnityEngine;
using UnityEngine.UI; // UI 관련 기능을 위해 필수
using System.Collections;

public class DisplayBootLoader : MonoBehaviour
{
    [Header("Settings")]
    public GameObject targetCanvas; // HUD 캔버스 오브젝트
    public float delayTime = 3.0f;  // 시작 전 대기 시간
    public float fadeDuration = 1.5f; // 페이드 인 걸리는 시간

    void Start()
    {
        if (targetCanvas != null)
        {
            // 1. 캔버스 그룹 컴포넌트가 없으면 자동으로 추가해줍니다 (안전장치)
            CanvasGroup cg = targetCanvas.GetComponent<CanvasGroup>();
            if (cg == null) cg = targetCanvas.AddComponent<CanvasGroup>();

            // 2. 일단 투명하게 만들고 꺼둡니다.
            cg.alpha = 0f;
            targetCanvas.SetActive(false);
        }

        // 부팅 시퀀스 시작
        StartCoroutine(BootSequence());
    }

    IEnumerator BootSequence()
    {
        // 1. 대기 시간 (검은 화면)
        yield return new WaitForSeconds(delayTime);

        // 2. 캔버스 켜기 (아직 투명해서 안 보임)
        if (targetCanvas != null)
        {
            targetCanvas.SetActive(true);

            // 3. 서서히 밝아지기 (Fade In)
            CanvasGroup cg = targetCanvas.GetComponent<CanvasGroup>();
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                // Alpha 값을 0에서 1로 부드럽게 변경
                cg.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }

            // 확실하게 1로 고정
            cg.alpha = 1f;
            Debug.Log("시스템 부팅 및 페이드 인 완료!");
        }
    }
}