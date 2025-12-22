using UnityEngine;

public class BifrostBridge : MonoBehaviour
{
    public Transform player;       // OVRCameraRig의 CenterEyeAnchor
    public Transform bridgeModel;  // 늘어날 다리 모델
    public float offset = 5.0f;    // 플레이어보다 얼마나 더 앞에 다리가 생길지
    public float startZ = 0f;      // 다리 시작점 Z 위치

    void Update()
    {
        // 플레이어가 시작점보다 뒤에 있으면 실행 안 함
        if (player.position.z < startZ) return;

        // 다리의 길이를 플레이어 위치 + 오프셋만큼 늘림
        // (다리의 Pivot이 (0,0,0) 즉, 시작점에 있어야 자연스럽게 늘어납니다)
        float newLength = (player.position.z - startZ) + offset;

        // 다리 길이가 줄어들지는 않게(뒤로 가도 다리는 유지)
        if (newLength > bridgeModel.localScale.z)
        {
            Vector3 newScale = bridgeModel.localScale;
            newScale.z = newLength;
            bridgeModel.localScale = newScale;

            // 다리 중심 위치도 조정 (Pivot이 중앙에 있는 Cube인 경우)
            // Pivot이 끝점에 있다면 이 코드는 필요 없습니다.
            // bridgeModel.position = new Vector3(bridgeModel.position.x, bridgeModel.position.y, startZ + newLength * 0.5f);
        }
    }
}