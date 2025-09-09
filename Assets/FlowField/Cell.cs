using UnityEngine;

public class Cell
{
    public Vector3 worldPos;
    public int gridX;
    public int gridY;

    public ushort cost; // Cost to traverse this cell
    public uint integrationCost; // Cost from this cell to the target

    public int bestDirectionX;
    public int bestDirectionY;

    public Cell(Vector3 _worldPosition, Vector2Int _index)
    {
        worldPos = _worldPosition;
        gridX = _index.x;
        gridY = _index.y;
        integrationCost = uint.MaxValue;
        bestDirectionX = 0;
        bestDirectionY = 0;

    }

    public void SetCost(ushort _cost)
    {
        cost = _cost;
    }

    public void MaxCost()
    {
        cost = ushort.MaxValue;
    }

}
