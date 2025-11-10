using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// [최종본] 큐브를 잡는 순간, 자동 회전(AutoRotate)을 일시정지시키고 회전을 "딱" 고정시킵니다.
/// 잡고 있는 동안 'Update'에서 매 프레임 햅틱을 갱신하여 끊기지 않게 합니다.
/// 'AutoRotate'의 회전 방향을 감지하여 좌우 햅틱의 세기를 다르게 적용합니다.
/// </summary>
public class SpringResistanceTransformer : MonoBehaviour, ITransformer
{
    [Tooltip("실제로 회전을 계산할 원본 Transformer (예: TwoGrabRotateTransformer)")]
    [SerializeField, Interface(typeof(ITransformer))]
    private UnityEngine.Object _baseTransformer;
    private ITransformer BaseTransformer { get; set; }

    [Header("자동 회전 제어")]
    [Tooltip("큐브를 자동으로 회전시키는 스크립트(예: AutoRotate.cs)를 여기에 끌어다 놓으세요.")]
    [SerializeField]
    private MonoBehaviour _autoRotationScript;

    // --- [ ⭐ 수정된 햅틱 섹션 (방향성 추가) ] ---
    [Header("회전력 햅틱 (방향성)")]
    [Tooltip("강한 진동의 세기 (0~1)")]
    [Range(0f, 1f)] public float strongAmplitude = 0.8f;
    [Tooltip("약한 진동의 세기 (0~1)")]
    [Range(0f, 1f)] public float weakAmplitude = 0.2f;
    [Tooltip("강한 진동의 주파수 (0~1)")]
    [Range(0f, 1f)] public float strongFrequency = 0.9f;
    [Tooltip("약한 진동의 주파수 (0~1)")]
    [Range(0f, 1f)] public float weakFrequency = 0.2f;

    [Tooltip("이 값(Y축 회전 속도)의 절대값보다 커야 햅틱이 울립니다.")]
    public float rotationSpeedThreshold = 0.1f;
    // --- [ 수정 끝 ] ---

    private IGrabbable _grabbable;
    private Quaternion _restingRotation;
    private AutoRotate _autoRotateComponent; // AutoRotate 스크립트 캐시

    public void Initialize(IGrabbable grabbable)
    {
        _grabbable = grabbable;
        BaseTransformer = _baseTransformer as ITransformer;
        BaseTransformer.Initialize(grabbable);

        if (_autoRotationScript != null)
        {
            _autoRotateComponent = _autoRotationScript as AutoRotate;
        }
    }

    public void BeginTransform()
    {
        BaseTransformer.BeginTransform();
        _restingRotation = _grabbable.Transform.rotation;

        if (_autoRotateComponent != null)
        {
            _autoRotateComponent.enabled = false;
        }

        // [수정] BeginTransform 에서는 햅틱을 켜지 않습니다. (Update에서 켤 것이므로)
    }

    public void UpdateTransform()
    {
        // 1. 원본 Transformer가 내부 상태를 업데이트하도록 호출합니다.
        BaseTransformer.UpdateTransform();

        // 2. 큐브의 실제 회전 값을 '기준점'으로 "딱" 고정시킵니다.
        _grabbable.Transform.rotation = _restingRotation;

        // --- [ ⭐ 핵심 햅틱 로직 (Update로 이동) ] ---
        if (_autoRotateComponent == null) return;

        // 3. AutoRotate 스크립트에서 회전 방향(속도)을 읽어옵니다.
        float rotationSpeedY = _autoRotateComponent.rotationSpeed.y;

        // 4. 회전 방향에 따라 좌우 햅틱을 다르게 설정하여 매 프레임 갱신합니다.
        if (rotationSpeedY < -rotationSpeedThreshold) // 회전 방향: 왼쪽 (Y값이 음수)
        {
            OVRInput.SetControllerVibration(strongFrequency, strongAmplitude, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(weakFrequency, weakAmplitude, OVRInput.Controller.RTouch);
        }
        else if (rotationSpeedY > rotationSpeedThreshold) // 회전 방향: 오른쪽 (Y값이 양수)
        {
            OVRInput.SetControllerVibration(weakFrequency, weakAmplitude, OVRInput.Controller.RTouch);
            OVRInput.SetControllerVibration(strongFrequency, strongAmplitude, OVRInput.Controller.LTouch);
        }
        else // 회전력이 없음 (Y값이 0)
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }
    }

    public void EndTransform()
    {
        BaseTransformer.EndTransform();

        // [핵심] 큐브를 놓으면 햅틱을 확실히 끕니다.
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);

        if (_autoRotateComponent != null)
        {
            _autoRotateComponent.enabled = true;
        }
    }
}