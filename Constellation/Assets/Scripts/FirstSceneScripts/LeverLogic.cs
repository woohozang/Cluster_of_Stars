using UnityEngine;
using UnityEngine.SceneManagement;

public class LeverLogic : MonoBehaviour
{
    [Header("Settings")]
    public string tutorialSceneName = "StartScene";
    public float targetAngle = -59f; // 작동할 각도 (여유있게 설정)

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
            GoToTutorial();
        }
    }

    void GoToTutorial()
    {
        isTriggered = true;
        Debug.Log("🚀 레버 작동! 튜토리얼로 이동!");

        // 효과음 재생 등이 있다면 여기서

        SceneManager.LoadScene(tutorialSceneName);
    }

    // 유니티 내부 각도(0~360)를 보기 편한 -180~180으로 변환
    float GetInspectorAngle(float angle)
    {
        if (angle > 180) return angle - 360;
        return angle;
    }
}