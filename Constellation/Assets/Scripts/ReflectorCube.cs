using UnityEngine;

// 이 스크립트는 "Reflector" 태그가 붙은 큐브에 부착합니다.
public class ReflectorCube : MonoBehaviour
{
    [Header("머티리얼 설정")]
    [Tooltip("빛이 닿았을 때 변경될 빛나는 머티리얼")]
    public Material activatedMaterial;
    public GameObject R_Particle;

    private Material originalMaterial;
    private MeshRenderer meshRenderer;

    // ★ [수정 1] 변수 이름을 EndStarController가 찾는 'isHit'으로 변경했습니다.
    public bool isHit = false;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer가 이 오브젝트에 없습니다!", this);
            return;
        }

        originalMaterial = meshRenderer.material;
    }

    /// <summary>
    /// 빛을 쏘는 스크립트(LaserEmitter 등)가 호출하여 이 큐브를 활성화시킵니다.
    /// </summary>
    public void Activate()
    {
        // ★ [수정 2] 빛이 닿았으므로 true로 설정
        isHit = true;

        if (meshRenderer.material != activatedMaterial)
        {
            meshRenderer.material = activatedMaterial;
            if (R_Particle != null) R_Particle.SetActive(true);
        }
    }

    // 모든 Update가 끝난 후 호출됩니다.
    void LateUpdate()
    {
        // ★ [수정 3] 이번 프레임에 빛이 닿지 않았다면 (isHit이 false라면)
        if (!isHit)
        {
            if (meshRenderer.material != originalMaterial)
            {
                meshRenderer.material = originalMaterial;
            }
            if (R_Particle != null) R_Particle.SetActive(false);
        }

        // ★ [수정 4] 다음 프레임을 위해 리셋
        // 주의: EndStarController가 이 값을 읽기 전에 리셋되면 안 되므로, 
        // Project Settings > Script Execution Order에서 EndStarController를 먼저 실행되게 설정하는 것이 좋습니다.
        isHit = false;
    }
}