using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class StageEventManager : MonoBehaviour
{
    [System.Serializable]
    public class StageEndingData
    {
        [Header("기본 정보")]
        public string stageName;

        [Header("UI 시퀀스 연출")]
        [Tooltip("엔딩 UI들이 모여있는 부모 캔버스")]
        public GameObject endingCanvas;

        [Tooltip("첫 번째로 뜰 이미지 (없으면 비워두세요)")]
        public RawImage firstImage;
        [Tooltip("두 번째로 뜰 이미지 (없으면 비워두세요)")]
        public RawImage secondImage;

        public TextMeshProUGUI clearText;

        [Tooltip("클리어 시 숨길 UI (예: 튜토리얼 창)")]
        public GameObject uiToHide;

        [Header("시간 설정")]
        public float uiFadeDuration = 1f; // 나타나는 데 걸리는 시간
        public float delayBetweenUI = 0.5f; // 다음 UI가 나올 때까지 대기 시간

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
        // 1. 숨길 UI 끄기
        if (data.uiToHide != null) data.uiToHide.SetActive(false);

        // 2. 엔딩 캔버스 켜기 & 초기화 (깜빡임 방지)
        if (data.endingCanvas != null)
        {
            // 먼저 캔버스를 켜기 전에 내용물들을 투명하게 만듭니다.
            if (data.firstImage != null) { data.firstImage.gameObject.SetActive(false); data.firstImage.canvasRenderer.SetAlpha(0f); }
            if (data.secondImage != null) { data.secondImage.gameObject.SetActive(false); data.secondImage.canvasRenderer.SetAlpha(0f); }
            if (data.clearText != null) { data.clearText.gameObject.SetActive(false); data.clearText.alpha = 0f; }

            // 그 다음 캔버스를 켭니다.
            data.endingCanvas.SetActive(true);
        }

        // 3. 오브젝트/파티클 연출 실행
        if (data.defaultParticle != null) data.defaultParticle.SetActive(false);
        if (data.shineParticle != null) data.shineParticle.SetActive(true);
        if (data.clearParticle != null) data.clearParticle.SetActive(true);
        if (data.clearEffect != null) data.clearEffect.SetActive(true);
        if (data.targetMapLight != null) data.targetMapLight.SetActive(true);

        // 4. 시선 고정
        if (playerRig != null && centerEyeAnchor != null && data.lookTarget != null)
        {
            Vector3 dir = data.lookTarget.position - centerEyeAnchor.position;
            dir.y = 0; dir.Normalize();
            Vector3 headDir = centerEyeAnchor.forward;
            headDir.y = 0; headDir.Normalize();
            float angle = Vector3.SignedAngle(headDir, dir, Vector3.up);
            playerRig.Rotate(Vector3.up, angle);
        }

        // 5. 시네마틱 재생
        if (data.cinematicScript != null) data.cinematicScript.PlaySequence();

        // 6. UI 시퀀스 시작 (나타나고 유지됨)

        // (1) First Image Fade In
        if (data.firstImage != null)
        {
            yield return StartCoroutine(FadeInUI(data.firstImage, data.uiFadeDuration));
            yield return new WaitForSeconds(data.delayBetweenUI);
        }

        // (2) Second Image Fade In (First Image 위에 겹쳐서 뜸 or 옆에 뜸)
        if (data.secondImage != null)
        {
            yield return StartCoroutine(FadeInUI(data.secondImage, data.uiFadeDuration));
            yield return new WaitForSeconds(data.delayBetweenUI);
        }

        // (3) Clear Text Fade In
        if (data.clearText != null)
        {
            data.clearText.gameObject.SetActive(true);
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

    // 사라지지 않고 나타나기만 하는 함수
    IEnumerator FadeInUI(Graphic uiElement, float duration)
    {
        uiElement.gameObject.SetActive(true);
        uiElement.canvasRenderer.SetAlpha(0f);
        uiElement.CrossFadeAlpha(1f, duration, false); // 투명 -> 불투명
        yield return new WaitForSeconds(duration);
    }
}