using UnityEngine;

public class ElevatorRideZone : MonoBehaviour
{
    public Transform platform;   // 실제로 움직이는 플랫폼(부모)

    void Reset()
    {
        if (platform == null)
            platform = transform.parent; // 기본값: 자기 부모
    }

    void OnTriggerEnter(Collider other)
    {
        var follower = other.GetComponentInParent<PlatformFollower>();
        if (follower != null)
            follower.SetPlatform(platform);
    }

    void OnTriggerExit(Collider other)
    {
        var follower = other.GetComponentInParent<PlatformFollower>();
        if (follower != null)
            follower.ClearPlatform();
    }
}
