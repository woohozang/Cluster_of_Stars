using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    [Tooltip("초당 회전 속도(도 단위)")]
    public Vector3 rotationSpeed = new Vector3(0, 30f, 0);

    private bool _isRotating = true;

    void Update()
    {
        if (!_isRotating) return;

        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
    }

    // 외부에서 끄고 켜기 위한 함수
    public void PauseRotate()
    {
        _isRotating = false;
    }

    public void ResumeRotate()
    {
        _isRotating = true;
    }
}
