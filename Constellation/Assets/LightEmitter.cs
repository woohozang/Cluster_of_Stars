using UnityEngine;

public class LightEmitter : MonoBehaviour
{
    public int maxReflections = 5; // 최대 반사 횟수
    public float maxDistance = 50f; // Ray 길이
    public LineRenderer lineRenderer;

    void Update()
    {
        CastLight();
    }

    void CastLight()
    {
        Vector3 direction = transform.forward; // 기본 진행 방향
        Vector3 position = transform.position;

        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, position);

        for (int i = 0; i < maxReflections; i++)
        {
            if (Physics.Raycast(position, direction, out RaycastHit hit, maxDistance))
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);

                // 거울(Cube)에 맞으면 반사
                direction = Vector3.Reflect(direction, hit.normal);
                position = hit.point;
            }
            else
            {
                // 맞은 게 없으면 직선으로 끝
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, position + direction * maxDistance);
                break;
            }
        }
    }
}
