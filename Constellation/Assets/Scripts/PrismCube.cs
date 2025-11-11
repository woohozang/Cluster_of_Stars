using UnityEngine;

public class PrismCube : MonoBehaviour
{
    [Header("출력되는 빛 (자식 오브젝트)")]
    public GameObject outputRayA;
    public GameObject outputRayB;

    [Header("빛 분기 설정")]
    [Tooltip("큐브의 로컬 Z축(앞)을 기준으로 얼마나 옆으로 벌어질지 정합니다.")]
    public Vector3 localSplitDirA = new Vector3(0.3f, 0, 1.0f);
    public Vector3 localSplitDirB = new Vector3(-0.3f, 0, 1.0f);

    // 이번 프레임에 빛이 닿았는지 확인하기 위한 플래그
    private bool wasHitThisFrame = false;

    void Awake()
    {
        // 시작할 때는 두 빛줄기를 꺼둡니다.
        if (outputRayA != null) outputRayA.SetActive(false);
        if (outputRayB != null) outputRayB.SetActive(false);
    }

    /// <summary>
    /// LE.cs 스크립트가 이 함수를 호출하여 프리즘을 활성화시킵니다.
    /// </summary>
    public void Activate(RaycastHit hit)
    {
        wasHitThisFrame = true;

        if (outputRayA == null || outputRayB == null) return;

        // 1. 두 빛줄기를 활성화합니다.
        outputRayA.SetActive(true);
        outputRayB.SetActive(true);

        // 2. 빛줄기의 시작 위치를 레이가 맞은 지점으로 설정합니다.
        outputRayA.transform.position = hit.point;
        outputRayB.transform.position = hit.point;

        // 3. 큐브의 현재 회전(transform)을 기준으로 빛의 방향을 계산합니다.
        // localSplitDirA를 월드 방향으로 변환합니다.
        Vector3 worldDirA = transform.TransformDirection(localSplitDirA.normalized);
        Vector3 worldDirB = transform.TransformDirection(localSplitDirB.normalized);

        // 4. 두 빛줄기 오브젝트가 해당 방향을 바라보게 합니다.
        // (LE.cs가 transform.forward를 기준으로 Raycast를 쏘기 때문)
        outputRayA.transform.forward = worldDirA;
        outputRayB.transform.forward = worldDirB;

        // outputRayA의 LE 스크립트를 가져와 isReflectable 값을 설정합니다.
        LE leA = outputRayA.GetComponent<LE>();
        if (leA != null)
        {
            leA.isReflectable = false; // 예를 들어 A는 반사 가능
            leA.isPrismOutput = true; // 프리즘에서 나온 빛이라고 표시
        }

        // outputRayB의 LE 스크립트를 가져와 isReflectable 값을 설정합니다.
        LE leB = outputRayB.GetComponent<LE>();
        if (leB != null)
        {
            leB.isReflectable = true; // 예를 들어 B는 반사 불가능
            leB.isPrismOutput = true; // 프리즘에서 나온 빛이라고 표시
        }
    }

    // Update가 끝난 후 호출되어 "꺼짐" 상태를 처리합니다.
    void LateUpdate()
    {
        // 이번 프레임에 Activate가 호출되지 않았다면 (빛이 빗나갔다면)
        if (!wasHitThisFrame)
        {
            if (outputRayA != null) outputRayA.SetActive(false);
            if (outputRayB != null) outputRayB.SetActive(false);
        }

        // 다음 프레임을 위해 플래그를 리셋합니다.
        wasHitThisFrame = false;
    }
}