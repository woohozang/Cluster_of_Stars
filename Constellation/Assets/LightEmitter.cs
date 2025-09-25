using UnityEngine;

public class LightEmitter : MonoBehaviour
{
    public int maxReflections = 5; // �ִ� �ݻ� Ƚ��
    public float maxDistance = 50f; // Ray ����
    public LineRenderer lineRenderer;

    void Update()
    {
        CastLight();
    }

    void CastLight()
    {
        Vector3 direction = transform.forward; // �⺻ ���� ����
        Vector3 position = transform.position;

        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, position);

        for (int i = 0; i < maxReflections; i++)
        {
            if (Physics.Raycast(position, direction, out RaycastHit hit, maxDistance))
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);

                // �ſ�(Cube)�� ������ �ݻ�
                direction = Vector3.Reflect(direction, hit.normal);
                position = hit.point;
            }
            else
            {
                // ���� �� ������ �������� ��
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, position + direction * maxDistance);
                break;
            }
        }
    }
}
