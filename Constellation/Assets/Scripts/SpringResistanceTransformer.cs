using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// 자동으로 회전하는 별에 “스프링 같은 저항”을 주면서,
/// 잡힌 동안에는 사용자가 직접 회전시킬 수 있고,
/// 회전 방향 + 손의 주도권(컨트롤러 속도)에 따라 좌/우 햅틱을 다르게 준다.
/// </summary>
public class SpringResistanceTransformer : MonoBehaviour, ITransformer
{
    [Tooltip("실제 회전을 담당하는 기본 Transformer (예: TwoGrabRotateTransformer)")]
    [SerializeField, Interface(typeof(ITransformer))]
    private UnityEngine.Object _baseTransformer;
    private ITransformer BaseTransformer { get; set; }

    [Header("자동 회전 제어")]
    [Tooltip("별을 자동으로 회전시키는 스크립트(AutoRotate)를 지정")]
    [SerializeField]
    private AutoRotate _autoRotate;

    // ------------------ 햅틱 세팅 ------------------

    [Header("기본 회전 저항 햅틱")]
    [Tooltip("저항감을 주는 기본 강한 진폭")]
    [Range(0f, 1f)] public float baseStrongAmplitude = 0.7f;
    [Range(0f, 1f)] public float baseWeakAmplitude = 0.25f;
    [Range(0f, 1f)] public float baseStrongFrequency = 0.85f;
    [Range(0f, 1f)] public float baseWeakFrequency = 0.25f;

    [Header("손 주도권 보정")]
    [Tooltip("컨트롤러 속도 기준(이 값보다 커야 '움직이고 있다'고 판단)")]
    public float velocityThreshold = 0.1f;
    [Tooltip("주도하는 손에 추가로 더해줄 진폭(+α)")]
    [Range(0f, 1f)] public float leaderAmplitudeBoost = 0.2f;

    [Header("회전 속도 기준")]
    [Tooltip("별의 AutoRotate 회전 속도 Y 성분 절대값이 이 값보다 커야 저항 햅틱을 냄")]
    public float rotationSpeedThreshold = 0.1f;

    private IGrabbable _grabbable;

    // 회전 축 (기본은 Y축 기준 회전한다고 가정)
    private Vector3 _worldRotationAxis = Vector3.up;

    public void Initialize(IGrabbable grabbable)
    {
        _grabbable = grabbable;
        BaseTransformer = _baseTransformer as ITransformer;
        BaseTransformer.Initialize(grabbable);

        // 회전축을 오브젝트의 up 기준으로 쓰고 싶으면 아래 한 줄 사용
        _worldRotationAxis = transform.up;
    }

    public void BeginTransform()
    {
        BaseTransformer.BeginTransform();

        // 잡는 순간 자동 회전은 일시정지
        if (_autoRotate != null)
        {
            _autoRotate.PauseRotate();
        }

        // 잡힌 직후에도 바로 저항 햅틱이 느껴지도록
        UpdateHaptics();
    }

    public void UpdateTransform()
    {
        // 원래 회전 로직(두 손 회전 등)을 그대로 수행
        BaseTransformer.UpdateTransform();

        // 잡는 동안 매 프레임 햅틱 갱신
        UpdateHaptics();
    }

    public void EndTransform()
    {
        BaseTransformer.EndTransform();

        // 햅틱 꺼주기
        StopHaptics();

        // 놓으면 다시 자동 회전 재개
        if (_autoRotate != null)
        {
            _autoRotate.ResumeRotate();
        }
    }

    // ------------------ 내부 로직 ------------------

    /// <summary>
    /// 별의 회전 방향(+Y / -Y)에 따라 "기본 저항"을 어느 손에 줄지 정하고,
    /// 컨트롤러 속도(누가 더 세게 움직이는지)에 따라 주도권 보정을 한다.
    /// </summary>
    private void UpdateHaptics()
    {
        if (_autoRotate == null)
        {
            StopHaptics();
            return;
        }

        float autoY = _autoRotate.rotationSpeed.y;

        // 회전이 거의 없다면(정지에 가까우면) 햅틱도 끔
        if (Mathf.Abs(autoY) < rotationSpeedThreshold)
        {
            StopHaptics();
            return;
        }

        // -------- 1) 기본 저항: 회전 방향 반대손에 더 강하게 --------
        // 오른쪽(시계 방향, +Y)으로 돌고 있다 → 왼손이 더 많이 버티는 느낌을 주고 싶다고 가정
        bool rotatingRight = autoY > 0f;

        float leftAmp = baseWeakAmplitude;
        float rightAmp = baseWeakAmplitude;
        float leftFreq = baseWeakFrequency;
        float rightFreq = baseWeakFrequency;

        if (rotatingRight)
        {
            // 오른쪽으로 돌아갈 때 왼손에 기본 저항 강하게
            leftAmp = baseStrongAmplitude;
            leftFreq = baseStrongFrequency;
        }
        else
        {
            // 왼쪽(-Y)으로 돌아갈 때 오른손에 기본 저항 강하게
            rightAmp = baseStrongAmplitude;
            rightFreq = baseStrongFrequency;
        }

        // -------- 2) 손 주도권 판단: 어느 손이 더 세게 움직이는가 --------
        // 로컬 컨트롤러 속도
        Vector3 lVelLocal = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch);
        Vector3 rVelLocal = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);

        // 회전축 방향 성분만 보려면 축으로 투영 (월드 회전축 기준)
        Vector3 lVelWorld = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch);
        Vector3 rVelWorld = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);

        float lAlongAxis = Vector3.Dot(lVelWorld, _worldRotationAxis);
        float rAlongAxis = Vector3.Dot(rVelWorld, _worldRotationAxis);

        float lMag = Mathf.Abs(lAlongAxis);
        float rMag = Mathf.Abs(rAlongAxis);

        // 둘 다 거의 안 움직이면 기본 저항만 유지
        if (lMag < velocityThreshold && rMag < velocityThreshold)
        {
            // do nothing, keep base values
        }
        else if (lMag > rMag)
        {
            // 왼손이 더 세게 움직이는 중 → 왼손이 주도
            leftAmp = Mathf.Clamp01(leftAmp + leaderAmplitudeBoost);
        }
        else if (rMag > lMag)
        {
            // 오른손이 더 세게 움직이는 중 → 오른손이 주도
            rightAmp = Mathf.Clamp01(rightAmp + leaderAmplitudeBoost);
        }

        // -------- 3) 최종 햅틱 적용 --------
        OVRInput.SetControllerVibration(leftFreq, leftAmp, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(rightFreq, rightAmp, OVRInput.Controller.RTouch);
    }

    private void StopHaptics()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }
}
