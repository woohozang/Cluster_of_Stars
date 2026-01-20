using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

public class LeverHaptics : MonoBehaviour
{
    [Header("Settings")]
    public Grabbable grabbable;
    public OneGrabRotateTransformer transformer; // 각도 제한 정보 가져오기용

    [Header("Ratcheting (톱니바퀴 느낌)")]
    public float stepAngle = 10f; // 몇 도마다 걸리는 느낌을 줄 것인지 (예: 10도마다 '틱')
    public float hapticDuration = 0.05f; // 진동 지속 시간 (짧아야 '틱' 느낌이 남)
    [Range(0, 1)] public float hapticStrength = 1.0f; // 진동 세기 (강하게)

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip ratchetSound; // '틱' 소리 (기계음)
    public AudioClip completeSound; // '쿵' 완료 소리

    [Header("Events")]
    public UnityEvent onLeverComplete; // 완료 시 실행할 이벤트 (씬 이동 등)

    private int lastStepIndex = 0;
    private bool isCompleted = false;
    private float minAngle;
    private float maxAngle;

    void Start()
    {
        // OneGrabRotateTransformer에서 설정한 최소/최대 각도를 가져옴
        if (transformer != null)
        {
            minAngle = transformer.Constraints.MinAngle.Value; // 예: -60
            maxAngle = transformer.Constraints.MaxAngle.Value; // 예: 0
        }
    }

    void Update()
    {
        // 1. 잡고 있을 때만 작동
        if (grabbable.SelectingPointsCount == 0) return;

        // 2. 현재 각도 가져오기 (인스펙터 보기용 변환)
        float currentAngle = GetInspectorAngle(transform.localEulerAngles.x);

        // 3. 현재 각도가 몇 번째 '칸(Step)'에 있는지 계산
        // 예: 10도 단위라면, 5도는 0번 칸, 15도는 1번 칸
        int currentStepIndex = Mathf.Abs((int)(currentAngle / stepAngle));

        // 4. 칸이 바뀌었을 때 (톱니가 넘어갈 때) 햅틱 발생
        if (currentStepIndex != lastStepIndex)
        {
            PlayRatchetEffect();
            lastStepIndex = currentStepIndex;
        }

        // 5. 완료 체크 (목표 각도 근처에 도달했는지)
        // 레버가 아래로 내려가는 방식이므로 minAngle(-60)에 가까워지면 완료
        if (!isCompleted && currentAngle <= minAngle + 2f)
        {
            CompleteLever();
        }
        // 다시 올리면 완료 상태 초기화 (재사용 가능하게 하려면)
        else if (currentAngle > minAngle + 10f)
        {
            isCompleted = false;
        }
    }

    void PlayRatchetEffect()
    {
        // 소리 재생 (피치를 살짝 랜덤하게 주어 기계적인 느낌 강화)
        if (audioSource && ratchetSound)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(ratchetSound);
        }

        // 짧고 강한 햅틱 (탁! 치는 느낌)
        StartCoroutine(HapticPulse(hapticStrength, hapticDuration));
    }

    void CompleteLever()
    {
        isCompleted = true;
        Debug.Log("Lever Completed! (알릭스 스타일)");

        // 완료 소리 (쿵!)
        if (audioSource && completeSound)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(completeSound);
        }

        // 완료 햅틱 (길고 묵직하게)
        StartCoroutine(HapticPulse(1.0f, 0.2f));

        // 연결된 이벤트 실행 (씬 이동 등)
        onLeverComplete.Invoke();
    }

    // 햅틱 코루틴
    System.Collections.IEnumerator HapticPulse(float strength, float duration)
    {
        OVRInput.Controller controller = GetActiveController();
        if (controller != OVRInput.Controller.None)
        {
            OVRInput.SetControllerVibration(1, strength, controller);
            yield return new WaitForSeconds(duration);
            OVRInput.SetControllerVibration(0, 0, controller);
        }
    }

    OVRInput.Controller GetActiveController()
    {
        // 간단하게 오른쪽 핸드 트리거가 눌려있으면 오른쪽으로 간주
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.5f)
            return OVRInput.Controller.RTouch;
        else if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > 0.5f)
            return OVRInput.Controller.LTouch;

        return OVRInput.Controller.None;
    }

    float GetInspectorAngle(float angle)
    {
        if (angle > 180) return angle - 360;
        return angle;
    }
}