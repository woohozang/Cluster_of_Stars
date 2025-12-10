using UnityEngine;                 // 유니티 기본 네임스페이스 (Transform, MonoBehaviour 등 사용)
using Oculus.Interaction;          // Meta XR Interaction SDK (ITransformer, IGrabbable 인터페이스 등 사용)

public class HeavyRotationDampingTransformer : MonoBehaviour, ITransformer
{                                   // MonoBehaviour + ITransformer 구현하는 스크립트

    [Header("원본 회전을 계산할 Transformer (예: TwoGrabRotateTransformer)")]
    [SerializeField, Interface(typeof(ITransformer))]
    private UnityEngine.Object _baseTransformer;
    // 인스펙터에서 드래그할 "기본 회전 계산기" (보통 TwoGrabRotateTransformer)
    // UnityEngine.Object 타입이지만, 실제로는 ITransformer로 캐스팅해서 씀

    private ITransformer _base;     // 진짜로 사용할 ITransformer 레퍼런스
    private IGrabbable _grabbable;  // 이 오브젝트를 잡는 Grabbable 정보

    [Header("회전 감쇠 속도")]
    [Tooltip("값이 작을수록 무겁게, 천천히 따라옵니다 (예: 1~3).")]
    public float rotationLerpSpeed = 1.5f;
    // 회전이 목표 각도로 얼마나 빠르게 따라갈지 결정하는 속도
    // 값이 작으면 별이 더 무겁게, 천천히 따라감

    private Quaternion _smoothedRotation;
    // "부드럽게 보간된 회전값"을 저장해두는 변수
    // 매 프레임 Slerp 결과가 여기에 저장됨

    public void Initialize(IGrabbable grabbable)
    {
        _grabbable = grabbable;                  // Grabbable 정보 저장

        _base = _baseTransformer as ITransformer;
        // 인스펙터에서 받은 Object를 ITransformer로 캐스팅
        // 보통: TwoGrabRotateTransformer 를 여기로 가져옴

        _base.Initialize(grabbable);
        // 원본 Transformer에게도 "너도 이 Grabbable을 쓴다"라고 초기화 해줌

        _smoothedRotation = grabbable.Transform.rotation;
        // 처음에는 현재 회전값을 "부드러운 회전"의 시작점으로 사용
    }

    public void BeginTransform()
    {
        _base.BeginTransform();
        // 잡기 시작할 때, 원본 Transformer(TwoGrabRotateTransformer)의 BeginTransform도 호출

        _smoothedRotation = _grabbable.Transform.rotation;
        // 잡기 시작 시점의 회전값을 다시 기준으로 잡음
    }

    public void UpdateTransform()
    {
        // 1) 먼저 원본 Transformer가 "원래라면 돌아갈 위치"를 계산하게 둠
        _base.UpdateTransform();
        // → 여기서 TwoGrabRotateTransformer가 손 움직임 기반으로 회전값을 갱신함

        Quaternion target = _grabbable.Transform.rotation;
        // 방금 _base.UpdateTransform()이 계산한 "목표 회전값"을 저장
        // 사용자가 손으로 돌렸다면, 그 직후의 결과 회전

        // 2) 그 위치로 바로 가지 말고, 서서히 보간해서 무겁게 만들기
        _smoothedRotation = Quaternion.Slerp(
            _smoothedRotation,           // 현재(저번 프레임까지)의 부드러운 회전값
            target,                      // 이번 프레임 원래 가야 할 목표 회전값
            rotationLerpSpeed * Time.deltaTime
        // t: 0~1 사이 값.
        // rotationLerpSpeed가 작을수록 느리게(무겁게) 따라가고,
        // Time.deltaTime 곱해서 프레임마다 조금씩 이동
        );

        _grabbable.Transform.rotation = _smoothedRotation;
        // 실제로 오브젝트에 적용하는 회전값은 "목표값 그대로"가 아니라
        // Slerp로 섞은 느린 회전값 => 시각적으로 무거운 느낌
    }

    public void EndTransform()
    {
        _base.EndTransform();
        // 잡기 끝났을 때, 원본 Transformer의 EndTransform도 호출
    }
}
