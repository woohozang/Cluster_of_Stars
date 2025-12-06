using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필수!
using System.Collections;

public class ScenePortal : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("이동할 씬의 정확한 이름 (예: GameScene)")]
    public string sceneName;

    [Tooltip("화면이 어두워지는 속도")]
    public float fadeDuration = 1.0f;

    private bool isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        // 이미 이동 중이 아니고, 플레이어라면 실행
        if (!isTeleporting && other.CompareTag("Player"))
        {
            // OVRCameraRig(플레이어 최상위 부모)를 찾습니다.
            Transform playerRig = other.transform.root;

            // 씬 전환 시퀀스 시작
            StartCoroutine(LoadSceneSequence(playerRig));
        }
    }

    private IEnumerator LoadSceneSequence(Transform playerRig)
    {
        isTeleporting = true;

        // 1. 페이드 아웃 (화면 검게 만들기)
        var screenFade = playerRig.GetComponentInChildren<OVRScreenFade>();
        if (screenFade != null)
        {
            screenFade.FadeOut();
        }

        // 화면이 완전히 어두워질 때까지 대기
        yield return new WaitForSeconds(fadeDuration);

        // 2. 씬 비동기 로드 (로딩 중 멈춤 현상 방지)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // 로딩이 끝날 때까지 대기
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // (참고: 새 씬이 로드되면 OVRScreenFade는 자동으로 초기화되어 화면이 밝아질 것입니다.)
    }
}