using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageClearCinematic : MonoBehaviour
{
    [Header("시네마틱 스크린 설정")]
    [Tooltip("CenterEyeAnchor 아래에 만든 CinemaScreen 캔버스")]
    public GameObject cinemaScreenCanvas;

    [Tooltip("영화 스크린이 있는 레이어 (보통 UI)")]
    public LayerMask cinemaLayer; // ★ 추가됨

    [Tooltip("연출용 고정 카메라")]
    public Camera cinematicCamera;

    [Header("플레이어 이동 & 페이드")]
    public GameObject ovrCameraRigRoot;

    [Tooltip("시네마틱 시작 시(암전 중) 플레이어가 이동할 위치")]
    public Transform playerResetPoint; // ★ 추가됨

    public float fadeDuration = 0.5f;

    [Header("빛 연출 설정")]
    public GameObject tracerObject;
    public float moveSpeed = 5.0f;
    public List<Transform> wayPoints;

    private TrailRenderer tracerTrail;
    private OVRScreenFade screenFade;
    private Camera playerEyeCamera; // 플레이어의 눈(카메라)

    // 원래 카메라 설정 저장용
    private int originalCullingMask;
    private CameraClearFlags originalClearFlags;
    private Color originalBackgroundColor;

    void Start()
    {
        if (cinemaScreenCanvas != null) cinemaScreenCanvas.SetActive(false);
        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(false);

        if (tracerObject != null)
        {
            tracerTrail = tracerObject.GetComponent<TrailRenderer>();
            tracerObject.SetActive(false);
        }

        if (ovrCameraRigRoot != null)
        {
            screenFade = ovrCameraRigRoot.GetComponentInChildren<OVRScreenFade>();
            // 플레이어의 실제 카메라 컴포넌트(CenterEyeAnchor)를 찾습니다.
            playerEyeCamera = ovrCameraRigRoot.GetComponentInChildren<Camera>();
        }
    }

    public void PlaySequence()
    {
        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        Debug.Log("시네마틱 시작");

        // 1. [페이드 아웃] 화면 암전
        if (screenFade != null) screenFade.FadeOut();
        yield return new WaitForSeconds(fadeDuration);

        // -------------------------------------------------------
        // ★ 암전 상태에서 무대 세팅 (이동 + 주변 안 보이게 하기)
        // -------------------------------------------------------

        // (1) 플레이어 위치 초기화
        if (playerResetPoint != null && ovrCameraRigRoot != null)
        {
            ovrCameraRigRoot.transform.position = playerResetPoint.position;
            // 회전값도 초기화하려면 아래 주석 해제
            // ovrCameraRigRoot.transform.rotation = playerResetPoint.rotation;
        }

        // (2) 카메라가 '스크린(UI)'만 찍도록 설정 (주변 환경 숨기기)
        if (playerEyeCamera != null)
        {
            // 원래 설정 저장
            originalCullingMask = playerEyeCamera.cullingMask;
            originalClearFlags = playerEyeCamera.clearFlags;
            originalBackgroundColor = playerEyeCamera.backgroundColor;

            // 설정 변경: 배경은 검은색, 보이는 건 cinemaLayer(UI)만
            playerEyeCamera.clearFlags = CameraClearFlags.SolidColor;
            playerEyeCamera.backgroundColor = Color.black;
            playerEyeCamera.cullingMask = cinemaLayer;
        }

        // (3) 시네마틱 장치 켜기
        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(true);
        if (cinemaScreenCanvas != null) cinemaScreenCanvas.SetActive(true);

        // (4) 빛 오브젝트 초기화
        if (tracerObject != null && wayPoints.Count > 0)
        {
            tracerObject.transform.position = wayPoints[0].position;
            tracerObject.SetActive(true);
            if (tracerTrail != null) tracerTrail.Clear();
        }

        yield return new WaitForSeconds(0.5f); // 로딩 대기

        // 2. [페이드 인] 스크린만 둥둥 떠있는 상태로 시작
        if (screenFade != null) screenFade.FadeIn();
        yield return new WaitForSeconds(1.0f);

        // 3. [연출 재생]
        if (tracerObject != null && wayPoints.Count > 0)
        {
            for (int i = 0; i < wayPoints.Count - 1; i++)
            {
                Transform startPos = wayPoints[i];
                Transform endPos = wayPoints[i + 1];
                float journey = 0f;
                float distance = Vector3.Distance(startPos.position, endPos.position);
                float duration = distance / moveSpeed;

                while (journey < duration)
                {
                    journey += Time.deltaTime;
                    float percent = journey / duration;
                    tracerObject.transform.position = Vector3.Lerp(startPos.position, endPos.position, percent);
                    yield return null;
                }
                tracerObject.transform.position = endPos.position;
            }
        }

        yield return new WaitForSeconds(2.0f); // 여운

        // 4. [페이드 아웃] 다시 암전
        if (screenFade != null) screenFade.FadeOut();
        yield return new WaitForSeconds(fadeDuration);

        // -------------------------------------------------------
        // ★ 현실 복귀 (설정 원상복구)
        // -------------------------------------------------------

        // (1) 카메라 설정 복구 (다시 세상이 보이게)
        if (playerEyeCamera != null)
        {
            playerEyeCamera.cullingMask = originalCullingMask;
            playerEyeCamera.clearFlags = originalClearFlags;
            playerEyeCamera.backgroundColor = originalBackgroundColor;
        }

        // (2) 시네마틱 장치 끄기
        if (cinemaScreenCanvas != null) cinemaScreenCanvas.SetActive(false);
        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(false);
        if (tracerObject != null) tracerObject.SetActive(false);

        // 5. [페이드 인] 현실(초기 위치)로 돌아옴
        if (screenFade != null) screenFade.FadeIn();

        Debug.Log("시네마틱 종료");
    }
}