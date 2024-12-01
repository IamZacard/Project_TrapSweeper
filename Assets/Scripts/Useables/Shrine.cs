using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shrine : MonoBehaviour, IInteractable
{
    [Header("Shrine Settings")]
    [SerializeField] private int maxOrbCount = 3;
    [SerializeField] private int orbCount = 3; // Total orbs available
    [SerializeField] private List<GameObject> orbs; // Assigned in the inspector
    [SerializeField] private Transform orbContainer; // Optional parent object for orbs

    private bool isCellSelectionMode = false;
    private GamePlay gamePlay;
    private CharacterBase player;

    public static event Action OnEnterCellSelectionMode;
    public static event Action OnExitCellSelectionMode;


    private void Awake()
    {
        // Cache GamePlay and player references
        gamePlay = FindObjectOfType<GamePlay>();
        player = FindObjectOfType<CharacterBase>();

        // Validate the assigned orbs list
        if (orbs.Count != maxOrbCount)
        {
            Debug.LogWarning($"Orbs list count ({orbs.Count}) does not match maxOrbCount ({maxOrbCount}). Adjust the list in the inspector.");
            maxOrbCount = Mathf.Min(orbs.Count, maxOrbCount);
            orbCount = maxOrbCount;
        }
    }

    public void Interact()
    {
        if (!isCellSelectionMode)
        {
            EnterCellSelectionMode();
        }
        else
        {
            Debug.LogWarning("Already in Cell Selection Mode. Use the exit key to leave.");
        }
    }

    private void EnterCellSelectionMode()
    {
        if (orbCount <= 0) return;

        isCellSelectionMode = true;
        player.SetActive(false); // Disable player movement

        // Trigger the Enter Cell Selection Mode event
        OnEnterCellSelectionMode?.Invoke();
    }

    private void ExitCellSelectionMode()
    {
        if (!isCellSelectionMode) return;

        isCellSelectionMode = false;
        player.SetActive(true);
        Debug.Log("Exited Cell Selection Mode.");

        // Trigger the Exit Cell Selection Mode event
        OnExitCellSelectionMode?.Invoke();
    }

    private void Update()
    {
        if (isCellSelectionMode)
        {
            // Exit Cell Selection Mode when E is pressed
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("Exiting Cell Selection Mode...");
                ExitCellSelectionMode();
            }

            // Reveal cells with left mouse button
            if (Input.GetMouseButtonDown(0) && orbCount > 0)
            {
                if (gamePlay.TryGetCellAtMousePosition(out Cell cell) && IsValidCell(cell))
                {
                    Debug.Log($"Revealing cell at {cell.position}");
                    cell.revealed = true;
                    gamePlay.UpdateBoard();
                    UseOrb();
                }
            }
        }
    }

    private void UseOrb()
    {
        if (orbCount <= 0) return;

        orbCount--;

        // Deactivate one orb visually
        if (orbs.Count > 0)
        {
            var orb = orbs[maxOrbCount - orbCount - 1]; // Access orbs in sequence
            orb.SetActive(false); // Disable the orb instead of destroying it
        }

        // Exit Cell Selection Mode if all orbs are used
        if (orbCount == 0)
        {
            Debug.Log("All orbs used!");
            ExitCellSelectionMode();
        }
    }

    public void ResetOrbs()
    {
        orbCount = maxOrbCount;

        // Reactivate all orbs visually
        for (int i = 0; i < maxOrbCount; i++)
        {
            if (orbs[i] != null)
            {
                orbs[i].SetActive(true);
            }
        }

        Debug.Log("Orbs have been reset.");
    }

    private bool IsValidCell(Cell cell)
    {
        // Ensure the cell can be revealed (not flagged, revealed, etc.)
        return !cell.revealed && !cell.flagged && !gamePlay.IsGameOver;
    }
}
