using UnityEngine;

public class NormalStar : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("빛이 닿았을 때 활성화될 파티클")]
    public GameObject hitParticle;

    // 마지막으로 빛이 닿은 시간 기록
    private float lastHitTime = -10f;

    // [핵심 수정] 0.1초 이내에 빛을 맞았다면 '켜짐'으로 인정 (타이밍 문제 해결)
    public bool IsActive => (Time.time - lastHitTime) < 0.1f;

    void Start()
    {
        if (hitParticle != null)
            hitParticle.SetActive(false);
    }

    // LE.cs에서 호출
    public void OnHit()
    {
        // 빛이 닿으면 현재 시간을 기록
        lastHitTime = Time.time;
    }

    void Update()
    {
        // IsActive 상태에 따라 파티클 끄고 켜기
        if (IsActive)
        {
            if (hitParticle != null && !hitParticle.activeSelf)
                hitParticle.SetActive(true);
        }
        else
        {
            if (hitParticle != null && hitParticle.activeSelf)
                hitParticle.SetActive(false);
        }
    }
}