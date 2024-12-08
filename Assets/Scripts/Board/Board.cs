using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }

    //public Tile[] tileUnknownVariants;
    public AnimatedTile tileUnknown;
    public Tile tileFloor;
    public Tile tileEmpty;
    public Tile tileTrap;
    public Tile tileExploded;
    public AnimatedTile tileFlag_anim;
    public Tile tileNum1;
    public Tile tileNum2;
    public Tile tileNum3;
    public Tile tileNum4;
    public Tile tileNum5;
    public Tile tileNum6;
    public Tile tileNum7;
    public Tile tileNum8;

    private HashSet<Vector3Int> pillarPositions = new HashSet<Vector3Int>();
    private HashSet<Vector3Int> shrinePositions = new HashSet<Vector3Int>();

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void Draw(CellGrid grid)
    {
        int width = grid.Width;
        int height = grid.Height;

        // Get all game objects with the "Pillar" tag
        GameObject[] pillars = GameObject.FindGameObjectsWithTag("Pillar");
        pillarPositions.Clear();

        // Store the positions of all pillars
        foreach (GameObject pillar in pillars)
        {
            Vector3Int position = tilemap.WorldToCell(pillar.transform.position);
            pillarPositions.Add(position);
        }

        // Get all game objects with the "Shrine" tag
        GameObject[] shrines = GameObject.FindGameObjectsWithTag("Shrine");
        shrinePositions.Clear();

        // Store the positions of all shrines
        foreach (GameObject shrine in shrines)
        {
            Vector3Int position = tilemap.WorldToCell(shrine.transform.position);
            shrinePositions.Add(position);
        }

        // Iterate through all cells in the grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                Cell cell = grid[x, y];

                // Check if the cell is a pillar position / Check if the cell is a shrine position
                if (pillarPositions.Contains(cellPosition))
                {
                    cell.type = Cell.Type.Pillar;
                    continue;
                }
                else if (shrinePositions.Contains(cellPosition))
                {
                    cell.type = Cell.Type.Shrine;
                    continue;
                }

                // Set the tile
                tilemap.SetTile(cell.position, GetTile(cell));
            }
        }
    }

    private TileBase GetTile(Cell cell)
    {
        if (cell.revealed)
        {
            return GetRevealedTile(cell);
        }
        else if (cell.flagged)
        {
            return tileFlag_anim;
        }
        else
        {
            return GetUnknownTile();
        }
    }

    private TileBase GetRevealedTile(Cell cell)
    {
        switch (cell.type)
        {
            case Cell.Type.Empty:
                return tileEmpty;

            case Cell.Type.Trap:
                // Show flag tile if trap is flagged
                if (cell.flagged)
                    return tileFlag_anim;
                return cell.exploded ? tileExploded : tileTrap;

            case Cell.Type.Number:
                return GetNumberTile(cell);

            default:
                return null;
        }
    }

    private Tile GetNumberTile(Cell cell)
    {
        switch (cell.number)
        {
            case 1: return tileNum1;
            case 2: return tileNum2;
            case 3: return tileNum3;
            case 4: return tileNum4;
            case 5: return tileNum5;
            case 6: return tileNum6;
            case 7: return tileNum7;
            case 8: return tileNum8;
            default: return null;
        }
    }

    private TileBase GetUnknownTile()
    {
        if (tileUnknown != null)
        {
            return tileUnknown;
        }
        else
        {
            Debug.LogWarning("No tileUnknown variants assigned!");
            return null;
        }
    }
}
