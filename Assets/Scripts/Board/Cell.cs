using UnityEngine;

public class Cell
{
    public enum Type
    {
        Empty,
        Trap,
        Number,
        Pillar,
        Shrine,
        Curse
    }

    public Vector3Int position;
    public Type type;
    public int number;
    public bool revealed;
    public bool flagged;
    public bool exploded;
}
