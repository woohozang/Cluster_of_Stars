using UnityEngine;

// 이 스크립트는 "Reflector" 태그가 붙은 큐브에 부착합니다.
public class ReflectorCube : MonoBehaviour
{
    [Header("머티리얼 설정")]
    [Tooltip("빛이 닿았을 때 변경될 빛나는 머티리얼")]
    public Material activatedMaterial; // 여기에 "ReflactLight" 머티리얼을 연결하세요.
    public GameObject R_Particle;

    private Material originalMaterial; // 큐브의 원래 머티리얼
    private MeshRenderer meshRenderer;
    public bool wasHitThisFrame = false;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer가 이 오브젝트에 없습니다!", this);
            return;
        }

        // 큐브의 원래 머티리얼을 저장합니다.
        originalMaterial = meshRenderer.material;
    }

    /// <summary>
    /// LE.cs가 호출하여 이 큐브를 활성화시킵니다.
    /// </summary>
    public void Activate()
    {
        wasHitThisFrame = true;

        // 머티리얼이 이미 activatedMaterial이 아니라면 변경합니다.
        if (meshRenderer.material != activatedMaterial)
        {
            meshRenderer.material = activatedMaterial;
            R_Particle.SetActive(true);
        }
    }

    // 모든 Update가 끝난 후 호출됩니다.
    void LateUpdate()
    {
        // 이번 프레임에 Activate()가 호출되지 않았다면 (빛이 빗나갔다면)
        if (!wasHitThisFrame)
        {
            // 머티리얼이 원래대로 돌아가야 한다면 변경합니다.
            if (meshRenderer.material != originalMaterial)
            {
                meshRenderer.material = originalMaterial;
            }
            R_Particle.SetActive(false);
        }

        // 다음 프레임을 위해 플래그를 리셋합니다.
        wasHitThisFrame = false;
    }
}