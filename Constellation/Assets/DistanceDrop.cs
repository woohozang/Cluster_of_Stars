using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Collections.Generic;

[RequireComponent(typeof(PointableElement))]
public class DistanceDrop : MonoBehaviour
{
    [Header("����")]
    [Tooltip("�հ� �� ������Ʈ�� �� �Ÿ�(����) �̻� �־����� �ڵ����� ���� �˴ϴ�.")]
    public float maxGrabDistance = 0.5f;

    [Header("���� (�߿�)")]
    // [������] Interactor ��� HandGrabInteractor�� ���� ����մϴ�.
    [Tooltip("���� �ִ� �޼� HandGrabInteractor�� ���⿡ ����� ��������.")]
    public HandGrabInteractor LeftHandInteractor;

    // [������] Interactor ��� HandGrabInteractor�� ���� ����մϴ�.
    [Tooltip("���� �ִ� ������ HandGrabInteractor�� ���⿡ ����� ��������.")]
    public HandGrabInteractor RightHandInteractor;

    private PointableElement _pointableElement;
    private readonly Dictionary<int, IInteractor> _grabbingInteractors = new Dictionary<int, IInteractor>();

    void Awake()
    {
        _pointableElement = GetComponent<PointableElement>();
    }

    void OnEnable()
    {
        _pointableElement.WhenPointerEventRaised += HandlePointerEventRaised;
    }

    void OnDisable()
    {
        _pointableElement.WhenPointerEventRaised -= HandlePointerEventRaised;
    }

    private void HandlePointerEventRaised(PointerEvent evt)
    {
        switch (evt.Type)
        {
            case PointerEventType.Select:
                if (LeftHandInteractor != null && evt.Identifier == LeftHandInteractor.Identifier)
                {
                    _grabbingInteractors[evt.Identifier] = LeftHandInteractor;
                }
                else if (RightHandInteractor != null && evt.Identifier == RightHandInteractor.Identifier)
                {
                    _grabbingInteractors[evt.Identifier] = RightHandInteractor;
                }
                break;

            case PointerEventType.Unselect:
            case PointerEventType.Cancel:
                if (_grabbingInteractors.ContainsKey(evt.Identifier))
                {
                    _grabbingInteractors.Remove(evt.Identifier);
                }
                break;
        }
    }

    void Update()
    {
        if (_grabbingInteractors.Count == 0)
        {
            return;
        }

        var keys = new List<int>(_grabbingInteractors.Keys);
        foreach (var key in keys)
        {
            IInteractor interactor = _grabbingInteractors[key];
            var interactorMono = interactor as MonoBehaviour;

            if (interactorMono == null) continue;

            float distance = Vector3.Distance(interactorMono.transform.position, this.transform.position);

            if (distance > maxGrabDistance)
            {
                interactor.Unselect();
            }
        }
    }
}