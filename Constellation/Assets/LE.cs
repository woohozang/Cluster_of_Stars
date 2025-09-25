using UnityEngine;

public class LE : MonoBehaviour
{
    [Header("Ray Settings")]
    public int maxReflections = 8;
    public float maxDistance = 50f;

    [Header("References")]
    public LineRenderer lineRenderer;
    public Color hitColor = Color.green;

    private MeshRenderer targetRenderer;
    private MaterialPropertyBlock block;
    private Color originalColor;

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
                    // �ſ� ������ �ݻ�
                    dir = Vector3.Reflect(dir, hit.normal);
                    pos = hit.point;
                }
                else if (hit.collider.CompareTag("Target"))
                {
                    // Target ������ ���� ����
                    ApplyHitColor(hit.collider.gameObject);
                    hitTarget = true;
                    break;
                }
                else
                {
                    // �ٸ� ��ü ������ ����
                    break;
                }
            }
            else
            {
                // �� �̻� ���� �� ������ �������� ����
                Vector3 endPos = pos + dir * maxDistance;
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, endPos);
                break;
            }
        }

        if (!hitTarget && targetRenderer != null)
        {
            // Target�� �� �°� ������ ���� ������ ����
            SetColor(originalColor);
            targetRenderer = null;
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
}
