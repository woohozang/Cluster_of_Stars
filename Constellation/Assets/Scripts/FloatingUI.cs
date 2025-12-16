using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("위아래로 움직이는 범위 (높이)")]
    public float amplitude = 0.1f; // 0.1m 정도 움직임

    [Tooltip("움직이는 속도")]
    public float frequency = 1.0f; // 1초에 한 번 사이클

    private Vector3 startPos;

    void Start()
    {
        // 시작할 때의 원래 위치를 기억해둡니다.
        startPos = transform.localPosition;
    }

    void Update()
    {
        // Sin 함수를 이용해 -1 ~ 1 사이를 오가는 값을 만듭니다.
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;

        // X와 Z는 그대로 두고 Y값만 변경하여 위아래 움직임을 줍니다.
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
    }
}