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

        [Header("★ [핵심] 영화관 모드 설정")]
        [Tooltip("체크하면 엔딩 시 플레이어를 MovieRoom으로 이동시킵니다.")]
        public bool useMovieRoom = true;
        [Tooltip("이동할 영화관 위치 (Transform)")]
        public Transform movieRoomPoint;
        [Tooltip("이동 시 잠시 꺼둘 현재 스테이지 맵 (예: Stage1 전체 부모)")]
        public GameObject stageRootObject;

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
        public float screenFadeDuration = 1.0f; // 화면 암전 시간
        public float uiFadeDuration = 1.0f;     // UI 나타나는 시간
        public float displayDuration = 3.0f;    // 다 보여주고 감상하는 시간
        public float delayBetweenUI = 0.5f;     // UI 간 간격

        [Header("연출 오브젝트")]
        public GameObject targetMapLight;
        public Transform lookTarget; // (영화관 안쓸때만 사용됨)

        [Header("파티클 효과")]
        public GameObject defaultParticle;
        public GameObject shineParticle;
        public GameObject clearParticle;
        public GameObject clearEffect;
    }

    [Header("전체 스테이지 리스트")]
    public List<StageEndingData> stageList;

    [Header("플레이어 및 OVR 필수 설정")]
    public Transform playerRig;
    public Transform centerEyeAnchor;
    public OVRScreenFade screenFade;  // OVRScreenFade 컴포넌트 (CenterEyeAnchor에 있음)
    public MonoBehaviour playerController; // 플레이어 조작 스크립트

    public static StageEventManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;

        if (playerRig == null) playerRig = GameObject.Find("OVRCameraRig")?.transform;
        if (centerEyeAnchor == null && playerRig != null)
        {
            centerEyeAnchor = playerRig.Find("TrackingSpace/CenterEyeAnchor");
            if (screenFade == null) screenFade = centerEyeAnchor.GetComponent<OVRScreenFade>();
        }
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
        // 0. 현재 위치 저장 (돌아오기 위해)
        Vector3 originalPos = playerRig.position;
        Quaternion originalRot = playerRig.rotation;

        // 1. 조작 차단
        if (playerController != null) playerController.enabled = false;

        // 2. 숨길 UI 끄기
        if (data.uiToHide != null) data.uiToHide.SetActive(false);

        // ---------------------------------------------------
        // ★ [이동 로직] 영화관으로 이동 (Use Movie Room 체크 시)
        // ---------------------------------------------------
        if (data.useMovieRoom && data.movieRoomPoint != null)
        {
            // (1) 화면 암전 (Fade Out)
            if (screenFade != null) screenFade.FadeOut();
            yield return new WaitForSeconds(data.screenFadeDuration);

            // (2) 맵 끄기 & UI 초기화
            if (data.stageRootObject != null) data.stageRootObject.SetActive(false);

            if (data.endingCanvas != null)
            {
                // 내용물 안보이게 초기화 후 캔버스 켜기
                if (data.firstImage != null) { data.firstImage.gameObject.SetActive(false); data.firstImage.canvasRenderer.SetAlpha(0f); }
                if (data.secondImage != null) { data.secondImage.gameObject.SetActive(false); data.secondImage.canvasRenderer.SetAlpha(0f); }
                if (data.clearText != null) { data.clearText.gameObject.SetActive(false); data.clearText.alpha = 0f; }
                data.endingCanvas.SetActive(true);
            }

            // (3) 플레이어 이동
            playerRig.position = data.movieRoomPoint.position;
            playerRig.rotation = data.movieRoomPoint.rotation;

            // (4) 화면 밝아짐 (Fade In)
            if (screenFade != null) screenFade.FadeIn();
            yield return new WaitForSeconds(data.screenFadeDuration);
        }
        else
        {
            // 영화관 안 쓰면 제자리에서 캔버스만 켬
            if (data.endingCanvas != null) data.endingCanvas.SetActive(true);
            // 시선 고정 (제자리일 경우)
            if (data.lookTarget != null)
            {
                Vector3 dir = data.lookTarget.position - centerEyeAnchor.position;
                dir.y = 0; dir.Normalize();
                Vector3 headDir = centerEyeAnchor.forward;
                headDir.y = 0; headDir.Normalize();
                float angle = Vector3.SignedAngle(headDir, dir, Vector3.up);
                playerRig.Rotate(Vector3.up, angle);
            }
        }

        // 3. 파티클/불빛 연출
        if (data.defaultParticle != null) data.defaultParticle.SetActive(false);
        if (data.shineParticle != null) data.shineParticle.SetActive(true);
        if (data.clearParticle != null) data.clearParticle.SetActive(true);
        if (data.clearEffect != null) data.clearEffect.SetActive(true);
        if (data.targetMapLight != null) data.targetMapLight.SetActive(true);

        // 4. ★ UI 순차 등장 (사라지지 않고 유지됨)
        if (data.firstImage != null)
        {
            yield return StartCoroutine(FadeInUI(data.firstImage, data.uiFadeDuration));
            yield return new WaitForSeconds(data.delayBetweenUI);
        }

        if (data.secondImage != null)
        {
            yield return StartCoroutine(FadeInUI(data.secondImage, data.uiFadeDuration));
            yield return new WaitForSeconds(data.delayBetweenUI);
        }

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

        // 5. 감상 시간 대기
        yield return new WaitForSeconds(data.displayDuration);

        // ---------------------------------------------------
        // ★ [복귀 로직] 원래 위치로 돌아오기
        // ---------------------------------------------------
        if (data.useMovieRoom)
        {
            // (1) 화면 암전
            if (screenFade != null) screenFade.FadeOut();
            yield return new WaitForSeconds(data.screenFadeDuration);

            // (2) 복귀 & 맵 켜기 & 엔딩 UI 끄기
            playerRig.position = originalPos;
            playerRig.rotation = originalRot;
            if (data.stageRootObject != null) data.stageRootObject.SetActive(true);
            if (data.endingCanvas != null) data.endingCanvas.SetActive(false);

            // (3) 화면 밝아짐
            if (screenFade != null) screenFade.FadeIn();
            yield return new WaitForSeconds(data.screenFadeDuration);
        }

        // 6. 조작 해제
        if (playerController != null) playerController.enabled = true;
    }

    // UI 페이드인 (나타나고 유지)
    IEnumerator FadeInUI(Graphic uiElement, float duration)
    {
        uiElement.gameObject.SetActive(true);
        uiElement.canvasRenderer.SetAlpha(0f);
        uiElement.CrossFadeAlpha(1f, duration, false);
        yield return new WaitForSeconds(duration);
    }
}