using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// 다른 Transformer를 감싸서(Wrapping) 회전 값에 가중치를 적용합니다.
/// [수정된 버전] 덮어쓰기(절대) 방식이 아닌,
/// 매 프레임의 회전 변화량(증가)을 감지하여 비율을 적용합니다.
/// </summary>
public class WeightedRotateTransformer : MonoBehaviour, ITransformer
{
    [Tooltip("실제로 회전을 계산할 원본 Transformer (예: TwoGrabRotateTransformer)")]
    [SerializeField, Interface(typeof(ITransformer))]
    private UnityEngine.Object _baseTransformer;
    private ITransformer BaseTransformer { get; set; }

    [Tooltip("회전 비율. 0.5로 설정하면 30도 회전 시 15도만 회전합니다.")]
    [Range(0f, 2f)]
    public float rotationScale = 0.5f;

    private IGrabbable _grabbable;

    // [수정됨] '시작 회전 값' 대신 '이전 프레임의 회전 값'을 저장할 변수
    private Quaternion _lastRotation;

    public void Initialize(IGrabbable grabbable)
    {
        _grabbable = grabbable;
        BaseTransformer = _baseTransformer as ITransformer;
        BaseTransformer.Initialize(grabbable);
    }

    public void BeginTransform()
    {
        BaseTransformer.BeginTransform();
        // [수정됨] 변환이 시작될 때의 현재 회전 값을 저장합니다.
        _lastRotation = _grabbable.Transform.rotation;
    }

    public void UpdateTransform()
    {
        // 1. [수정됨] 현재 회전 값을 '업데이트 전' 상태로 미리 저장합니다.
        Quaternion rotationBeforeUpdate = _grabbable.Transform.rotation;

        // 2. 원본 Transformer가 회전을 계산하고 *직접 적용*하도록 합니다. (예: 15도 -> 17도)
        BaseTransformer.UpdateTransform();

        // 3. 원본 Transformer가 적용한 '업데이트 후' 회전 값을 가져옵니다. (예: 17도)
        Quaternion rotationAfterUpdate = _grabbable.Transform.rotation;

        // 4. '업데이트 전'과 '업데이트 후'의 차이, 즉 '원본이 적용하려 한 회전 변화량(delta)'을 계산합니다.
        // (예: 17도 * inverse(15도) = 2도)
        Quaternion appliedDelta = rotationAfterUpdate * Quaternion.Inverse(rotationBeforeUpdate);

        // 5. 이 '회전 변화량(delta)'에 우리가 원하는 비율을 적용합니다.
        // (예: 2도 -> 1도)
        Quaternion scaledDelta = Quaternion.SlerpUnclamped(Quaternion.identity, appliedDelta, rotationScale);

        // 6. [수정됨] '업데이트 전' 상태에 '비율이 적용된 회전 변화량'을 더해 최종 적용합니다.
        // (예: 15도 * 1도 = 16도)
        // 이렇게 하면 원본 Transformer와 싸우지 않고 협력하게 됩니다.
        _grabbable.Transform.rotation = rotationBeforeUpdate * scaledDelta;
    }

    public void EndTransform()
    {
        BaseTransformer.EndTransform();
    }
}