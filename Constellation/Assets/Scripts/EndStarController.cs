using UnityEngine;
using System.Collections.Generic;
using Oculus.Interaction; // ★ 필수 추가!

public class EndStarController : MonoBehaviour
{
    [Header("★ 맵 불빛 설정")]
    public GameObject targetMapLight;

    [Header("★ 클리어 조건 설정")]
    public bool useNormalStarCondition = false;
    public List<NormalStar> requiredStars;

    [Header("시네마틱 연결")]
    public StageClearCinematic cinematicScript;

    [Header("BlendShape 설정")]
    public SkinnedMeshRenderer starMesh;
    public string blendShapeName = "Star_full";
    private int blendShapeIndex;

    [Header("시간 설정")]
    public float requiredTime = 5f;
    private float timer = 0f;

    [Header("파티클")]
    public GameObject defaultParticle;
    public GameObject ShineParticle;
    public GameObject clearParticle;
    public GameObject ClearEffect;

    [Header("자동 회전 제어")]
    public AutoRotate autoRotateScript;

    private bool isHit = false;
    private bool isCleared = false;
    private Grabbable grabbableComponent; // ★ 잡기 컴포넌트

    [Header("★ 트랜스포머 연결 (필수)")]
    // ★ [추가] 트랜스포머 스크립트를 여기에 연결하세요
    public SpringResistanceTransformer resistanceTransformer;

    void Start()
    {
        if (starMesh == null)
            starMesh = GetComponent<SkinnedMeshRenderer>();

        blendShapeIndex = starMesh.sharedMesh.GetBlendShapeIndex(blendShapeName);

        if (clearParticle != null)
            clearParticle.SetActive(false);

        starMesh.SetBlendShapeWeight(blendShapeIndex, 100f);

        // ★ Grabbable 컴포넌트 찾아두기
        grabbableComponent = GetComponent<Grabbable>();

        if (resistanceTransformer == null)
            resistanceTransformer = GetComponent<SpringResistanceTransformer>();
    }

    void Update()
    {
        if (isCleared) return;

        bool canCharge = isHit;

        if (canCharge && useNormalStarCondition)
        {
            foreach (var star in requiredStars)
            {
                if (star != null && !star.IsActive)
                {
                    canCharge = false;
                    break;
                }
            }
        }

        if (canCharge)
        {
            timer += Time.deltaTime;
            float t = timer / requiredTime;
            t = Mathf.Clamp01(t);

            float blendValue = Mathf.Lerp(100f, 0f, t);
            starMesh.SetBlendShapeWeight(blendShapeIndex, blendValue);

            if (timer >= requiredTime)
            {
                StageClear();
            }
        }
        else
        {
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
                float t = timer / requiredTime;
                t = Mathf.Clamp01(t);
                float blendValue = Mathf.Lerp(100f, 0f, t);
                starMesh.SetBlendShapeWeight(blendShapeIndex, blendValue);
            }
        }
        isHit = false;
    }

    public void OnHit()
    {
        isHit = true;
    }

    void StageClear()
    {
        isCleared = true;
        Debug.Log("Stage Clear!");

        // ★ [핵심 추가] 트랜스포머에게 "클리어됐다"고 알림 (회전 재실행 방지)
        if (resistanceTransformer != null)
        {
            resistanceTransformer.isStageCleared = true;
        }

        // 1. 자동 회전 끄기 (이제 트랜스포머가 다시 켜지 않음)
        if (autoRotateScript != null) autoRotateScript.enabled = false;

        // ★ [순서 중요] 잡기를 해제하면 EndTransform이 호출됨
        // 위에서 isStageCleared를 true로 했으므로, 이제 안전하게 잡기를 풀 수 있음
        if (grabbableComponent != null)
        {
            grabbableComponent.enabled = false;
        }

        // 2. 파티클 효과
        if (clearParticle != null)
        {
            if (defaultParticle != null) defaultParticle.SetActive(false);
            if (ShineParticle != null) ShineParticle.SetActive(true);
            clearParticle.SetActive(true);
            ClearEffect.SetActive(true);
        }

        // 3. 맵 불빛 켜기
        if (targetMapLight != null) targetMapLight.SetActive(true);

        // 4. 시네마틱 재생 (이동)
        if (cinematicScript != null)
        {
            cinematicScript.PlaySequence();
        }
    }
}