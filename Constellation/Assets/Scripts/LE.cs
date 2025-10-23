using UnityEngine;

public class LE : MonoBehaviour
{
    [Header("Ray Settings")]
    public int maxReflections = 8;
    public float maxDistance = 50f;

    [Header("References")]
    public LineRenderer lineRenderer;
    public Color hitColor = Color.green;
    public GameObject hitParticlePrefab; //  추가 (Inspector에 연결할 프리팹)

    private MeshRenderer targetRenderer;
    private MaterialPropertyBlock block;
    private Color originalColor;
    private GameObject activeParticle;

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

        for (int i = 0; i < maxReflections; i++)
        {
            if (Physics.Raycast(pos, dir, out RaycastHit hit, maxDistance))
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);

                if (hit.collider.CompareTag("Reflector"))
                {
                    dir = Vector3.Reflect(dir, hit.normal);
                    pos = hit.point;
                }
                else if (hit.collider.CompareTag("Target"))
                {
                    ApplyHitColor(hit.collider.gameObject);
                    SpawnParticle(hit.point, hit.normal); // 파티클 생성
                    hitTarget = true;
                    break;
                }
                else
                {
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
            if (activeParticle != null)
                Destroy(activeParticle);

            if (targetRenderer != null)
            {
                SetColor(originalColor);
                targetRenderer = null;
            }
        }
    }

    void ApplyHitColor(GameObject target)
    {
        MeshRenderer mr = target.GetComponent<MeshRenderer>();
        if (mr == null) return;

        if (targetRenderer == null)
        {
            targetRenderer = mr;
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

    void SpawnParticle(Vector3 position, Vector3 normal)
    {
        if (hitParticlePrefab == null) return;

        if (activeParticle == null)
        {
            activeParticle = Instantiate(hitParticlePrefab, position, Quaternion.LookRotation(normal));
        }
        else
        {
            activeParticle.transform.position = position;
            activeParticle.transform.rotation = Quaternion.LookRotation(normal);
        }
    }
}
