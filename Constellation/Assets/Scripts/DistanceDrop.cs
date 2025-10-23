using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Collections.Generic;

[RequireComponent(typeof(PointableElement))]
public class DistanceDrop : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("손과 이 오브젝트가 이 거리(미터) 이상 멀어지면 자동으로 놓게 됩니다.")]
    public float maxGrabDistance = 0.5f;

    [Header("참조 (중요)")]
    // [수정됨] Interactor 대신 HandGrabInteractor를 직접 사용합니다.
    [Tooltip("씬에 있는 왼손 HandGrabInteractor를 여기에 끌어다 놓으세요.")]
    public HandGrabInteractor LeftHandInteractor;

    // [수정됨] Interactor 대신 HandGrabInteractor를 직접 사용합니다.
    [Tooltip("씬에 있는 오른손 HandGrabInteractor를 여기에 끌어다 놓으세요.")]
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