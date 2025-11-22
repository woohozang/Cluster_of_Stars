using UnityEngine;

public class EndStarController : MonoBehaviour
{
    [Header("BlendShape 설정")]
    public SkinnedMeshRenderer starMesh;
    public string blendShapeName = "Star_full";
    private int blendShapeIndex;

    [Header("시간 설정")]
    public float requiredTime = 5f;    // 빛이 닿아야 하는 시간
    private float timer = 0f;

    [Header("파티클")]
    public GameObject defaultParticle;
    public GameObject ShineParticle;
    public GameObject clearParticle;   // SetActive()로 관리

    private bool isHit = false;
    private bool isCleared = false;

    void Start()
    {
        if (starMesh == null)
            starMesh = GetComponent<SkinnedMeshRenderer>();

        blendShapeIndex = starMesh.sharedMesh.GetBlendShapeIndex(blendShapeName);

        if (clearParticle != null)
            clearParticle.SetActive(false);

        // BlendShape 기본 100으로 시작
        starMesh.SetBlendShapeWeight(blendShapeIndex, 100f);
    }

    void Update()
    {
        if (isCleared) return;

        if (isHit)
        {
            // 타이머 증가
            timer += Time.deltaTime;

            float t = timer / requiredTime;
            t = Mathf.Clamp01(t);

            // 100 → 0 으로 줄어드는 값
            float blendValue = Mathf.Lerp(100f, 0f, t);
            starMesh.SetBlendShapeWeight(blendShapeIndex, blendValue);

            // 5초 경과 → 클리어 처리
            if (timer >= requiredTime)
            {
                StageClear();
            }
        }
        else
        {
            // 빛이 닿지 않을 때 복귀(선택)
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
                float t = timer / requiredTime;
                t = Mathf.Clamp01(t);

                float blendValue = Mathf.Lerp(100f, 0f, t);
                starMesh.SetBlendShapeWeight(blendShapeIndex, blendValue);
            }
        }

        // 매 프레임 hit 값 초기화 (Ray 스크립트가 매 프레임 OnHit() 호출해야 함)
        isHit = false;
    }

    // Light(레이저) 스크립트에서 매 프레임 호출해줄 함수
    public void OnHit()
    {
        isHit = true;
    }

    void StageClear()
    {
        isCleared = true;

        Debug.Log("Stage Clear!");

        if (clearParticle != null)
        {
            defaultParticle.SetActive(false);
            ShineParticle.SetActive(true);
            clearParticle.SetActive(true);
        }
        // 추가: 문 열기 / 다음 스테이지 로딩 등 호출 가능
    }
}
