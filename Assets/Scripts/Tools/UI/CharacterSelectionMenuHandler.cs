using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class CharacterSelectionMenuHandler : MonoBehaviour
{
    [Header("References")]
    public List<Selectable> Selectables = new List<Selectable>();
    [SerializeField] protected Selectable _firstSelected;

    [Header("Controls")]
    [SerializeField] protected InputActionReference _navigateReference;

    [Header("Animations")]
    [SerializeField] protected float _selectedAnimationScale = 1.2f;
    [SerializeField] protected float _scaleDuration = .5f;
    [SerializeField] protected List<GameObject> _animationExclusions = new List<GameObject>();

    [Header("Sounds")]
    [SerializeField] protected UnityEvent SoundEvent;

    protected Dictionary<Selectable, Vector3> _scales = new Dictionary<Selectable, Vector3>();
    protected Selectable _lastSelected;
    protected Tween _scaleUpTween;
    protected Tween _scaleDownTween;

    [Header("Character Selection")]
    public Button galeButton;
    public Button violetButton;
    public Button mysticButton;
    public Button sageButton;
    public Button shufflegrinButton;
    public Button startGameButton;

    private string selectedCharacter;

    public virtual void Awake()
    {
        foreach (var selectable in Selectables)
        {
            AddSelectionListener(selectable);
            _scales.Add(selectable, selectable.transform.localScale);
        }
    }

    private void Start()
    {
        // Disable the start button initially
        startGameButton.interactable = false;
        _animationExclusions.Add(startGameButton.gameObject);

        // Assign methods to each character button
        galeButton.onClick.AddListener(() => SelectCharacter("Gale"));
        violetButton.onClick.AddListener(() => SelectCharacter("Violet"));
        mysticButton.onClick.AddListener(() => SelectCharacter("Mystic"));
        sageButton.onClick.AddListener(() => SelectCharacter("Sage"));
        shufflegrinButton.onClick.AddListener(() => SelectCharacter("Shufflegrin"));
        startGameButton.onClick.AddListener(StartGame);

        // Initialize start button animation state
        UpdateStartButtonAnimation();
    }

    private void OnEnable()
    {
        _navigateReference.action.performed += OnNavigate;

        foreach (var selectable in Selectables)
        {
            selectable.transform.localScale = _scales[selectable];
        }

        StartCoroutine(SelectAfterDelay());
    }

    private void OnDisable()
    {
        _navigateReference.action.performed -= OnNavigate;

        if (_scaleUpTween?.target != null)
            _scaleUpTween.Kill();

        if (_scaleDownTween?.target != null)
            _scaleDownTween.Kill();
    }

    protected virtual IEnumerator SelectAfterDelay()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
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

        if (_animationExclusions.Contains(eventData.selectedObject))
        {
            return;
        }

        if (eventData.selectedObject != null && !_animationExclusions.Contains(eventData.selectedObject))
        {
            Vector3 newScale = eventData.selectedObject.transform.localScale * _selectedAnimationScale;
            _scaleUpTween = eventData.selectedObject.transform.DOScale(newScale, _scaleDuration);
        }
        else
        {
            Debug.LogWarning("OnSelect: Null or excluded object.");
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (_animationExclusions.Contains(eventData.selectedObject))
        {
            return;
        }

        Selectable Sel = eventData.selectedObject?.GetComponent<Selectable>();
        if (Sel != null && _scales.ContainsKey(Sel))
        {
            _scaleDownTween = Sel.transform.DOScale(_scales[Sel], _scaleDuration);
        }
        else
        {
            Debug.LogWarning("OnDeselect: Null or missing scale data.");
        }
    }

    public void OnPointerEnter(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            var enterObject = pointerEventData.pointerEnter;
            if (enterObject != null)
            {
                Selectable sel = enterObject.GetComponent<Selectable>();
                if (sel == null)
                {
                    sel = enterObject.GetComponentInParent<Selectable>();
                }

                if (sel != null)
                {
                    Debug.Log($"Pointer entered Selectable: {sel.name}");
                    EventSystem.current.SetSelectedGameObject(sel.gameObject);
                }
                else
                {
                    Debug.LogWarning($"No selectable found on {enterObject.name}");
                }
            }
        }
    }

    public void OnPointerExit(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            pointerEventData.selectedObject = null;
        }
    }

    private void SelectCharacter(string characterName)
    {
        selectedCharacter = characterName;
        AudioManager.Instance.PlaySound(AudioManager.SoundType.CharacterPick, 1f);
        Debug.Log($"Selected Character: {selectedCharacter}");

        // Enable and allow animations for the start button
        startGameButton.interactable = true;

        // Update animation state for the start button
        UpdateStartButtonAnimation();
    }
    private void UpdateStartButtonAnimation()
    {
        if (startGameButton.interactable)
        {
            _animationExclusions.Remove(startGameButton.gameObject);
        }
        else
        {
            _animationExclusions.Add(startGameButton.gameObject);
        }
    }

    private void StartGame()
    {
        if (string.IsNullOrEmpty(selectedCharacter)) return;

        AudioManager.Instance.PlaySound(AudioManager.SoundType.ButtonClick, 1f);

        // Store selected character and load the next scene
        CharacterSelectionManager.Instance.SetSelectedCharacter(selectedCharacter);
        SceneManager.LoadScene("Level1");
    }

    protected virtual void OnNavigate(InputAction.CallbackContext context)
    {
        if (EventSystem.current.currentSelectedGameObject == null && _lastSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(_lastSelected.gameObject);
        }
    }
}
