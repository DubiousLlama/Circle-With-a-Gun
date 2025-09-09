using System.Collections.Generic;
using UnityEngine;

public class FlowField
{
    public Cell[,] grid { get; private set; }
    public float cellRadius { get; private set; }
    public float cellDiameter { get; private set; }

    public Cell destinationCell { get; set; }

    RectTransform gridArea;
    float targetCellSize;
    int rows;
    int columns;

    public FlowField(RectTransform area, float cellSize)
    {
        gridArea = area;
        targetCellSize = cellSize;
        SetupGrid();
    }

    void SetupGrid()
    {
        // Calculate the number of rows and columns such that each cell is approximately 0.4 units in size (but possibly slightly different to fit evenly)
        rows = Mathf.RoundToInt(gridArea.rect.height / targetCellSize);
        columns = Mathf.RoundToInt(gridArea.rect.width / targetCellSize);
        cellDiameter = Mathf.Min(gridArea.rect.height / rows, gridArea.rect.width / columns);

        cellRadius = cellDiameter / 2f;

        grid = new Cell[columns, rows];

        // Create the grid of cells
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector2 cellPosition = new Vector2(
                    gridArea.rect.xMin + x * cellDiameter + cellRadius,
                    gridArea.rect.yMin + y * cellDiameter + cellRadius);

                Vector3 worldPos = gridArea.transform.TransformPoint(cellPosition);
                grid[x, y] = new Cell(worldPos, new Vector2Int(x, y));
            }
        }
    }

    public void AssignCosts()
    {
        Vector2 cellHalfExtents = Vector2.one * cellRadius;
        int obstacleMask = LayerMask.GetMask("Obstacle");


        foreach (Cell curCell in grid)
        {
            Collider2D hit = Physics2D.OverlapBox(curCell.worldPos, cellHalfExtents, 0, obstacleMask);
            if (hit != null)
            {
                curCell.MaxCost();
            }
            else
            {
                curCell.SetCost(1);
            }
        }
    }

    public void CreateIntegrationField()
    {
        if (destinationCell == null)
        {
            Debug.LogError("Destination cell is not set.");
            return;
        }

        ushort startingcost = destinationCell.cost;
        destinationCell.integrationCost = 0;
        destinationCell.cost = 0;

        // First, set all cells' integration costs to max value
        foreach (Cell cell in grid)
        {
            cell.integrationCost = uint.MaxValue;
        }

        // Breadth-first search to propagate integration costs
        Queue<Cell> cellsToCheck = new Queue<Cell>();
        cellsToCheck.Enqueue(destinationCell);
        while (cellsToCheck.Count > 0)
        {
            Cell currentCell = cellsToCheck.Dequeue();
            foreach (Vector2Int direction in GridDirection.CardinalAndIntercardinalDirections)
            {
                Vector2Int neighborIndex = currentCell.gridIndex + direction;
                if (neighborIndex.x >= 0 && neighborIndex.x < columns && neighborIndex.y >= 0 && neighborIndex.y < rows)
                {
                    Cell neighborCell = grid[neighborIndex.x, neighborIndex.y];
                    if (neighborCell.cost == short.MaxValue) // Skip impassable cells
                        continue;
                    uint newCost = currentCell.integrationCost + neighborCell.cost;
                    if (newCost < neighborCell.integrationCost)
                    {
                        neighborCell.integrationCost = newCost;
                        cellsToCheck.Enqueue(neighborCell);
                    }
                }
            }
        }

        destinationCell.cost = startingcost;
    }


    public void PopulateDirections()
    {
        int maxX = columns - 1;
        int maxY = rows - 1;

        // Define neighbor checks only as tuples to avoid repeated allocations without using GridDirection instances
        var neighborChecks = new (int dx, int dy)[]
        {
            (0, 1),
            (0, -1),
            (1, 0),
            (-1, 0),
            (1, 1),
            (-1, 1),
            (1, -1),
            (-1, -1)
        };

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                uint cellCost = grid[x, y].integrationCost;

                foreach (var (dx, dy) in neighborChecks)
                {
                    int neighborX = x + dx;
                    int neighborY = y + dy;

                    if (neighborX >= 0 && neighborX <= maxX && neighborY >= 0 && neighborY <= maxY)
                    {
                        uint neighborCost = grid[neighborX, neighborY].integrationCost;
                        if (neighborCost < cellCost) {
                            cellCost = neighborCost;
                            grid[x, y].bestDirectionX = dx;
                            grid[x, y].bestDirectionY = dy;
                        }
                    }
                }
            }
        }
    }


    public Cell GetCellFromWorldPosition(Vector3 worldPos)
    {
        Vector2 localPos = gridArea.InverseTransformPoint(worldPos);
        float percentX = Mathf.Clamp01((localPos.x - gridArea.rect.xMin) / gridArea.rect.width);
        float percentY = Mathf.Clamp01((localPos.y - gridArea.rect.yMin) / gridArea.rect.height);
        int x = Mathf.Clamp(Mathf.FloorToInt(percentX * columns), 0, columns - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(percentY * rows), 0, rows - 1);
        return grid[x, y];
    }

    public int GetNumTerrainCells()
    {
        int count = 0;
        foreach (Cell curCell in grid)
        {
            if (curCell.cost == ushort.MaxValue)
            {
                count++;
            }
        }
        return count;
    }

    public void DebugGrid()
    {
#if UNITY_EDITOR
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Cell cell = grid[x, y];
                Color cellColor = (cell.cost == ushort.MaxValue) ? Color.red : Color.green;
                Vector3 cellPos = cell.worldPos;
                
                // Draw cell boundaries with shorter duration (1 second instead of 100)
                Vector3 bottomLeft = new Vector3(cellPos.x - cellRadius, cellPos.y - cellRadius, 0);
                Vector3 bottomRight = new Vector3(cellPos.x + cellRadius, cellPos.y - cellRadius, 0);
                Vector3 topLeft = new Vector3(cellPos.x - cellRadius, cellPos.y + cellRadius, 0);
                Vector3 topRight = new Vector3(cellPos.x + cellRadius, cellPos.y + cellRadius, 0);
                Debug.DrawLine(bottomLeft, bottomRight, cellColor, 1.1f); // Slightly longer than update interval
                Debug.DrawLine(bottomRight, topRight, cellColor, 1.1f);
                Debug.DrawLine(topRight, topLeft, cellColor, 1.1f);
                Debug.DrawLine(topLeft, bottomLeft, cellColor, 1.1f);
            }
        }
#endif
    }
}
