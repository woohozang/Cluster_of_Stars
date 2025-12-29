using UnityEngine;
using System.Collections.Generic;

public class PrismCube : MonoBehaviour
{
    // 각 출력 빛줄기에 대한 설정을 담는 클래스
    [System.Serializable]
    public class OutputRayConfig
    {
        [Tooltip("출력될 빛줄기 오브젝트 (LE 스크립트가 있어야 함)")]
        public GameObject rayObject;

        [Tooltip("이 빛줄기가 다른 거울에 반사될 수 있는지 여부")]
        public bool canReflect = true;

        [Tooltip("큐브의 로컬 Z축(앞)을 기준으로 얼마나 옆으로 벌어질지 방향 설정")]
        public Vector3 localSplitDirection = new Vector3(0.3f, 0, 1.0f);
    }

    [Header("빛 분기 설정 (리스트로 관리)")]
    public List<OutputRayConfig> outputRays = new List<OutputRayConfig>();

    [Header("파티클 및 머티리얼")]
    public GameObject P_Particle;
    [Tooltip("빛이 닿았을 때 변경될 빛나는 머티리얼")]
    public Material activatedMaterial;

    private Material originalMaterial;
    private MeshRenderer meshRenderer;
    private bool wasHitThisFrame = false;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer가 이 오브젝트에 없습니다!", this);
            return;
        }

        originalMaterial = meshRenderer.material;

        // 시작할 때 모든 빛줄기를 꺼둡니다.
        foreach (var config in outputRays)
        {
            if (config.rayObject != null)
                config.rayObject.SetActive(false);
        }
    }

    /// <summary>
    /// LE.cs 스크립트가 이 함수를 호출하여 프리즘을 활성화시킵니다.
    /// </summary>
    public void Activate(RaycastHit hit)
    {
        wasHitThisFrame = true;

        // 머티리얼 변경 및 파티클 활성화
        if (meshRenderer != null && meshRenderer.material != activatedMaterial)
        {
            meshRenderer.material = activatedMaterial;
        }
        if (P_Particle != null) P_Particle.SetActive(true);

        // 리스트에 등록된 모든 빛줄기 활성화 및 설정 적용
        foreach (var config in outputRays)
        {
            if (config.rayObject == null) continue;

            // 1. 빛줄기 활성화
            config.rayObject.SetActive(true);

            // 2. 시작 위치 설정 (레이가 맞은 지점)
            config.rayObject.transform.position = hit.point;

            // 3. 방향 설정 (로컬 방향 -> 월드 방향)
            Vector3 worldDir = transform.TransformDirection(config.localSplitDirection.normalized);
            config.rayObject.transform.forward = worldDir;

            // 4. LE 스크립트 설정 (반사 가능 여부 적용)
            LE leScript = config.rayObject.GetComponent<LE>();
            if (leScript != null)
            {
                leScript.isReflectable = config.canReflect; // 여기서 리스트의 설정을 적용!
                leScript.isPrismOutput = true;
            }
        }
    }

    void LateUpdate()
    {
        // 이번 프레임에 Activate가 호출되지 않았다면 (빛이 빗나갔다면)
        if (!wasHitThisFrame)
        {
            // 모든 빛줄기 비활성화
            foreach (var config in outputRays)
            {
                if (config.rayObject != null)
                    config.rayObject.SetActive(false);
            }

            // 머티리얼 원상 복구
            if (meshRenderer != null && meshRenderer.material != originalMaterial)
            {
                meshRenderer.material = originalMaterial;
            }
            if (P_Particle != null) P_Particle.SetActive(false);
        }

        wasHitThisFrame = false;
    }
}