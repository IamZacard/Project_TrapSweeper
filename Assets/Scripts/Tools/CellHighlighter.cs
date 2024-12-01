using UnityEngine;

public class CellHighlighter : MonoBehaviour
{
    [SerializeField] private GameObject highlightPrefab; // Drag your highlight prefab here
    private GameObject currentHighlight; // Instance of the highlight
    private Cell lastHighlightedCell;    // Keep track of the last highlighted cell

    private GamePlay gamePlay; // Reference to the GamePlay script

    private void Awake()
    {
        if (highlightPrefab != null)
        {
            currentHighlight = Instantiate(highlightPrefab);
            currentHighlight.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Highlight Prefab is not set!");
        }

        // Find the GamePlay script in the scene
        gamePlay = FindObjectOfType<GamePlay>();
        if (gamePlay == null)
        {
            Debug.LogError("GamePlay script not found in the scene!");
        }
    }

    private void Update()
    {
        if (gamePlay == null) return; // Safety check

        HighlightCellUnderMouse();
    }

    private void HighlightCellUnderMouse()
    {
        if (gamePlay.TryGetCellAtMousePosition(out Cell cell))
        {
            if (cell == lastHighlightedCell) return;

            lastHighlightedCell = cell;

            // Debugging position
            Debug.Log($"Highlighting Cell at {cell.position}");

            currentHighlight.SetActive(true);
            currentHighlight.transform.position = new Vector3(cell.position.x + 0.5f, cell.position.y + 0.5f, 5f);
        }
        else
        {
            Debug.Log("Mouse is out of bounds");
            currentHighlight.SetActive(false);
            lastHighlightedCell = null;
        }
    }

}
