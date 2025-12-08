using UnityEngine;
using Oculus.Interaction;

public class SpringResistanceTransformer : MonoBehaviour, ITransformer
{
    [Tooltip("TwoGrabRotateTransformer 연결")]
    [SerializeField, Interface(typeof(ITransformer))]
    private UnityEngine.Object _baseTransformer;
    private ITransformer BaseTransformer { get; set; }

    [Header("자동 회전 제어")]
    [Tooltip("AutoRotate 스크립트 연결")]
    [SerializeField] private MonoBehaviour _autoRotationScript;

    [Header("역감 햅틱 기본 설정")]
    [Tooltip("기본 진동 세기")]
    [Range(0f, 1f)] public float baseAmplitude = 0.2f;

    [Header("왼손/오른손 개별 설정 (중요)")]
    [Tooltip("왼손 햅틱 강도 배율 (별이 오른쪽으로 돌 때 왼손에 더 강한 자극을 주기 위함)")]
    [Range(1f, 5f)] public float leftHandMultiplier = 2.0f; // 왼손 기본 2배

    [Tooltip("오른손 햅틱 강도 배율")]
    [Range(0.1f, 3f)] public float rightHandMultiplier = 1.0f;

    [Header("동적 저항 설정")]
    [Tooltip("회전 반대 방향(역행)으로 힘을 줄 때 추가되는 강도")]
    [Range(0f, 5f)] public float resistanceMultiplier = 3.0f; // 저항 시 더 강하게

    [Tooltip("회전 같은 방향(순행)으로 밀어줄 때 줄어드는 비율")]
    [Range(0f, 1f)] public float assistanceMultiplier = 0.5f;

    [Range(0f, 1f)] public float minFrequency = 0.2f;
    [Range(0f, 1f)] public float maxFrequency = 1.0f;

    private Transform trackingSpace;
    private IGrabbable _grabbable;
    private Quaternion _restingRotation;
    private AutoRotate _autoRotateComponent;

    public void Initialize(IGrabbable grabbable)
    {
        _grabbable = grabbable;
        BaseTransformer = _baseTransformer as ITransformer;
        BaseTransformer.Initialize(grabbable);

        if (_autoRotationScript != null)
            _autoRotateComponent = _autoRotationScript as AutoRotate;

        if (Camera.main != null && Camera.main.transform.parent != null)
        {
            trackingSpace = Camera.main.transform.parent;
        }
        else
        {
            var rig = FindObjectOfType<OVRCameraRig>();
            if (rig != null) trackingSpace = rig.trackingSpace;
        }
    }

    public void BeginTransform()
    {
        BaseTransformer.BeginTransform();
        _restingRotation = _grabbable.Transform.rotation;

        if (_autoRotateComponent != null) _autoRotateComponent.enabled = false;
    }

    public void UpdateTransform()
    {
        // 1. 실제 회전 적용
        BaseTransformer.UpdateTransform();

        // --- 햅틱 계산 ---
        if (_autoRotateComponent != null)
        {
            float autoSpeedY = _autoRotateComponent.rotationSpeed.y;

            // 왼손 계산 (isLeftHand: true)
            ApplyHandHaptics(OVRInput.Controller.LTouch, autoSpeedY, true);

            // 오른손 계산 (isLeftHand: false)
            ApplyHandHaptics(OVRInput.Controller.RTouch, autoSpeedY, false);
        }
    }

    public void EndTransform()
    {
        BaseTransformer.EndTransform();
        StopHaptics();
        if (_autoRotateComponent != null) _autoRotateComponent.enabled = true;
    }

    // ★ [핵심 수정] isLeftHand 파라미터 추가 및 로직 단순화
    void ApplyHandHaptics(OVRInput.Controller controller, float autoSpeedY, bool isLeftHand)
    {
        float handTorque = CalculateHandTorque(controller);
        float handSpeed = Mathf.Abs(handTorque);

        // 1. 기본 햅틱 (잡고만 있어도 느껴짐)
        float currentAmplitude = baseAmplitude;

        // ★ [왼손 강제 강화] 
        // 왼손이면 설정한 배율(예: 2배)만큼 무조건 더 세게 줍니다.
        if (isLeftHand)
        {
            currentAmplitude *= leftHandMultiplier;
        }
        else
        {
            currentAmplitude *= rightHandMultiplier;
        }

        // 2. 동적 저항 계산 (움직일 때 추가 햅틱)
        if (handSpeed > 0.05f)
        {
            // 방향 비교 (반대면 저항)
            bool isResisting = (autoSpeedY * handTorque) < 0;

            if (isResisting)
            {
                // 역행(왼쪽으로 돌림): 속도에 비례해서 강도 "추가" (+)
                // 기존 강도에 + (속도 * 저항계수)를 더해서 확 세지게 만듦
                currentAmplitude += (handSpeed * 0.1f) * resistanceMultiplier;
            }
            else
            {
                // 순행(오른쪽으로 돌림): 강도 "감소" (*)
                currentAmplitude *= assistanceMultiplier;
            }
        }

        // 3. 주파수 및 최종 적용
        // 저항 중이거나 왼손일 때는 좀 더 거친 주파수 사용
        float targetFreq = (isLeftHand || (handSpeed > 0.05f && (autoSpeedY * handTorque) < 0)) ? maxFrequency : minFrequency;

        currentAmplitude = Mathf.Clamp01(currentAmplitude);
        OVRInput.SetControllerVibration(targetFreq, currentAmplitude, controller);
    }

    float CalculateHandTorque(OVRInput.Controller controller)
    {
        Vector3 localVel = OVRInput.GetLocalControllerVelocity(controller);
        Vector3 localPos = OVRInput.GetLocalControllerPosition(controller);

        Vector3 worldVel = localVel;
        Vector3 worldPos = localPos;

        if (trackingSpace != null)
        {
            worldVel = trackingSpace.TransformDirection(localVel);
            worldPos = trackingSpace.TransformPoint(localPos);
        }

        Vector3 r = worldPos - transform.position;
        return (r.z * worldVel.x) - (r.x * worldVel.z);
    }

    void StopHaptics()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }
}