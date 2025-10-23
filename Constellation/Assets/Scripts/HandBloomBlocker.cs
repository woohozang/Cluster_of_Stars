// HandBloomBlocker.cs  (Left/Right Hand �������� ��Ʈ�� ���̱�)
using UnityEngine;

public class HandBloomBlocker : MonoBehaviour
{
    void Start()
    {
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            // Bloom �� ������ ���̾� ����� ���� �ʵ��� ����ũ ����
            r.renderingLayerMask = 0;
        }
    }
}
