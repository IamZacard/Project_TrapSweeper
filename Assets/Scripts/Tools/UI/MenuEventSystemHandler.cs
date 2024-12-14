using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using DG.Tweening;
using UnityEngine.Events;

public class MenuEventSystemHandler : MonoBehaviour
{
    [Header("References")]
    public List<Selectable> Selectables = new List<Selectable>();
    [SerializeField] protected Selectable _firstSelected;

    [Header("Controls")]
    [SerializeField] protected InputActionReference _navigateReference;

    [Header("Animations")]
    [SerializeField] protected float _selectedAnimationScale = 1.2f;
    [SerializeField] protected float _scaleDuration = .5f;
    [SerializeField] protected List<GameObject> _animationExclutions = new List<GameObject>();

    [Header("Sounds")]
    [SerializeField] protected UnityEvent SoundEvent;

    protected Dictionary<Selectable, Vector3> _scales = new Dictionary<Selectable, Vector3>();

    protected Selectable _lastSelected;

    protected Tween _scaleUpTween;
    protected Tween _scaleDownTween;

    public virtual void Awake()
    {
        foreach(var selectable in Selectables)
        {
            AddSelectionListener(selectable);
            _scales.Add(selectable, selectable.transform.localScale);
        }
    }

    private void OnEnable()
    {
        _navigateReference.action.performed += OnNavigate;

        for (int i = 0; i < Selectables.Count; i++)
        {
            Selectables[i].transform.localScale = _scales[Selectables[i]];
        }

        StartCoroutine(SelectAfterDelay());
    }

    protected virtual IEnumerator SelectAfterDelay()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
    }

    private void OnDisable()
    {
        _navigateReference.action.performed -= OnNavigate;

        _scaleUpTween.Kill(true);
        _scaleDownTween.Kill(true);
    }

    protected virtual void AddSelectionListener(Selectable selectable)
    {
        if (selectable == null)
        {
            Debug.LogError("Selectable is null.");
            return;
        }

        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        // Define event triggers
        EventTrigger.Entry SelectEntry = new EventTrigger.Entry { eventID = EventTriggerType.Select };
        SelectEntry.callback.AddListener(OnSelect);
        trigger.triggers.Add(SelectEntry);

        EventTrigger.Entry DeselectEntry = new EventTrigger.Entry { eventID = EventTriggerType.Deselect };
        DeselectEntry.callback.AddListener(OnDeselect);
        trigger.triggers.Add(DeselectEntry);

        EventTrigger.Entry PointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        PointerEnter.callback.AddListener(OnPointerEnter);
        trigger.triggers.Add(PointerEnter);

        EventTrigger.Entry PointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        PointerExit.callback.AddListener(OnPointerExit);
        trigger.triggers.Add(PointerExit);
    }


    public void OnSelect(BaseEventData eventData)
    {
        SoundEvent?.Invoke();
        AudioManager.Instance.PlaySound(AudioManager.SoundType.ButtonClick, 1f);

        _lastSelected = eventData.selectedObject.GetComponent<Selectable>();

        if (_animationExclutions.Contains(eventData.selectedObject)) {
            return;
        }

        Vector3 newScale = eventData.selectedObject.transform.localScale * _selectedAnimationScale;
        _scaleUpTween = eventData.selectedObject.transform.DOScale(newScale, _scaleDuration);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (_animationExclutions.Contains(eventData.selectedObject))
        {
            return;
        }

        Selectable Sel = eventData.selectedObject.GetComponent<Selectable>();
        _scaleDownTween = eventData.selectedObject.transform.DOScale(_scales[Sel],_scaleDuration);
    }

    public void OnPointerEnter(BaseEventData eventData)
    {
        if (eventData == null)
        {
            Debug.LogError("OnPointerEnter called with null BaseEventData.");
            return;
        }

        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData == null)
        {
            Debug.LogError("BaseEventData is not a PointerEventData.");
            return;
        }

        if (pointerEventData.pointerEnter == null)
        {
            Debug.LogWarning("PointerEnter object is null.");
            return;
        }

        // Try to find a Selectable in the pointerEnter or its parent hierarchy
        Selectable sel = pointerEventData.pointerEnter.GetComponentInParent<Selectable>();
        if (sel != null)
        {
            Debug.Log($"Pointer entered Selectable: {sel.gameObject.name}");
            pointerEventData.selectedObject = sel.gameObject;

        }
        else
        {
            Debug.LogWarning($"Selectable component not found on object: {pointerEventData.pointerEnter.name}");
        }
    }

    public void OnPointerExit(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null) { pointerEventData.selectedObject = null; }
    }

    protected virtual void OnNavigate(InputAction.CallbackContext context)
    {
        if (EventSystem.current.currentSelectedGameObject == null && _lastSelected != null) 
        {
            EventSystem.current.SetSelectedGameObject(_lastSelected.gameObject);
        }
    }
}