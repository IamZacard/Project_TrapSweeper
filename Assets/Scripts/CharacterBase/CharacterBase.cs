using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public abstract class CharacterBase : MonoBehaviour, ICharacterBase
{
    [Header("CharacterBase")]
    public Stats stats;
    public PlayerMovement controls;
    public bool isActive { get; private set; } = true;

    private IInteractable _interactable;
    private Vector3 originalScale;
    public float _characterModelScaleNumber = 1.2f;

    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private Tilemap roomTileMap;

    private Light2D characterLight;

    [Header("Flickering")]
    private SpriteRenderer sr;
    private GamePlay game;
    private Coroutine flickerCoroutine;
    private bool isOnNumberCell = false; // Track if the player is standing on a number cell
    [SerializeField] private float timeToTriggerFlicker = 1f; // Time in seconds before flickering starts

    private bool _isInteractionKeyPressed => Input.GetKeyDown(KeyCode.E);

    private void OnEnable() => controls.Enable();

    private void OnDisable() => controls.Disable();

    protected virtual void Awake()
    {
        controls = new PlayerMovement();
        isActive = true;

        characterLight = GetComponentInChildren<Light2D>();
        if (characterLight == null)
        {
            Debug.LogError("Light2D component not found. Ensure it's attached as a child object to the character.");
        }
        else if (stats != null)
        {
            characterLight.pointLightOuterRadius = stats._lightRadius;
            Debug.Log($"Initial Light Radius set to {stats._lightRadius}");
        }

        sr = GetComponent<SpriteRenderer>();

        game = FindObjectOfType<GamePlay>();
        if (game == null)
            Debug.LogError("GamePlay script not found in the scene!");
    }

    protected virtual void Start()
    {
        controls.Main.Movement.performed += ctx => Move(ctx.ReadValue<Vector2>());

        originalScale = transform.localScale;

        
    }

    protected virtual void Update()
    {
        Flickering();
    }

    #region Movement
    public virtual void Move(Vector2 direction)
    {
        if (!isActive)
        {
            // Play an error sound
            AudioManager.Instance.PlaySound(AudioManager.SoundType.ErrorSound, 1f);
            return; // Prevent movement if the player is not active
        }

        // Ensure only one direction is processed at a time
        direction = new Vector2(
            direction.x != 0 ? Mathf.Sign(direction.x) : 0,
            direction.y != 0 ? Mathf.Sign(direction.y) : 0
        );

        if (CanMove(direction))
        {
            // Perform the jump
            Vector3 targetPosition = transform.position + (Vector3)direction;
            StartCoroutine(JumpToPosition(targetPosition)); // Ensure this is the only call to JumpToPosition
        }
    }

    private bool CanMove(Vector2 direction)
    {
        if (groundTileMap == null) return false;

        Vector3Int groundGridPosition = groundTileMap.WorldToCell(transform.position + (Vector3)direction);
        bool canMoveOnGround = groundTileMap.HasTile(groundGridPosition);

        if (roomTileMap != null && roomTileMap.gameObject.activeSelf)
        {
            Vector3Int roomGridPosition = roomTileMap.WorldToCell(transform.position + (Vector3)direction);
            bool canMoveInRoom = roomTileMap.HasTile(roomGridPosition);

            if (canMoveInRoom)
            {
                return true;
            }
        }

        return canMoveOnGround;
    }

    private IEnumerator JumpToPosition(Vector3 targetPosition)
    {
        float jumpHeight = 0.4f; // Height of the jump
        float jumpDuration = 0.05f; // Duration of the jump
        Vector3 startPosition = transform.position;
        Vector3 peakPosition = startPosition + new Vector3(0, jumpHeight, 0);

        // Move to peak position
        float elapsed = 0f;
        while (elapsed < jumpDuration / 2)
        {
            transform.position = Vector3.Lerp(startPosition, peakPosition, elapsed / (jumpDuration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Move to target position
        elapsed = 0f;
        while (elapsed < jumpDuration / 2)
        {
            transform.position = Vector3.Lerp(peakPosition, targetPosition, elapsed / (jumpDuration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to final position
        transform.position = SnapPosition(targetPosition);

        // Notify GamePlay script
        GamePlay game = FindObjectOfType<GamePlay>();
        if (game != null)
        {
            game.PlayerMoved(transform.position);
        }        

        // Scale character after jump
        StartCoroutine(ScaleCharacter());
    }

    private Vector3 SnapPosition(Vector3 position)
    {
        StepGameFeel();

        // Snapping to the nearest half unit grid (0.5 units)
        position.x = Mathf.Floor(position.x) + 0.5f;
        position.y = Mathf.Floor(position.y) + 0.5f;
        return position;
    }

    private IEnumerator ScaleCharacter()
    {
        transform.localScale = originalScale * _characterModelScaleNumber;
        yield return new WaitForSeconds(0.25f);
        transform.localScale = originalScale;
    }
    #endregion


    #region Flickering   

    private void Flickering()
    {
        if (IsOnNumberCell())
        {
            if (!isOnNumberCell)
            {
                isOnNumberCell = true;
                Invoke(nameof(StartFlicker), timeToTriggerFlicker); // Start flickering after N seconds
            }
        }
        else
        {
            if (isOnNumberCell)
            {
                isOnNumberCell = false;
                CancelInvoke(nameof(StartFlicker)); // Cancel flickering if the player moves off the cell
                StopFlicker();
            }
        }
    }

    // Method to start flickering when the player stands on a numbered cell
    private void StartFlicker()
    {
        if (flickerCoroutine == null)
        {
            flickerCoroutine = StartCoroutine(FlickerSprite());
        }
    }

    // Method to stop flickering
    private void StopFlicker()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;

            // Reset sprite transparency to full alpha
            if (sr != null)
            {
                Color color = sr.color;
                color.a = 1f; // Fully visible
                sr.color = color;
            }
        }
    }

    // Coroutine to handle sprite flickering
    private IEnumerator FlickerSprite()
    {
        if (sr == null) yield break;

        while (true)
        {
            // Flicker to low alpha
            Color color = sr.color;
            color.a = 0.15f; // Low alpha
            sr.color = color;
            yield return new WaitForSeconds(1f); // Low alpha duration

            // Flicker back to full alpha
            color.a = 1f; // Full alpha
            sr.color = color;
            yield return new WaitForSeconds(1.5f); // Full alpha duration
        }
    }

    private bool IsOnNumberCell()
    {
        // Implement logic to detect numbered cells
        // Example: Check tile type or specific tag
        Vector3Int gridPosition = groundTileMap.WorldToCell(transform.position);
        TileBase currentTile = groundTileMap.GetTile(gridPosition);

        if (currentTile != null && currentTile.name.Contains("Number")) // Adjust as needed
        {
            return true;
        }

        return false;
    }

    #endregion

    #region Interacting
    public void Interact()
    {
        if (_isInteractionKeyPressed && isActive && _interactable != null)
        {
            _interactable.Interact();
        }
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D other) => _interactable = other.GetComponent<IInteractable>();

    private void OnTriggerExit2D(Collider2D other) => _interactable = null;

    public void SetActive(bool state)
    {
        isActive = state;

        // Optional: Perform additional actions like disabling input, animations, etc.
        if (!state)
        {
            Debug.Log("Character deactivated (stepped on a trap).");
        }
        else
        {
            Debug.Log("Character activated for a new game.");
        }
    }
    private void StepGameFeel()
    {
        // Configure and trigger game feel
        GameFeel stepGameFeel = new GameFeel(
            AudioManager.SoundType.FootStepSound, // Sound type
            1f,                                   // Sound volume
            0.1f,                                 // Shake duration
            0.5f,                                 // Shake magnitude
            EffectManager.EffectType.StepEffect,  // Effect type
            transform.position                    // Source transform
        );

        stepGameFeel.Trigger();
    }
}