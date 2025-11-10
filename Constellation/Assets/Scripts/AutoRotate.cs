using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 30, 0); // 초당 Y축으로 30도 회전

    void Update()
    {
        // 매 프레임마다 큐브를 회전시킵니다.
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}