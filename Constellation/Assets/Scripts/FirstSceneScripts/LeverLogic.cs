using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LeverLogic : MonoBehaviour
{
    [Header("Settings")]
    public string tutorialSceneName = "StartScene";
    public float targetAngle = -59f; // 작동할 각도 (여유있게 설정)

    [Header("Transition Settings")]
    public float fadeDuration = 2.0f; // 페이드 아웃 걸리는 시간 (2초 추천)

    private bool isTriggered = false;

    void Update()
    {
        // One Grab Rotate Transformer가 transform.localRotation을 직접 변경합니다.
        // 우리는 그 각도만 읽어오면 됩니다.

        // 1. 현재 X축 각도 가져오기 (Inspector에 보이는 값으로 변환)
        float currentX = GetInspectorAngle(transform.localEulerAngles.x);

        // 2. 각도가 목표치(-55도)보다 더 작아지면 (더 당겨지면) 실행
        if (!isTriggered && currentX <= targetAngle)
        {
            StartCoroutine(TransitionSequence());
            //GoToTutorial();
        }
    }
    IEnumerator TransitionSequence()
    {
        isTriggered = true;
        OVRScreenFade fader = FindObjectOfType<OVRScreenFade>();

        if (fader != null)
        {
            fader.FadeOut();
        }
        else
        {
            fadeDuration = 0.5f;
        }
        /*AudioSource audio = GetComponent<AudioSource>();
        if (audio != null) audio.Play();*/
        yield return new WaitForSeconds(fadeDuration);

        SceneManager.LoadScene(tutorialSceneName);
    }
           

    // 유니티 내부 각도(0~360)를 보기 편한 -180~180으로 변환
    float GetInspectorAngle(float angle)
    {
        if (angle > 180) return angle - 360;
        return angle;
    }
}