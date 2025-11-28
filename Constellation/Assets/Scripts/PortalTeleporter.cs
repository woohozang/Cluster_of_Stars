using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 필요

public class PortalTeleporter : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("이동할 목적지 위치")]
    public Transform destinationPoint;

    [Tooltip("화면이 깜빡이는 속도 (초 단위)")]
    public float fadeDuration = 0.5f;

    private bool isTeleporting = false; // 중복 이동 방지용

    private void OnTriggerEnter(Collider other)
    {
        // 이미 이동 중이 아니고, 플레이어라면 실행
        if (!isTeleporting && other.CompareTag("Player"))
        {
            // OVRCameraRig(플레이어 최상위 부모)를 찾습니다.
            Transform playerRig = other.transform.root;

            // 이동 시퀀스(코루틴) 시작
            StartCoroutine(TeleportSequence(playerRig));
        }
    }

    private IEnumerator TeleportSequence(Transform playerRig)
    {
        isTeleporting = true; // 이동 시작 체크

        // 1. 플레이어의 눈(CenterEyeAnchor)에서 OVRScreenFade 컴포넌트를 찾습니다.
        // (보통 OVRCameraRig -> TrackingSpace -> CenterEyeAnchor에 있습니다.)
        var screenFade = playerRig.GetComponentInChildren<OVRScreenFade>();

        // 2. 페이드 아웃 (화면 검게 만들기)
        if (screenFade != null)
        {
            screenFade.FadeOut();
        }

        // 화면이 완전히 어두워질 때까지 대기
        yield return new WaitForSeconds(fadeDuration);

        // 3. 플레이어 위치 이동 (순간이동)
        playerRig.position = destinationPoint.position;

        // (선택) 회전값도 맞추려면 아래 주석 해제
        // playerRig.rotation = destinationPoint.rotation;

        // 위치 이동 후 아주 잠깐 대기 (로딩/렌더링 안정화)
        yield return new WaitForSeconds(0.1f);

        // 4. 페이드 인 (화면 다시 밝게 만들기)
        if (screenFade != null)
        {
            screenFade.FadeIn();
        }

        // 이동 완료 후 플래그 해제 (잠시 후 다시 이동 가능하도록)
        yield return new WaitForSeconds(1.0f);
        isTeleporting = false;
    }
}