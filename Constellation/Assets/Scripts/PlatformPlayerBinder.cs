using UnityEngine;

public class PlatformPlayerBinder : MonoBehaviour
{
    void OnCollisionEnter(Collision c)
    {
        var follower = c.collider.GetComponentInParent<PlatformFollower>();
        if (follower != null)
            follower.SetPlatform(transform);
    }

    void OnCollisionExit(Collision c)
    {
        var follower = c.collider.GetComponentInParent<PlatformFollower>();
        if (follower != null)
            follower.ClearPlatform();
    }
}
