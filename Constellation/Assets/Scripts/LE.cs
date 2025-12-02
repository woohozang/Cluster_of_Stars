using UnityEngine;

public class LE : MonoBehaviour
{
    [Header("Ray Settings")]
    public int maxReflections = 30;
    public float maxDistance = 50f;

    [Header("References")]
    public LineRenderer lineRenderer;
    public Color hitColor = Color.green;
    //public GameObject hitParticlePrefab; //  추가 (Inspector에 연결할 프리팹)
    //public GameObject ClearParticle;

    private MeshRenderer targetRenderer;
    private MaterialPropertyBlock block;
    private Color originalColor;
    private GameObject activeParticle;

    public bool isReflectable = true; // 이 빛줄기가 반사 큐브에 의해 반사될 수 있는지
    public bool isPrismOutput = false; // 이 빛줄기가 프리즘에서 나왔는지 (옵션)

    void Awake()
    {
        block = new MaterialPropertyBlock();
    }

    void Update()
    {
        CastAndRender();
    }

    void CastAndRender()
    {
        Vector3 pos = transform.position;
        Vector3 dir = transform.forward;

        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, pos);

        bool hitTarget = false;

        // C# 8.0 이상을 사용한다고 가정하고 'out var' 대신 'out RaycastHit'을 유지합니다.
        // (사용자 코드를 최대한 존중)

        for (int i = 0; i < maxReflections; i++)
        {
            if (Physics.Raycast(pos, dir, out RaycastHit hit, maxDistance))
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);

                if (hit.collider.CompareTag("Reflector"))
                {
                    ReflectorCube reflector = hit.collider.GetComponent<ReflectorCube>();
                    if (reflector != null)
                    {
                        reflector.Activate();    
                    }
                    if (isReflectable /*&& !isPrismOutput*/) // isPrismOutput을 체크하면 프리즘 출력이 반사 안 됨
                    {
                        dir = Vector3.Reflect(dir, hit.normal);
                        pos = hit.point;
                        //pos = hit.point + dir * 0.01f;
                    }
                    else
                    {
                        // 반사 불가능한 빛이면 여기서 멈춤
                        break;
                    }
                }
                else if (hit.collider.CompareTag("Target"))
                {
                    ApplyHitColor(hit.collider.gameObject);
                    /*hitParticlePrefab.SetActive(true);// 파티클 생성
                    ClearParticle.SetActive(true);
                    hitTarget = true;
                    break;*/
                    var star = hit.collider.GetComponent<EndStarController>();
                    if (star != null)
                        star.OnHit();

                    hitTarget = true;
                    break;
                }
                // --- ▼ 프리즘 로직 추가 ▼ ---
                else if (hit.collider.CompareTag("Prism"))
                {

                    // 프리즘에 닿으면, 해당 프리즘을 활성화시키고 현재 레이는 여기서 멈춤
                    PrismCube prism = hit.collider.GetComponent<PrismCube>();
                    
                    if (prism != null)
                    {
                        // 프리즘에게 빛이 닿았다고 알림
                        prism.Activate(hit);
                       
                       
                    }
                    
                    // 현재 LineRenderer는 여기서 종료
                    break;
                }
                // --- ▲ 프리즘 로직 추가 ▲ ---
                else
                {
                    // 1. 혹시 부딪힌 물체에 NormalStar 스크립트가 있는지 확인
                    NormalStar normalStar = hit.collider.GetComponent<NormalStar>();

                    if (normalStar != null)
                    {
                        // 2. 있다면 OnHit() 호출해서 파티클 켜기
                        normalStar.OnHit();

                        // 3. 별에 빛이 닿았으니 여기서 레이 멈춤 (뚫고 지나가지 않음)
                        // 만약 뚫고 지나가게 하고 싶으면 이 break;를 지우고
                        // pos = hit.point + dir * 0.01f; 로 업데이트 해주면 됩니다.
                        break;
                    }

                    // NormalStar도 아니고 다른 태그도 아니면 그냥 벽이므로 멈춤
                    break;
                }
            }
            else
            {
                Vector3 endPos = pos + dir * maxDistance;
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, endPos);
                break;
            }
        }

        if (!hitTarget)
        {
            // (기존 파티클 및 색상 초기화 로직)
           // hitParticlePrefab.SetActive(false);
          //  ClearParticle.SetActive(false);
            if (activeParticle != null)
                Destroy(activeParticle);

            if (targetRenderer != null)
            {
                SetColor(originalColor);
                targetRenderer = null;
            }
        }
    }

    // (ApplyHitColor 및 SetColor 함수는 기존 코드 그대로 사용)
    void ApplyHitColor(GameObject target)
    {
        MeshRenderer mr = target.GetComponent<MeshRenderer>();
        if (mr == null) return;

        if (targetRenderer == null)
        {
            targetRenderer = mr;
            // 주의: sharedMaterial.GetColor는 에디터에서만 안전할 수 있습니다.
            // 런타임에서는 mr.material.GetColor를 사용하고 캐시하는 것이 좋습니다.
            // 여기서는 원본 코드를 유지합니다.
            originalColor = mr.sharedMaterial.GetColor("_BaseColor");
        }

        SetColor(hitColor);
    }

    void SetColor(Color c)
    {
        if (targetRenderer == null) return;

        targetRenderer.GetPropertyBlock(block);
        block.SetColor("_BaseColor", c);
        targetRenderer.SetPropertyBlock(block);
    }
}