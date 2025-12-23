using UnityEngine;

public class BifrostBridge : MonoBehaviour
{
    [Header("필수 할당")]
    public Transform player;        // 플레이어 (CenterEyeAnchor)
    public Transform startPoint;    // ★ 다리가 시작될 위치 (빈 오브젝트)

    [Header("설정")]
    public float extraLength = 5.0f; // 플레이어보다 얼마나 더 앞에 다리가 생길지

    private Material bridgeMat;

    void Start()
    {
        if (TryGetComponent(out Renderer r))
        {
            bridgeMat = r.material;
        }
    }

    void Update()
    {
        if (player == null || startPoint == null) return;

        // 1. 플레이어가 시작점 기준으로 얼마나 앞에 있는지 계산 (내적 사용)
        // (플레이어가 딴짓해서 옆으로 가도, 다리 방향(Forward) 기준 거리만 계산함)
        Vector3 playerVector = player.position - startPoint.position;
        float currentDist = Vector3.Dot(playerVector, startPoint.forward);

        // 시작점보다 뒤에 있거나, 너무 조금 갔으면 무시
        if (currentDist < 0) return;

        // 2. 목표 길이 계산
        float targetLength = currentDist + extraLength;

        // 기존 길이보다 작아지진 않게 (다리가 줄어들지 않음)
        if (targetLength > transform.localScale.z)
        {
            // (1) 스케일(길이) 적용
            Vector3 newScale = transform.localScale;
            newScale.z = targetLength;
            transform.localScale = newScale;

            // (2) ★ 위치 보정 (이게 핵심!)
            // 다리 중심을 '시작점'에서 '길이의 절반'만큼 앞으로 이동시킴
            // 결과적으로 뒤쪽 끝은 고정되고 앞쪽만 늘어나는 것처럼 보임
            transform.position = startPoint.position + (startPoint.forward * (targetLength / 2));

            // (3) 텍스처 타일링 조절 (선택사항: 텍스처 늘어짐 방지)
            if (bridgeMat != null)
            {
                // Emission 텍스처가 있다면 "_EmissionMap", 기본은 "_MainTex"
                // 쉐이더 그래프를 쓰신다면 프로퍼티 이름 확인 필요
                bridgeMat.SetTextureScale("_MainTex", new Vector2(1, targetLength));
            }
        }
    }
}