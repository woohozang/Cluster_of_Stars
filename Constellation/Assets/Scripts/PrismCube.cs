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

    [Header("깜빡임 방지 설정")]
    [Tooltip("신호가 끊겨도 이 시간(초) 동안은 켜진 상태를 유지합니다.")]
    public float keepAliveTime = 0.1f;

    private Material originalMaterial;
    private MeshRenderer meshRenderer;

    // 마지막으로 Activate가 호출된 시간 기록
    private float lastHitTime = -1f;

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
        DisableAllEffects();
    }

    /// <summary>
    /// LE.cs 스크립트가 이 함수를 호출하여 프리즘을 활성화시킵니다.
    /// </summary>
    public void Activate(RaycastHit hit)
    {
        // [핵심] 마지막으로 맞은 시간을 현재 시간으로 갱신
        lastHitTime = Time.time;

        // 켜져야 하는 상태라면 활성화 로직 실행
        // (매 프레임 호출되므로, 불필요한 연산을 줄이기 위해 상태 체크를 할 수도 있지만, 
        //  위치 업데이트가 필요하므로 계속 실행합니다.)
        EnableEffects(hit);
    }

    void LateUpdate()
    {
        // [핵심 로직 변경]
        // "이번 프레임에 안 맞았으면 끈다" -> "마지막으로 맞은 지 일정 시간이 지났으면 끈다"

        bool isAlive = (Time.time - lastHitTime) <= keepAliveTime;

        if (!isAlive)
        {
            DisableAllEffects();
        }
    }

    // 효과 켜기 (Activate 내부 로직 분리)
    void EnableEffects(RaycastHit hit)
    {
        // 1. 머티리얼 변경
        if (meshRenderer != null && meshRenderer.sharedMaterial != activatedMaterial)
        {
            meshRenderer.material = activatedMaterial;
        }

        // 2. 파티클 켜기
        if (P_Particle != null && !P_Particle.activeSelf)
            P_Particle.SetActive(true);

        // 3. 빛줄기 업데이트
        foreach (var config in outputRays)
        {
            if (config.rayObject == null) continue;

            if (!config.rayObject.activeSelf)
                config.rayObject.SetActive(true);

            // 시작 위치 설정 (레이가 맞은 지점)
            //config.rayObject.transform.position = hit.point;

            // 방향 설정 (로컬 방향 -> 월드 방향)
            Vector3 worldDir = transform.TransformDirection(config.localSplitDirection.normalized);
            config.rayObject.transform.forward = worldDir;

            // LE 스크립트 설정
            LE leScript = config.rayObject.GetComponent<LE>();
            if (leScript != null)
            {
                leScript.isReflectable = config.canReflect;
                leScript.isPrismOutput = true;
            }
        }
    }

    // 효과 끄기 (LateUpdate 내부 로직 분리)
    void DisableAllEffects()
    {
        // 1. 빛줄기 끄기
        foreach (var config in outputRays)
        {
            if (config.rayObject != null && config.rayObject.activeSelf)
                config.rayObject.SetActive(false);
        }

        // 2. 머티리얼 원상 복구
        if (meshRenderer != null && meshRenderer.sharedMaterial != originalMaterial)
        {
            meshRenderer.material = originalMaterial;
        }

        // 3. 파티클 끄기
        if (P_Particle != null && P_Particle.activeSelf)
            P_Particle.SetActive(false);
    }
}