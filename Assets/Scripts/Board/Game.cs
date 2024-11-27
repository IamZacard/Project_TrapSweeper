using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class Game : MonoBehaviour
{
    [Header("Board Setup")]
    public int width;
    public int height;
    public int trapCount;
    public float difficulty;

    [Header("Player's Settings")]
    private CharacterBase player; // Reference to the PlayerBase containing Stats
    public Vector2 startPos;
    public float flagCount;

    private Board board;
    private CellGrid grid;
    private bool gameover;
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

        SetupPlayer(); // Initialize the player and difficulty

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

        if (!gameover)
        {           
            if (Input.GetMouseButtonDown(1))
            {
                Flag();
            }
        }
    }

    private void NewGame()
    {
        StopAllCoroutines();

        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);

        gameover = false;
        generated = false;

        grid = new CellGrid(width, height);
        board.Draw(grid);

        player.transform.position = startPos;
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
        if (cell.revealed) return;
        if (cell.flagged) return;

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

        board.Draw(grid);
    }

    private IEnumerator Flood(Cell cell)
    {
        if (gameover) yield break;
        if (cell.revealed) yield break;
        if (cell.type == Cell.Type.Trap) yield break;

        cell.revealed = true;
        board.Draw(grid);

        yield return null;

        if (cell.type == Cell.Type.Empty)
        {
            if (grid.TryGetCell(cell.position.x - 1, cell.position.y, out Cell left)) {
                StartCoroutine(Flood(left));
            }
            if (grid.TryGetCell(cell.position.x + 1, cell.position.y, out Cell right)) {
                StartCoroutine(Flood(right));
            }
            if (grid.TryGetCell(cell.position.x, cell.position.y - 1, out Cell down)) {
                StartCoroutine(Flood(down));
            }
            if (grid.TryGetCell(cell.position.x, cell.position.y + 1, out Cell up)) {
                StartCoroutine(Flood(up));
            }
        }
    }

    private void Flag()
    {
        if (!TryGetCellAtMousePosition(out Cell cell)) return;
        if (cell.revealed) return;

        cell.flagged = !cell.flagged;
        board.Draw(grid);
    }

    private void Explode(Cell cell)
    {
        gameover = true;

        // Set the mine as exploded
        cell.exploded = true;
        cell.revealed = true;

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
    }

    private void CheckWinCondition()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];

                // All non-mine cells must be revealed to have won
                if (cell.type != Cell.Type.Trap && !cell.revealed) {
                    return; // no win
                }
            }
        }

        gameover = true;

        // Flag all the mines
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];

                if (cell.type == Cell.Type.Trap) {
                    cell.flagged = true;
                }
            }
        }
    }

    private bool TryGetCellAtMousePosition(out Cell cell)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        return grid.TryGetCell(cellPosition.x, cellPosition.y, out cell);
    }

    public void PlayerMoved(Vector3 playerPosition)
    {
        if (gameover) return;

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
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogWarning("Player GameObject not found!");
            return;
        }

        player = playerObject.GetComponent<CharacterBase>();
        if (player == null)
        {
            Debug.LogWarning("Player does not have a CharacterBase component!");
            return;
        }

        if (player.stats == null)
        {
            Debug.LogWarning("Player stats are null!");
            return;
        }

        difficulty = player.stats._difficultyIncrease;
    }

    private void CalculateGameSettings()
    {
        trapCount = Mathf.Clamp(Mathf.RoundToInt((width * height) * difficulty), 0, width * height);
        flagCount = trapCount; // Match flag count with trap count
    }
}
