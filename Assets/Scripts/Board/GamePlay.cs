using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class GamePlay : MonoBehaviour
{
    [Header("Board Setup")]
    public int width;
    public int height;
    public int trapCount;
    public float difficulty;

    [Header("Player's Settings")]
    private CharacterBase player;
    public Vector2 startPos;
    public float flagCount;
    public bool IsGameOver => gameOver;
    public bool IsLevelComplete => levelComplete;

    private Board board;
    private CellGrid grid;

    private bool levelComplete;
    private bool gameOver;
    private bool generated;

    private void OnValidate()
    {
        SetupPlayer(); // Runs in the editor, ensures stats are updated
        CalculateGameSettings();
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        board = GetComponentInChildren<Board>();

        SetupPlayer();

        CalculateGameSettings();
    }

    private void Start()
    {
        SetupPlayer(); // Ensure player and difficulty are properly set before starting

        CalculateGameSettings();
        NewGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            NewGame();
            return;
        }

        /*if (Input.GetMouseButtonDown(1) && !gameOver && !levelComplete)
        {
            Flag();
        }*/
    }

    private void NewGame()
    {
        StopAllCoroutines();

        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);

        gameOver = false;
        levelComplete = false;
        generated = false;

        grid = new CellGrid(width, height);
        board.Draw(grid);

        // Reset player
        player.transform.position = startPos;
        player.SetActive(true);

        CalculateGameSettings();
    }

    private void Reveal()
    {
        if (TryGetCellAtMousePosition(out Cell cell))
        {
            if (!generated)
            {
                grid.GenerateTraps(cell, trapCount);
                grid.GenerateNumbers();
                generated = true;
            }

            Reveal(cell);
        }
    }

    private void Reveal(Cell cell)
    {
        if (cell.revealed || cell.flagged || gameOver || levelComplete) return;

        switch (cell.type)
        {
            case Cell.Type.Trap:
                Explode(cell);
                break;

            case Cell.Type.Empty:
                StartCoroutine(Flood(cell));
                CheckWinCondition();
                break;

            default:
                cell.revealed = true;
                CheckWinCondition();
                break;
        }

        CellRevealGameFeel(cell);

        board.Draw(grid);

        CheckWinCondition();
    }

    private IEnumerator Flood(Cell startCell)
    {
        Queue<Cell> queue = new Queue<Cell>(); // Specify the type of the queue
        queue.Enqueue(startCell);

        while (queue.Count > 0)
        {
            Cell cell = queue.Dequeue();

            if (gameOver || cell.revealed || cell.type == Cell.Type.Trap) continue;

            cell.revealed = true;
            board.Draw(grid);

            if (cell.type == Cell.Type.Empty)
            {
                CellRevealGameFeel(cell);
                // Check all neighbors and enqueue them
                grid.TryGetCell(cell.position.x - 1, cell.position.y, out Cell left);
                grid.TryGetCell(cell.position.x + 1, cell.position.y, out Cell right);
                grid.TryGetCell(cell.position.x, cell.position.y - 1, out Cell down);
                grid.TryGetCell(cell.position.x, cell.position.y + 1, out Cell up);

                foreach (Cell neighbor in new[] { left, right, down, up })
                {
                    if (neighbor != null && !neighbor.revealed)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            yield return null; // Wait until the next frame to continue
        }
    }

    private void Flag()
    {
        if (!TryGetCellAtMousePosition(out Cell cell) || cell.revealed) return;

        if (cell.flagged)
        {
            cell.flagged = false;
            flagCount += 1;
        }
        else if (flagCount > 0)
        {
            cell.flagged = true;
            flagCount -= 1;
        }

        board.Draw(grid);

        CheckWinCondition();
    }

    private void Explode(Cell cell)
    {
        gameOver = true;

        // Set the mine as exploded
        cell.exploded = true;
        cell.revealed = true;

        // Deactivate the player
        player.SetActive(false);

        // Reveal all other mines
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cell = grid[x, y];

                if (cell.type == Cell.Type.Trap) {
                    cell.revealed = true;
                }
            }
        }

        Debug.Log("Game Over! You stepped on a trap.");
    }

    private void CheckWinCondition()
    {
        bool allNonTrapCellsRevealed = true;
        bool allTrapsFlagged = true;

        // Iterate over the grid to check the win conditions
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];

                // Check if any non-trap cell is not revealed
                if (cell.type != Cell.Type.Trap && !cell.revealed)
                {
                    allNonTrapCellsRevealed = false;
                }

                // Check if any trap cell is not flagged
                if (cell.type == Cell.Type.Trap && !cell.flagged)
                {
                    allTrapsFlagged = false;
                }

                // Exit early if either condition fails
                if (!allNonTrapCellsRevealed && !allTrapsFlagged)
                {
                    return; // The player has not won yet
                }
            }
        }

        // If all conditions are met, the level is complete
        levelComplete = true;

        // Automatically flag all traps as part of the win state
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];
                if (cell.type == Cell.Type.Trap)
                {
                    cell.flagged = true;
                }
            }
        }

        Debug.Log("Level Complete! You revealed all non-trap cells or flagged all traps.");
    }


    public bool TryGetCellAtMousePosition(out Cell cell)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        return grid.TryGetCell(cellPosition.x, cellPosition.y, out cell);
    }

    public void UpdateBoard()
    {
        board.Draw(grid);
    }

    public void PlayerMoved(Vector3 playerPosition)
    {
        if (gameOver) return;

        Vector3Int cellPosition = board.tilemap.WorldToCell(playerPosition);
        if (grid.TryGetCell(cellPosition.x, cellPosition.y, out Cell cell))
        {
            if (!generated) // Generate traps only once
            {
                grid.GenerateTraps(cell, trapCount);
                grid.GenerateNumbers();
                generated = true;
            }

            Reveal(cell); // Reveal the cell after generating traps
        }
    }
    private void SetupPlayer()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null) Debug.LogError("Player not found!");
            else player = playerObject.GetComponent<CharacterBase>();
        }

        if (player != null && player.stats != null)
        {
            difficulty = player.stats._difficultyIncrease;
        }
        else
        {
            Debug.LogWarning("Player stats are not properly set.");
        }
    }


    private void CalculateGameSettings()
    {
        trapCount = Mathf.Clamp(Mathf.RoundToInt((width * height) * difficulty), 0, width * height);
        flagCount = trapCount; // Match flag count with trap count
    }

    private void CellRevealGameFeel(Cell cell)
    {
        // Convert grid position to world position
        Vector3 cellWorldPosition = new Vector3(cell.position.x, cell.position.y, cell.position.z);

        // Configure and trigger game feel
        GameFeel revealGameFeel = new GameFeel(
            AudioManager.SoundType.BoardRevealSound,  // Sound type
            1f,                                       // Sound volume
            0.1f,                                     // Shake duration
            0.5f,                                     // Shake magnitude
            EffectManager.EffectType.CellRevealEffect, // Effect type
            cellWorldPosition + new Vector3(0.5f, 0.5f, 0) // Adjusted for offset
        );

        revealGameFeel.Trigger();
    }
}
