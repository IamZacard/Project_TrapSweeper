using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.RuleTile.TilingRuleOutput;

[DefaultExecutionOrder(-1)]
public class GamePlay : MonoBehaviour
{
    [Header("Board Setup")]
    public int width;
    public int height;
    public int trapCount;
    public float difficulty;
    private Cell highlightedCell;

    [Header("Player's Settings")]
    private CharacterBase player;
    public Vector2 startPos;
    public float flagCount;

    [Header("Portal Block")]
    [SerializeField] private GameObject magicBlock;
    public bool IsGameOver => gameOver;
    public bool IsLevelComplete => levelComplete;
    public bool IsLevelGenerated => generated;

    private Board board;
    private CellGrid grid;
    private ShardManager shardmanager;

    [SerializeField] private Shrine shrine;   

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
        if (board == null)
        {
            Debug.LogError("Board component is missing!");
        }

        shardmanager = FindObjectOfType<ShardManager>();
        if (shardmanager == null)
        {
            Debug.LogWarning("ShardManager instance not found.");
        }

        SetupPlayer();
        CalculateGameSettings();
    }

    private void Start()
    {
        NewGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !(gameOver && levelComplete))
        {
            NewGame();
            return;
        }
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
        if (!magicBlock.activeSelf)
        {
            magicBlock.SetActive(true);
        }
        AudioManager.Instance.PlaySound(AudioManager.SoundType.LevelStartSound, 1f);

        //Shrine thing

        // Reset orbs in shrine
        if (shrine != null)
        {
            shrine.ResetOrbs();
        }
        else
        {
            Debug.LogWarning("Shrine reference is missing in GamePlay script!");
        }       

        //shard thing
        if (shardmanager != null)
        {
            shardmanager.ClearSpawnedShards();
        }
        else
        {
            Debug.LogWarning("shardmanager reference is missing in GamePlay script!");
        }
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

    #region different supporting methods for revealing cell
    public void RevealCellAtMouse()
    {
        if (TryGetCellAtMousePosition(out Cell cell))
        {
            if (!generated)
            {
                grid.GenerateTraps(cell, trapCount);
                grid.GenerateNumbers();
                generated = true;
            }

            Reveal(cell, ignoreTraps: true); // Prevent traps from exploding
            CheckWinCondition(); // Ensure win condition check is done after reveal
        }
    }

    public void RevealCellTrap(Cell cell)
    {
        Reveal(cell, ignoreTraps: true); // Prevent traps from exploding
        CheckWinCondition(); // Ensure win condition check is done after reveal
    }
    #endregion

    private void Reveal(Cell cell, bool ignoreTraps = false)
    {
        if ((cell.revealed && cell.type != Cell.Type.Trap) || cell.flagged || gameOver || levelComplete) return;

        switch (cell.type)
        {
            case Cell.Type.Trap:
                if (!ignoreTraps)
                {
                    Explode(cell);
                }
                else
                {
                    cell.revealed = true; // Reveal the trap without triggering explosion
                }
                break;

            case Cell.Type.Empty:
                StartCoroutine(Flood(cell));
                break;

            default:
                cell.revealed = true;
                break;
        }

        // If Gale is the active character, attempt to spawn a shard
        if (player is Gale) // Assuming player inherits from CharacterBase
        {            
            shardmanager?.TrySpawnShard(cell, width, height);
        }

        // Check the win condition immediately after revealing
        CheckWinCondition();
        CellRevealGameFeel(cell);

        board.Draw(grid);
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

            CheckWinCondition();

            yield return null; // Wait until the next frame to continue
        }
    }

    private void Explode(Cell cell)
    {
        if (player != null)
        {
            player.OnTrapStepped(cell, () =>
            {
                gameOver = true;

                // Default explosion logic
                cell.exploded = true;
                cell.revealed = true;
                player.SetActive(false);

                // Reveal all other mines
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        var gridCell = grid[x, y];
                        if (gridCell.type == Cell.Type.Trap)
                        {
                            gridCell.revealed = true;
                        }
                    }
                }

                Debug.Log("Game Over! You stepped on a trap.");
            });
        }
    }


    private void CheckWinCondition()
    {
        bool allNonTrapCellsRevealed = true;
        bool allTrapsCorrectlyFlagged = true;

        // Check if all non-trap cells are revealed and all traps are flagged or revealed
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];

                // If it's a non-trap cell and not revealed, or flagged (incorrectly), it's a fail
                if (cell.type != Cell.Type.Trap)
                {
                    if (!cell.revealed || cell.flagged)  // Non-trap cells must be revealed and not flagged
                    {
                        allNonTrapCellsRevealed = false;
                    }
                }
                // If it's a trap cell, it must be either flagged or revealed (and not exploded)
                else
                {
                    if (!(cell.flagged || (cell.revealed && !cell.exploded)))
                    {
                        allTrapsCorrectlyFlagged = false;
                    }
                }
            }
        }

        // If one of the conditions is true, mark level as complete
        if (allNonTrapCellsRevealed || allTrapsCorrectlyFlagged)
        {
            if (!levelComplete) // Check to avoid duplicate completion triggers
            {
                levelComplete = true;

                // Flag all traps when the level is complete
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

                AudioManager.Instance.PlaySound(AudioManager.SoundType.LevelComplete, 1f);

                // Reveal all non-flagged, unrevealed cells
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Cell cell = grid[x, y];
                        if (!cell.revealed && !cell.flagged) // Skip flagged cells
                        {
                            cell.revealed = true; // Reveal the cell
                        }
                    }
                }

                board.Draw(grid);
                magicBlock.SetActive(false);
                StartCoroutine(GameWin());

                Debug.Log("Level Complete! You revealed all non-trap cells and flagged all traps.");
            }
        }
    }    

    public void UpdateBoard()
    {
        board.Draw(grid);
        CheckWinCondition();
    }

    public void PlayerMoved(Vector3 playerPosition)
    {
        if (gameOver) return;

        Vector3Int cellPosition = board.tilemap.WorldToCell(playerPosition);
        if (grid.TryGetCell(cellPosition.x, cellPosition.y, out Cell cell))
        {
            if (!generated)
            {
                grid.GenerateTraps(cell, trapCount);
                grid.GenerateNumbers();
                generated = true;
            }

            Reveal(cell);
            UpdateBoard();
        }
    }

    private void SetupPlayer()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null) Debug.LogWarning("Player not found!");
            else player = playerObject.GetComponent<CharacterBase>();
        }

        if (player != null && player.stats != null)
        {
            difficulty = player.stats._difficultyIncrease;
        }
        else
        {
            SpawnCharacter();
        }
    }

    private void SpawnCharacter()
    {
        var prefab = CharacterSelectionManager.Instance.GetSelectedCharacterPrefab();
        if (prefab != null && startPos != null)
        {
            Instantiate(prefab, startPos, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Failed to spawn character. Prefab or spawn point is missing.");
        }
    }

    private void CalculateGameSettings()
    {
        trapCount = Mathf.Clamp(Mathf.RoundToInt((width * height) * difficulty), 0, width * height);
        flagCount = trapCount; // Match flag count with trap count
    }

    #region Supporting methods

    public bool TryGetCellAtMousePosition(out Cell cell)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        return grid.TryGetCell(cellPosition.x, cellPosition.y, out cell);
    }

    public Vector2Int GetCellPosition(Vector3 worldPosition)
    {
        // Convert world position to grid position
        // Ensure this logic matches your grid system
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x),
            Mathf.FloorToInt(worldPosition.y)
        );
    }

    public bool TryGetCellAtPosition(Vector2Int position, out Cell cell)
    {
        // Check if position is within bounds
        if (position.x >= 0 && position.x < width &&
            position.y >= 0 && position.y < height)
        {
            cell = grid[position.x, position.y]; // Assuming grid is a 2D array of Cells
            return true;
        }

        cell = null;
        return false;
    }

    #endregion

    #region GameFeel
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

    private void BlockDestroyGameFeel()
    {
        // Convert grid position to world position
        Vector3 blockWorldPosition = new Vector3(magicBlock.transform.position.x, magicBlock.transform.position.y, magicBlock.transform.position.z);

        // Configure and trigger game feel
        GameFeel blockGameFeel = new GameFeel(
            AudioManager.SoundType.MagicBlockReleaseSound,  // Sound type
            1f,                                       // Sound volume
            0.5f,                                     // Shake duration
            2f,                                     // Shake magnitude
            EffectManager.EffectType.PortalDistractionEffect, // Effect type
            blockWorldPosition // Adjusted for offset
        );

        blockGameFeel.Trigger();
    }

    private IEnumerator GameWin()
    {
        BlockDestroyGameFeel(); // Trigger game feel

        yield return new WaitForSeconds(1f); // Wait for 1 seconds

        AudioManager.Instance.PlaySound(AudioManager.SoundType.LevelComplete, 1f);
    }
    #endregion
}