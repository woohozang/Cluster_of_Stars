using UnityEngine;
using Oculus.Interaction; // 오큘러스 인터랙션 필수

public class EndStarController : MonoBehaviour
{
    [Header("★ 스테이지 설정")]
    [Tooltip("이 별이 몇 번째 스테이지의 엔딩인지 설정하세요. (0 = 1스테이지, 1 = 2스테이지...)")]
    public int stageIndex = 0;

    [Header("★ 클리어 조건 (직전 별 감지)")]
    [Tooltip("엔드 포인트에 빛을 쏘는 바로 직전의 별(또는 큐브)을 넣으세요. (NormalStar 또는 ReflectorCube)")]
    public GameObject lastStarToCheck;

    [Header("BlendShape 설정 (게이지)")]
    public SkinnedMeshRenderer starMesh;
    public string blendShapeName = "Star_full";
    private int blendShapeIndex;

    [Header("시간 설정")]
    [Tooltip("게이지가 꽉 차는데 걸리는 시간")]
    public float requiredTime = 5f;
    private float timer = 0f;

    [Header("제어할 컴포넌트 (자동 할당 시도함)")]
    public AutoRotate autoRotateScript;
    public SpringResistanceTransformer resistanceTransformer;
    private Grabbable grabbableComponent;

    // 내부 상태 변수
    private bool isHit = false;    // 이번 프레임에 빛이 닿았는가?
    private bool isCleared = false; // 이미 클리어했는가?

    void Start()
    {
        // 1. BlendShape 초기화
        if (starMesh == null) starMesh = GetComponent<SkinnedMeshRenderer>();
        if (starMesh != null)
        {
            blendShapeIndex = starMesh.sharedMesh.GetBlendShapeIndex(blendShapeName);
            starMesh.SetBlendShapeWeight(blendShapeIndex, 100f); // 100(비어있음)으로 시작
        }

        // 2. 물리/상호작용 컴포넌트 가져오기
        grabbableComponent = GetComponent<Grabbable>();
        if (resistanceTransformer == null) resistanceTransformer = GetComponent<SpringResistanceTransformer>();
        if (autoRotateScript == null) autoRotateScript = GetComponent<AutoRotate>();
    }

    // 실행 순서 이슈 방지를 위해 LateUpdate 사용
    void LateUpdate()
    {
        if (isCleared) return;

        // --- 1. 충전 가능 여부 판단 ---
        bool canCharge = isHit; // 물리적으로 Ray가 닿았는가?

        // 직전 별(Last Star)이 켜져 있는지 확인 (꼼수 방지)
        if (canCharge && lastStarToCheck != null)
        {
            bool isLastStarActive = false;

            // (A) NormalStar 스크립트인지 확인
            if (lastStarToCheck.TryGetComponent(out NormalStar normalStar))
            {
                if (normalStar.IsActive) isLastStarActive = true;
            }
            // (B) ReflectorCube 스크립트인지 확인
            else if (lastStarToCheck.TryGetComponent(out ReflectorCube reflector))
            {
                // ReflectorCube.cs에 'isHit' 변수가 있어야 함 (이전 질문에서 수정함)
                if (reflector.isHit) isLastStarActive = true;
            }

            // 직전 별이 꺼져있다면 충전 불허
            if (!isLastStarActive)
            {
                canCharge = false;
            }
        }

        // --- 2. 게이지 충전 및 쉐이프키 애니메이션 ---
        if (canCharge)
        {
            timer += Time.deltaTime;
            float t = timer / requiredTime;
            t = Mathf.Clamp01(t);

            // 게이지 차오르는 연출 (100 -> 0)
            if (starMesh != null)
                starMesh.SetBlendShapeWeight(blendShapeIndex, Mathf.Lerp(100f, 0f, t));

            // 시간 충족 시 클리어
            if (timer >= requiredTime)
                StageClear();
        }
        else
        {
            // 조건 불충족 시 게이지 감소 (2배 속도로 빠르게 감소)
            if (timer > 0f)
            {
                timer -= Time.deltaTime * 2f;
                if (timer < 0) timer = 0;

                float t = timer / requiredTime;
                t = Mathf.Clamp01(t);
                if (starMesh != null)
                    starMesh.SetBlendShapeWeight(blendShapeIndex, Mathf.Lerp(100f, 0f, t));
            }
        }

        // Raycast 방식은 매 프레임 호출하므로 false로 리셋
        isHit = false;
    }

    // 외부(Ray)에서 호출하는 함수
    public void OnHit()
    {
        isHit = true;
    }

    void StageClear()
    {
        isCleared = true;
        Debug.Log($"Stage {stageIndex + 1} Cleared!");

        // 1. 별의 물리 기능 비활성화 (손에서 놓기, 회전 멈춤, 햅틱 끄기)
        if (resistanceTransformer != null) resistanceTransformer.isStageCleared = true;
        if (grabbableComponent != null) grabbableComponent.enabled = false;
        if (autoRotateScript != null) autoRotateScript.enabled = false;

        // 2. ★ 매니저에게 엔딩 연출 위임
        if (StageEventManager.Instance != null)
        {
            StageEventManager.Instance.PlayEnding(stageIndex);
        }
        else
        {
            Debug.LogError("씬에 StageEventManager가 없습니다! 빈 오브젝트를 만들고 스크립트를 추가해주세요.");
        }
    }
}