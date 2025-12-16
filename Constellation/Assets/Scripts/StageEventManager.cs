using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class StageEventManager : MonoBehaviour
{
    // ★ [수정] struct -> class로 변경 (초기값 문제 해결)
    [System.Serializable]
    public class StageEndingData
    {
        [Header("기본 정보")]
        public string stageName;

        [Header("UI 시퀀스 연출")]
        public RawImage firstImage;      // 첫 번째 이미지
        public RawImage secondImage;     // 두 번째 이미지
        public TextMeshProUGUI clearText; // 클리어 텍스트

        [Tooltip("클리어 시 숨길 UI (예: 튜토리얼 창)")]
        public GameObject uiToHide;

        [Header("시간 설정")]
        public float uiFadeDuration = 1f;   // 페이드 시간 (기본값 1초)
        public float displayDuration = 2f;  // 유지 시간 (기본값 2초)

        [Header("연출 오브젝트")]
        public GameObject targetMapLight;
        public Transform lookTarget;
        public StageClearCinematic cinematicScript;

        [Header("파티클 효과")]
        public GameObject defaultParticle;
        public GameObject shineParticle;
        public GameObject clearParticle;
        public GameObject clearEffect;
    }

    [Header("전체 스테이지 리스트")]
    public List<StageEndingData> stageList;

    [Header("플레이어 참조")]
    public Transform playerRig;
    public Transform centerEyeAnchor;

    public static StageEventManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;

        if (playerRig == null) playerRig = GameObject.Find("OVRCameraRig")?.transform;
        if (centerEyeAnchor == null && playerRig != null)
            centerEyeAnchor = playerRig.Find("TrackingSpace/CenterEyeAnchor");
    }

    public void PlayEnding(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= stageList.Count)
        {
            Debug.LogError($"스테이지 {stageIndex + 1} 데이터가 없습니다!");
            return;
        }

        StageEndingData data = stageList[stageIndex];

        StartCoroutine(ProcessEndingSequence(data));
    }

    IEnumerator ProcessEndingSequence(StageEndingData data)
    {
        // 1. 기존 UI 숨기기
        if (data.uiToHide != null) data.uiToHide.SetActive(false);

        // 2. 오브젝트/파티클 연출 실행
        if (data.defaultParticle != null) data.defaultParticle.SetActive(false);
        if (data.shineParticle != null) data.shineParticle.SetActive(true);
        if (data.clearParticle != null) data.clearParticle.SetActive(true);
        if (data.clearEffect != null) data.clearEffect.SetActive(true);
        if (data.targetMapLight != null) data.targetMapLight.SetActive(true);

        // 3. 시선 고정
        if (playerRig != null && centerEyeAnchor != null && data.lookTarget != null)
        {
            Vector3 dir = data.lookTarget.position - centerEyeAnchor.position;
            dir.y = 0; dir.Normalize();
            Vector3 headDir = centerEyeAnchor.forward;
            headDir.y = 0; headDir.Normalize();
            float angle = Vector3.SignedAngle(headDir, dir, Vector3.up);
            playerRig.Rotate(Vector3.up, angle);
        }

        // 4. 시네마틱 재생
        if (data.cinematicScript != null) data.cinematicScript.PlaySequence();

        // 5. UI 시퀀스 시작
        // (1) First Image
        if (data.firstImage != null)
        {
            yield return StartCoroutine(FadeUI(data.firstImage, data.uiFadeDuration, data.displayDuration));
        }

        // (2) Second Image
        if (data.secondImage != null)
        {
            yield return StartCoroutine(FadeUI(data.secondImage, data.uiFadeDuration, data.displayDuration));
        }

        // (3) Clear Text (마지막에 유지)
        if (data.clearText != null)
        {
            data.clearText.gameObject.SetActive(true);
            data.clearText.alpha = 0;

            float timer = 0f;
            while (timer < data.uiFadeDuration)
            {
                timer += Time.deltaTime;
                data.clearText.alpha = Mathf.Lerp(0f, 1f, timer / data.uiFadeDuration);
                yield return null;
            }
            data.clearText.alpha = 1f;
        }
    }

    IEnumerator FadeUI(Graphic uiElement, float fadeDuration, float displayDuration)
    {
        uiElement.gameObject.SetActive(true);
        uiElement.canvasRenderer.SetAlpha(0f);

        // Fade In
        uiElement.CrossFadeAlpha(1f, fadeDuration, false);
        yield return new WaitForSeconds(fadeDuration);

        // Display
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        uiElement.CrossFadeAlpha(0f, fadeDuration, false);
        yield return new WaitForSeconds(fadeDuration);

        uiElement.gameObject.SetActive(false);
    }
}