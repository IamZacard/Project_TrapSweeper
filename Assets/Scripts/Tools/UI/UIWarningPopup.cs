using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UIWarningPopup : MonoBehaviour
{
    public static UIWarningPopup Instance { get; private set; } // Singleton access

    [Header("UI Elements")]
    [SerializeField] private RectTransform popupTransform; // RectTransform for movement
    [SerializeField] private CanvasGroup canvasGroup; // For fade in/out
    [SerializeField] private TextMeshProUGUI warningText; // Replace with TMP if needed

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 1.5f; // Total animation duration
    [SerializeField] private float animationDurationb4Hide = 1f; // Total animation duration
    [SerializeField] private Vector3 startPos; // Off-screen start position
    [SerializeField] private Vector3 middlePos; // Target position (above UI border)
    [SerializeField] private Vector3 endPos; // Slight downward adjustment

    private Sequence _currentSequence;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null) 
            Instance = this;
        else 
        {
            Destroy(gameObject);
            return;
        }

        // Initialize positions
        if (popupTransform == null) popupTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0; // Ensure invisible at start
        popupTransform.anchoredPosition = startPos; // Place off-screen
    }

    /// <summary>
    /// Show the warning popup with animation and message.
    /// </summary>
    public void ShowPopup(string message)
    {
        if (popupTransform == null || canvasGroup == null || warningText == null)
        {
            Debug.LogError("UIWarningPopup: Missing references in Inspector!");
            return;
        }

        Debug.Log($"Animating Popup. Start: {startPos}, Middle: {middlePos}, End: {endPos}");

        // Ensure the popup is visible
        gameObject.SetActive(true);

        if (_currentSequence != null && _currentSequence.IsActive())
            _currentSequence.Kill();

        warningText.text = message;
        canvasGroup.alpha = 1;

        _currentSequence = DOTween.Sequence()
            .Append(popupTransform.DOAnchorPos(middlePos, animationDuration * 0.6f).SetEase(Ease.OutQuad))
            .Append(popupTransform.DOAnchorPos(endPos, animationDuration * 0.4f).SetEase(Ease.OutBounce))
            .Join(canvasGroup.DOFade(1, animationDuration * 0.5f))
            .OnComplete(() => StartCoroutine(AutoHide()));
    }

    /// <summary>
    /// Hide the popup with animation.
    /// </summary>
    public void HidePopup()
    {
        if (_currentSequence != null && _currentSequence.IsActive()) 
            _currentSequence.Kill();

        _currentSequence = DOTween.Sequence()
            .Append(canvasGroup.DOFade(0, animationDuration * 0.5f)) // Fade out
            .Join(popupTransform.DOAnchorPos(startPos, animationDuration * 0.5f).SetEase(Ease.InQuad)) // Move off-screen
            .OnComplete(() => canvasGroup.alpha = 0); // Reset alpha
    }

    /// <summary>
    /// Automatically hide popup after delay.
    /// </summary>
    private System.Collections.IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(animationDurationb4Hide); // Duration before hiding
        HidePopup();
    }
}
