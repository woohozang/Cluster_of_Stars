using UnityEngine;

public class PlatformFollower : MonoBehaviour
{
    Transform platform;
    Transform originalParent;

    void Awake()
    {
        originalParent = transform.parent;
    }

    public void SetPlatform(Transform p)
    {
        platform = p;
        // 엘리베이터를 부모로 붙임 (월드 위치 유지)
        transform.SetParent(platform, true);
    }

    public void ClearPlatform()
    {
        platform = null;
        // 원래 부모로 복귀
        transform.SetParent(originalParent, true);
    }
}
