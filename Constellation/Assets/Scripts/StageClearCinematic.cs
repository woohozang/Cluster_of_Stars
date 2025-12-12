using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StageClearCinematic : MonoBehaviour
{
    [Header("★ 필수 할당")]
    public Transform playerRig;       // OVRCameraRig (최상위 부모)
    public Transform centerEyeAnchor; // OVRCameraRig > TrackingSpace > CenterEyeAnchor
    public Transform targetView;      // 바라봐야 할 대상 (엔딩 화면, 주인공 등)

    [Header("위치 설정")]
    [Tooltip("UI를 감상할 별도 공간의 플레이어 위치 (MovieRoom 안)")]
    public Transform movieRoomPoint;

    [Tooltip("연출이 끝나고 돌아올 원래 시작 위치")]
    public Transform originalStartPoint;
    public GameObject Stage1;

    [Header("UI 연출 요소 (순서대로)")]
    public RawImage firstImage;
    public RawImage secondImage;
    public TMP_Text clearText;

    [Header("시간 설정")]
    public float uiFadeDuration = 1.0f;
    public float displayDuration = 3.0f;
    public float screenFadeDuration = 1.0f;

    [Header("플레이어 제어")]
    public GameObject ovrCameraRigRoot;
    public MonoBehaviour playerController;

    private OVRScreenFade screenFade;

    void Start()
    {
        if (ovrCameraRigRoot != null)
        {
            screenFade = ovrCameraRigRoot.GetComponentInChildren<OVRScreenFade>();
            
        }

        // 시작할 때 UI 요소들은 투명하게 초기화
        SetAlpha(firstImage, 0);
        SetAlpha(secondImage, 0);
        SetAlpha(clearText, 0);
    }

    public void AlignPlayerViewToTarget()
    {
        if (playerRig == null || centerEyeAnchor == null || targetView == null)
        {
            Debug.LogError("PlayerRig, CenterEyeAnchor, TargetView를 모두 연결해주세요!");
            return;
        }

        // 1. 목표 방향 벡터 계산 (높이 y축 무시)
        Vector3 directionToTarget = targetView.position - playerRig.position;
        directionToTarget.y = 0;
        directionToTarget.Normalize();

        // 2. 현재 내 머리(카메라)가 보고 있는 방향 (높이 y축 무시)
        Vector3 currentHeadDir = centerEyeAnchor.forward;
        currentHeadDir.y = 0;
        currentHeadDir.Normalize();

        // 3. 두 방향 사이의 각도 차이 계산 (Y축 기준)
        float angleDiff = Vector3.SignedAngle(currentHeadDir, directionToTarget, Vector3.up);

        // 4. 차이만큼 몸통(Rig)을 회전시켜서 보정
        playerRig.Rotate(Vector3.up, angleDiff);
    }

    public void PlaySequence()
    {
        AlignPlayerViewToTarget();
        StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        Debug.Log("시네마틱 시작: 안전 모드");
        Stage1.SetActive(false);
        // 1. 조작 차단
        if (playerController != null) playerController.enabled = false;

        // 2. 페이드 아웃 (화면 암전)
        if (screenFade != null) screenFade.FadeOut();
        yield return new WaitForSeconds(screenFadeDuration);

        // ------------------------------------------
        // ★ 이동 및 연출 시작
        // ------------------------------------------

        // 3. UI 투명화 (혹시 켜져있을까봐 다시 초기화)
        SetAlpha(firstImage, 0);
        SetAlpha(secondImage, 0);
        SetAlpha(clearText, 0);

        // 4. 플레이어 이동 (MovieRoom으로)
        if (ovrCameraRigRoot != null && movieRoomPoint != null)
        {
            ovrCameraRigRoot.transform.position = movieRoomPoint.position;
            ovrCameraRigRoot.transform.rotation = movieRoomPoint.rotation;
        }

        // 5. 페이드 인 (화면 밝아짐 -> MovieRoom이 보여야 함)
        if (screenFade != null) screenFade.FadeIn();
        yield return new WaitForSeconds(screenFadeDuration);

        // 6. UI 순차 등장
        yield return StartCoroutine(FadeInGraphic(firstImage, uiFadeDuration));
        yield return StartCoroutine(FadeInGraphic(secondImage, uiFadeDuration));
        yield return StartCoroutine(FadeInGraphic(clearText, uiFadeDuration));

        // 7. 감상 시간 대기
        yield return new WaitForSeconds(displayDuration);

        // ------------------------------------------
        // ★ 복귀
        // ------------------------------------------

        // 8. 페이드 아웃
        if (screenFade != null) screenFade.FadeOut();
        yield return new WaitForSeconds(screenFadeDuration);

        // 9. 원래 위치로 복귀
        if (ovrCameraRigRoot != null && originalStartPoint != null)
        {
            ovrCameraRigRoot.transform.position = originalStartPoint.position;
            ovrCameraRigRoot.transform.rotation = originalStartPoint.rotation;
            Stage1.SetActive(true);
        }

        // 10. 페이드 인
        if (screenFade != null) screenFade.FadeIn();

        // 11. 조작 해제
        if (playerController != null) playerController.enabled = true;

        Debug.Log("시네마틱 종료");
    }

    // 알파값 애니메이션 코루틴
    private IEnumerator FadeInGraphic(Graphic target, float duration)
    {
        if (target == null) yield break;

        // 혹시 꺼져있으면 켬
        target.gameObject.SetActive(true);

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / duration);
            SetAlpha(target, alpha);
            yield return null;
        }
        SetAlpha(target, 1f);
    }

    private void SetAlpha(Graphic target, float alpha)
    {
        if (target != null)
        {
            Color c = target.color;
            c.a = alpha;
            target.color = c;
        }
    }
}