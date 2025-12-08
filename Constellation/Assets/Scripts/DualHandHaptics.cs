using UnityEngine;
using Oculus.Interaction;

public class DualHandHaptics : MonoBehaviour
{
    [Header("기본 햅틱 세기")]
    [Tooltip("회전을 주도하는 손에 줄 강한 진동 세기 (0~1)")]
    [Range(0f, 1f)] public float strongAmplitude = 0.8f;
    [Tooltip("따라가는 손에 줄 약한 진동 세기 (0~1)")]
    [Range(0f, 1f)] public float weakAmplitude = 0.2f;

    [Header("무게 설정 (Heavy Mode)")]
    [Tooltip("이 항목을 체크하면 '무거운 물체'로 인식하여 햅틱이 강해집니다.")]
    public bool isHeavy = false;

    [Tooltip("무거울 때 햅틱 강도를 몇 배 증폭시킬지 설정합니다.")]
    public float heavyMultiplier = 2.0f;

    [Header("햅틱 주파수 (속도 반응형)")]
    [Tooltip("회전이 느릴 때 주파수 (드르르...)")]
    [Range(0f, 1f)] public float minFrequency = 0.1f;
    [Tooltip("회전이 빠를 때 주파수 (드득!!!)")]
    [Range(0f, 1f)] public float maxFrequency = 0.8f;

    [Tooltip("이 속도(도/초) 이상이면 최대 주파수를 적용합니다.")]
    public float maxRotationSpeed = 180f;

    [Header("민감도 설정")]
    [Tooltip("최소한 이 속도 이상으로 돌려야 햅틱이 작동합니다.")]
    public float activationThreshold = 5.0f;

    private Grabbable _grabbable;
    private float _previousYRotation;

    void Awake()
    {
        _grabbable = GetComponent<Grabbable>();
    }

    void Start()
    {
        _previousYRotation = transform.eulerAngles.y;
    }

    void Update()
    {
        // 1. 회전 속도 및 방향 계산
        float currentY = transform.eulerAngles.y;
        float deltaRotation = currentY - _previousYRotation;

        // 360도 보정
        if (deltaRotation > 180f) deltaRotation -= 360f;
        if (deltaRotation < -180f) deltaRotation += 360f;

        float rotationSpeed = Mathf.Abs(deltaRotation) / Time.deltaTime;

        // 2. 잡고 있지 않거나 너무 느리면 햅틱 끔
        if (_grabbable.SelectingPointsCount < 2 || rotationSpeed < activationThreshold)
        {
            StopHaptics();
            _previousYRotation = currentY;
            return;
        }

        // 3. 각 손의 기여도(토크) 계산
        float leftContribution = CalculateTorqueContribution(OVRInput.Controller.LTouch);
        float rightContribution = CalculateTorqueContribution(OVRInput.Controller.RTouch);

        // 4. 주파수 계산 (속도 비례)
        float speedFactor = Mathf.Clamp01(rotationSpeed / maxRotationSpeed);
        float targetFrequency = Mathf.Lerp(minFrequency, maxFrequency, speedFactor);

        // 5. 무게(Heavy) 적용 강도 계산
        // isHeavy가 켜져 있으면 기본 강도에 배수를 곱함
        float currentStrong = isHeavy ? strongAmplitude * heavyMultiplier : strongAmplitude;
        float currentWeak = isHeavy ? weakAmplitude * heavyMultiplier : weakAmplitude;

        // 1.0을 넘지 않도록 제한
        currentStrong = Mathf.Clamp01(currentStrong);
        currentWeak = Mathf.Clamp01(currentWeak);

        // 6. 주도하는 손 판별 및 햅틱 적용
        bool isRotatingLeft = deltaRotation < 0;
        bool isLeftDominant = false;

        // 왼쪽/오른쪽 회전 방향에 따라 기여도가 큰 손을 찾음
        if (isRotatingLeft)
        {
            // 왼쪽 회전 시: 기여도가 더 큰(절대값) 쪽이 주도
            if (Mathf.Abs(leftContribution) > Mathf.Abs(rightContribution)) isLeftDominant = true;
            else isLeftDominant = false;
        }
        else // 오른쪽 회전
        {
            if (Mathf.Abs(leftContribution) > Mathf.Abs(rightContribution)) isLeftDominant = true;
            else isLeftDominant = false;
        }

        if (isLeftDominant)
        {
            // 왼손 주도: 왼손 강함, 오른손 약함
            OVRInput.SetControllerVibration(targetFrequency, currentStrong, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(targetFrequency, currentWeak, OVRInput.Controller.RTouch);
        }
        else
        {
            // 오른손 주도: 오른손 강함, 왼손 약함
            OVRInput.SetControllerVibration(targetFrequency, currentWeak, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(targetFrequency, currentStrong, OVRInput.Controller.RTouch);
        }

        _previousYRotation = currentY;
    }

    float CalculateTorqueContribution(OVRInput.Controller controller)
    {
        Vector3 handPos = OVRInput.GetLocalControllerPosition(controller);
        Vector3 handVel = OVRInput.GetLocalControllerVelocity(controller);
        Vector3 r = handPos - transform.position;

        // 외적의 Y성분 (수직 회전축에 대한 힘)
        return (r.z * handVel.x) - (r.x * handVel.z);
    }

    void StopHaptics()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }

    void OnDisable()
    {
        StopHaptics();
    }
}