// HandBloomBlocker.cs  (Left/Right Hand 프리팹의 루트에 붙이기)
using UnityEngine;

public class HandBloomBlocker : MonoBehaviour
{
    void Start()
    {
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            // Bloom 등 렌더링 레이어 기능을 쓰지 않도록 마스크 제거
            r.renderingLayerMask = 0;
        }
    }
}
