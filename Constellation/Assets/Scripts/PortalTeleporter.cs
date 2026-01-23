using UnityEngine;
using System.Collections;

public class PortalTeleporter : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("이동할 목적지 위치")]
    public Transform destinationPoint;

    [Tooltip("화면이 깜빡이는 속도 (초 단위)")]
    public float fadeDuration = 0.5f;

    [Tooltip("이동 후 다음 이동까지 대기 시간 (초 단위)")]
    public float cooldownTime = 2.0f; // 2초 동안 텔레포트 금지

    [Header("오디오 설정")] // [추가됨]
    [Tooltip("소리를 재생할 스피커 (Audio Source)")]
    public AudioSource audioSource;
    [Tooltip("포탈 탈 때 날 소리 파일")]
    public AudioClip teleportSound;

    // static으로 선언하여 모든 포털이 이 변수를 공유합니다.
    // 즉, 어떤 포털이든 타고 이동 중이면 다른 포털도 작동하지 않습니다.
    private static bool isGlobalTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        // 이미 누군가 이동 중이라면(쿨타임 중이라면) 무시
        if (isGlobalTeleporting) return;

        if (other.CompareTag("Player"))
        {
            if (audioSource != null && teleportSound != null)
            {
                audioSource.PlayOneShot(teleportSound);
            }
            Transform playerRig = other.transform.root;
            StartCoroutine(TeleportSequence(playerRig));
        }
    }

    private IEnumerator TeleportSequence(Transform playerRig)
    {
        isGlobalTeleporting = true; // "나 이동 중이야! 다들 멈춰!"

        // 1. 페이드 아웃
        var screenFade = playerRig.GetComponentInChildren<OVRScreenFade>();
        if (screenFade != null) screenFade.FadeOut();

        yield return new WaitForSeconds(fadeDuration);

        // 2. 위치 이동
        playerRig.position = destinationPoint.position;
        // playerRig.rotation = destinationPoint.rotation; // 필요하면 주석 해제

        yield return new WaitForSeconds(0.1f);

        // 3. 페이드 인
        if (screenFade != null) screenFade.FadeIn();

        // 4. 쿨타임 대기 (도착 후 2초 동안은 포털에 닿아도 무시함)
        yield return new WaitForSeconds(cooldownTime);

        isGlobalTeleporting = false; // "이제 이동 끝! 다시 작동 가능."
    }
}