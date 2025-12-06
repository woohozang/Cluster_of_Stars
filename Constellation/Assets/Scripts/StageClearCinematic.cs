using UnityEngine;
using UnityEngine.UI; // RawImage 제어용
using TMPro;          // TextMeshPro 제어용
using System.Collections;

public class StageClearCinematic : MonoBehaviour
{
    [Header("위치 설정")]
    [Tooltip("UI를 감상할 별도 공간의 플레이어 위치 (MovieRoom 안)")]
    public Transform movieRoomPoint;

    [Tooltip("연출이 끝나고 돌아올 원래 시작 위치")]
    public Transform originalStartPoint;

    [Header("UI 연출 순서 설정 (순서대로 넣으세요)")]
    [Tooltip("가장 먼저 나타날 이미지")]
    public RawImage firstImage;
    [Tooltip("두 번째로 나타날 이미지")]
    public RawImage secondImage;
    [Tooltip("마지막에 나타날 텍스트")]
    public TMP_Text clearText;

    [Header("시간 설정")]
    [Tooltip("각 요소가 페이드 인 되는 데 걸리는 시간")]
    public float uiFadeDuration = 1.0f;
    [Tooltip("모든 UI가 다 나온 뒤 대기하는 시간")]
    public float displayDuration = 3.0f;
    [Tooltip("화면 전환(암전) 속도")]
    public float screenFadeDuration = 1.0f;

    [Header("플레이어 제어")]
    [Tooltip("플레이어의 OVRCameraRig 최상위 오브젝트")]
    public GameObject ovrCameraRigRoot;
    [Tooltip("이동/회전을 담당하는 스크립트 (PlayerController)")]
    public MonoBehaviour playerController;

    private OVRScreenFade screenFade;

    void Start()
    {
        if (ovrCameraRigRoot != null)
        {
            screenFade = ovrCameraRigRoot.GetComponentInChildren<OVRScreenFade>();
        }

        // 시작할 때 UI 요소들은 투명하게 만들거나 꺼둡니다.
        SetAlpha(firstImage, 0);
        SetAlpha(secondImage, 0);
        SetAlpha(clearText, 0);
    }

    public void PlaySequence()
    {
        StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        Debug.Log("스테이지 클리어: UI 시퀀스 시작");

        // 1. [조작 차단]
        if (playerController != null) playerController.enabled = false;

        // 2. [화면 암전]
        if (screenFade != null) screenFade.FadeOut();
        yield return new WaitForSeconds(screenFadeDuration);

        // ------------------------------------------
        // ★ 암전 상태에서 장소 이동 및 UI 초기화
        // ------------------------------------------

        // 3. 장소 이동
        if (ovrCameraRigRoot != null && movieRoomPoint != null)
        {
            ovrCameraRigRoot.transform.position = movieRoomPoint.position;
            ovrCameraRigRoot.transform.rotation = movieRoomPoint.rotation;
        }

        // 4. UI 초기화 (완전 투명 상태로 시작)
        SetAlpha(firstImage, 0);
        SetAlpha(secondImage, 0);
        SetAlpha(clearText, 0);

        // 5. [화면 밝아짐] 영화관 입장 완료
        if (screenFade != null) screenFade.FadeIn();
        yield return new WaitForSeconds(screenFadeDuration);

        // ------------------------------------------
        // ★ UI 순차적 페이드 인 연출
        // ------------------------------------------

        // 6. 첫 번째 이미지 등장
        yield return StartCoroutine(FadeInGraphic(firstImage, uiFadeDuration));

        // 7. 두 번째 이미지 등장
        yield return StartCoroutine(FadeInGraphic(secondImage, uiFadeDuration));

        // 8. 텍스트 등장
        yield return StartCoroutine(FadeInGraphic(clearText, uiFadeDuration));

        // 9. [대기] 감상 시간
        yield return new WaitForSeconds(displayDuration);

        // ------------------------------------------
        // ★ 연출 종료 후 복귀
        // ------------------------------------------

        // 10. [화면 암전]
        if (screenFade != null) screenFade.FadeOut();
        yield return new WaitForSeconds(screenFadeDuration);

        // 11. 원래 위치로 복귀
        if (ovrCameraRigRoot != null && originalStartPoint != null)
        {
            ovrCameraRigRoot.transform.position = originalStartPoint.position;
            ovrCameraRigRoot.transform.rotation = originalStartPoint.rotation;
        }

        // 12. [화면 밝아짐] 현실 복귀
        if (screenFade != null) screenFade.FadeIn();

        // 13. [조작 해제]
        if (playerController != null) playerController.enabled = true;

        Debug.Log("시퀀스 종료");
    }

    // 알파값을 서서히 올리는 코루틴 (이미지, 텍스트 공용)
    private IEnumerator FadeInGraphic(Graphic target, float duration)
    {
        if (target == null) yield break;

        float timer = 0f;
        Color startColor = target.color;
        startColor.a = 0f;
        target.color = startColor;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / duration);

            Color newColor = target.color;
            newColor.a = alpha;
            target.color = newColor;

            yield return null;
        }

        // 확실하게 1로 설정
        Color finalColor = target.color;
        finalColor.a = 1f;
        target.color = finalColor;
    }

    // 초기 알파값 설정 헬퍼 함수
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