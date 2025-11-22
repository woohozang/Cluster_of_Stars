using UnityEngine;

public class SimpleElevator : MonoBehaviour
{
    [Header("설정")]
    public Rigidbody platformRb; // 승강기 플랫폼의 리지드바디
    public float moveSpeed = 2.0f;

    [Header("위치 설정 (Y축 높이)")]
    public float minHeight = 0.0f;
    public float maxHeight = 5.0f;

    private bool isMovingUp = false;
    private bool isMovingDown = false;

    void Start()
    {
        // 만약 인스펙터에서 연결 안 했다면 자동으로 찾기
        if (platformRb == null)
        {
            // 이 스크립트가 플랫폼에 붙어있다면 GetComponent
            platformRb = GetComponent<Rigidbody>();

            // 만약 부모에 붙어있고 자식이 플랫폼이라면.. (상황에 따라 수정)
            if (platformRb == null)
                platformRb = GetComponentInChildren<Rigidbody>();
        }
    }

    // 물리 연산은 반드시 FixedUpdate에서 해야 뚫고 지나가지 않습니다.
    void FixedUpdate()
    {
        if (isMovingUp)
        {
            // 목표 위치 계산
            Vector3 targetPos = platformRb.position + (Vector3.up * moveSpeed * Time.fixedDeltaTime);

            // 최대 높이 제한
            if (targetPos.y >= maxHeight)
            {
                targetPos.y = maxHeight;
                isMovingUp = false;
            }

            // 물리적으로 이동 (이게 플레이어를 밀어 올립니다)
            platformRb.MovePosition(targetPos);
        }

        if (isMovingDown)
        {
            // 목표 위치 계산
            Vector3 targetPos = platformRb.position + (Vector3.down * moveSpeed * Time.fixedDeltaTime);

            // 최소 높이 제한
            if (targetPos.y <= minHeight)
            {
                targetPos.y = minHeight;
                isMovingDown = false;
            }

            // 물리적으로 이동
            platformRb.MovePosition(targetPos);
        }
    }

    public void OnUpButtonPressed()
    {
        isMovingUp = true;
        isMovingDown = false;
    }

    public void OnDownButtonPressed()
    {
        isMovingDown = true;
        isMovingUp = false;
    }
}