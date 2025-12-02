using UnityEngine;
using System.Collections.Generic;

public class EndStarController : MonoBehaviour
{
    [Header("★ 맵 불빛 설정 (직접 연결)")]
    [Tooltip("이 스테이지를 클리어하면 켜질 맵의 불빛(또는 UI) 오브젝트를 여기에 넣으세요.")]
    public GameObject targetMapLight;

    [Header("★ 클리어 조건 설정")]
    [Tooltip("체크하면 아래 리스트에 있는 '일반 별'들이 모두 빛나야만 게이지가 찹니다.")]
    public bool useNormalStarCondition = false;

    [Tooltip("조건이 켜져 있을 때, 빛나야 하는 일반 별들의 목록")]
    public List<NormalStar> requiredStars;

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

    void Start()
    {
        if (starMesh == null)
            starMesh = GetComponent<SkinnedMeshRenderer>();

        blendShapeIndex = starMesh.sharedMesh.GetBlendShapeIndex(blendShapeName);

        if (clearParticle != null)
            clearParticle.SetActive(false);

        // 시작할 때 쉐이프키 초기화
        starMesh.SetBlendShapeWeight(blendShapeIndex, 100f);

        // 시작할 때 타겟 불빛은 꺼두는 게 안전함 (혹시 켜져있을까봐)
        /* 만약 맵에서 직접 꺼두셨다면 이 코드는 없어도 됩니다. 
           혹시 자동으로 꺼지길 원하면 아래 주석을 해제하세요.
        */
        // if (targetMapLight != null) targetMapLight.SetActive(false);
    }

    void Update()
    {
        if (isCleared) return;

        // 1. 엔드 포인트가 빛을 맞았는지 확인
        // 2. 일반 별 조건이 켜져 있다면, 모든 별이 활성화되었는지 확인
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

        // 조건 만족 시 게이지 충전
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
        else // 조건 불만족 시 게이지 감소
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

        // 1. 자동 회전 끄기
        if (autoRotateScript != null) autoRotateScript.enabled = false;

        // 2. 파티클 효과 재생
        if (clearParticle != null)
        {
            if (defaultParticle != null) defaultParticle.SetActive(false);
            if (ShineParticle != null) ShineParticle.SetActive(true);
            clearParticle.SetActive(true);
            ClearEffect.SetActive(true);
        }

        // 3. [수정됨] 직접 연결된 맵 불빛 켜기
        if (targetMapLight != null)
        {
            targetMapLight.SetActive(true);
        }
    }
}