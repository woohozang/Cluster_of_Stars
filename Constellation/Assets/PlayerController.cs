using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("�÷��̾��� �̵� �ӵ��Դϴ�.")]
    public float moveSpeed = 2.0f;

    [Header("Rotation Settings")]
    [Tooltip("ȸ�� ����� �����մϴ�. (Smooth: �ε巴��, Snap: ���)")]
    public RotationType rotationType = RotationType.Snap;

    [Tooltip("Smooth Turn �� ȸ�� �ӵ��Դϴ�.")]
    public float rotationSpeed = 60.0f;

    [Tooltip("Snap Turn �� �� ���� ȸ���� �����Դϴ�.")]
    public float snapTurnAngle = 45.0f;

    // Snap Turn�� ���� ���� (���� �Է��� ����)
    private bool _isReadyToSnapTurn = true;

    // ȸ�� ����� �����ϱ� ���� Enum
    public enum RotationType
    {
        Smooth,
        Snap
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        // ���� ��Ʈ�ѷ� ���̽�ƽ �Է� �ޱ� (Primary: �ַ� �޼�)
        Vector2 moveInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        // OVRCameraRig�� ���� �� ���� ������ �������� �̵� ���� ���
        // ī�޶�(�Ӹ�)�� �ƴ� Rig ��ü�� �������� �ؾ� �ֹ̰� ���մϴ�.
        Vector3 forward = transform.forward * moveInput.y;
        Vector3 right = transform.right * moveInput.x;

        // �̵� ������ ����ȭ�Ͽ� �밢�� �̵��� �� �������� ���� ����
        Vector3 moveDirection = (forward + right).normalized;

        // �̵� ���� (Time.deltaTime�� ���� ������ �ӵ��� ������� ������ �ӵ� ����)
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void HandleRotation()
    {
        // ������ ��Ʈ�ѷ� ���̽�ƽ �Է� �ޱ� (Secondary: �ַ� ������)
        Vector2 rotationInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        switch (rotationType)
        {
            case RotationType.Smooth:
                // Smooth Turn: �Է� ���� ���� �ε巴�� ȸ��
                transform.Rotate(0, rotationInput.x * rotationSpeed * Time.deltaTime, 0);
                break;

            case RotationType.Snap:
                // Snap Turn: ���̽�ƽ�� ������ �о��� ���� �� �� ȸ��
                if (Mathf.Abs(rotationInput.x) > 0.8f && _isReadyToSnapTurn)
                {
                    // ���������� �и� ���, �������� �и� ���� ������ ȸ��
                    float angle = snapTurnAngle * Mathf.Sign(rotationInput.x);
                    transform.Rotate(0, angle, 0);
                    _isReadyToSnapTurn = false; // ���� ȸ�� ����
                }
                // ���̽�ƽ�� �߾����� ���ƿ��� �ٽ� ȸ�� �غ�
                else if (Mathf.Abs(rotationInput.x) < 0.2f)
                {
                    _isReadyToSnapTurn = true;
                }
                break;
        }
    }
}