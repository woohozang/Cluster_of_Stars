using UnityEngine;

public class DirectionalHaptics : MonoBehaviour
{
    [Header("��ƽ ���� ����")]
    [Tooltip("���� ������ ���� (0~1)")]
    [Range(0f, 1f)]
    public float strongAmplitude = 0.8f;

    [Tooltip("���� ������ ���� (0~1)")]
    [Range(0f, 1f)]
    public float weakAmplitude = 0.2f;

    [Header("�ΰ��� ����")]
    [Tooltip("�� ������ ũ�� ȸ���ؾ� ��ƽ�� �۵��մϴ�.")]
    public float rotationThreshold = 0.1f;

    private float previousYRotation;
    private bool isVibrating = false;

    // ��ũ��Ʈ�� ���۵� �� �� �� ȣ��˴ϴ�.
    void Start()
    {
        // ó�� ȸ�� ���� �����մϴ�.
        previousYRotation = transform.eulerAngles.y;
    }

    // �� �����Ӹ��� ȣ��˴ϴ�.
    void Update()
    {
        float currentYRotation = transform.eulerAngles.y;
        // ���� �����Ӱ� ���� �������� ȸ�� �� ���̸� ����մϴ�.
        float deltaRotation = currentYRotation - previousYRotation;

        // 359�� -> 0�� ó�� ���� ���ڱ� �����ϴ� ��츦 �����մϴ�.
        if (deltaRotation > 180f) { deltaRotation -= 360f; }
        if (deltaRotation < -180f) { deltaRotation += 360f; }

        // ȸ�� ��ȭ���� �ΰ��� ������ Ŭ ���� ��ƽ�� �����մϴ�.
        if (Mathf.Abs(deltaRotation) > rotationThreshold)
        {
            // �������� ȸ���� �� (���� ����)
            if (deltaRotation < 0)
            {
                // ���� ��Ʈ�ѷ��� ���ϰ�, �������� ���ϰ�
                OVRInput.SetControllerVibration(1, strongAmplitude, OVRInput.Controller.LTouch);
                OVRInput.SetControllerVibration(1, weakAmplitude, OVRInput.Controller.RTouch);
            }
            // ���������� ȸ���� �� (���� ����)
            else
            {
                // ���� ��Ʈ�ѷ��� ���ϰ�, �������� ���ϰ�
                OVRInput.SetControllerVibration(1, weakAmplitude, OVRInput.Controller.LTouch);
                OVRInput.SetControllerVibration(1, strongAmplitude, OVRInput.Controller.RTouch);
            }
            isVibrating = true;
        }
        else
        {
            // ȸ���� �����, ������ ������ �︮�� �־��ٸ� ������ ���ϴ�.
            if (isVibrating)
            {
                OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
                OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
                isVibrating = false;
            }
        }

        // ���� �������� ���� ���� ȸ�� ���� �����մϴ�.
        previousYRotation = currentYRotation;
    }

    // ������Ʈ�� ��Ȱ��ȭ�ǰų� �ı��� �� ������ Ȯ���� ���ϴ�.
    void OnDisable()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }
}