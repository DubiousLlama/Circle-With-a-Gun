using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowFieldController : MonoBehaviour
{
    public RectTransform gridArea;
    public float cellSize = 1f;

    public FlowField flowField { get; private set; }

    public GameObject player;

    float time = 0;

    // Start is called before the first frame update
    void Start()
    {
        flowField = new FlowField(gridArea, cellSize);
        Debug.Log("Flow field initialized with " + flowField.grid.GetLength(0) + " columns and " + flowField.grid.GetLength(1) + " rows.");

        flowField.AssignCosts();
        // display the number of cells with max cost vs normal cost
        Debug.Log("Costs initialized with " + flowField.GetNumTerrainCells() + " terrain cells. " + flowField.rows * flowField.columns);

        flowField.DebugGrid();
    }

    //private void Update()
    //{
    //    time+= Time.deltaTime;

    //    if (time > 1f)
    //    {
    //        time = 0;

    //        Cell playerCell = flowField.GetCellFromWorldPosition(player.transform.position);
    //        flowField.destinationCell = playerCell;

    //        // Time how long it takes to create the integration field and populate directions
    //        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    //        flowField.CreateIntegrationField();
    //        stopwatch.Stop();
    //        Debug.Log($"Integration field created in {stopwatch.ElapsedMilliseconds}ms ({stopwatch.ElapsedTicks} ticks)");

    //        stopwatch = System.Diagnostics.Stopwatch.StartNew();
    //        flowField.PopulateDirections();
    //        stopwatch.Stop();
    //        Debug.Log($"Directions populated in {stopwatch.ElapsedMilliseconds}ms ({stopwatch.ElapsedTicks} ticks)");
    //    }

    //}

    private void FixedUpdate()
    {
        // Set the destination cell to the player
        Cell playerCell = flowField.GetCellFromWorldPosition(player.transform.position);
        flowField.destinationCell = playerCell;

        // Time how long it takes to create the integration field and populate directions
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        flowField.CreateIntegrationField();
        stopwatch.Stop();
        Debug.Log($"Integration field created in {stopwatch.ElapsedMilliseconds}ms ({stopwatch.ElapsedTicks} ticks)");

        stopwatch = System.Diagnostics.Stopwatch.StartNew();
        flowField.PopulateDirections();
        stopwatch.Stop();
        Debug.Log($"Directions populated in {stopwatch.ElapsedMilliseconds}ms ({stopwatch.ElapsedTicks} ticks)");
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (flowField?.grid == null) return;

        for (int x = 0; x < flowField.grid.GetLength(0); x++)
        {
            for (int y = 0; y < flowField.grid.GetLength(1); y++)
            {
                Cell cell = flowField.grid[x, y];
                Gizmos.color = (cell.cost == ushort.MaxValue) ? Color.red : Color.green;

                // Draw cell as wireframe cube
                Vector3 cellPos = cell.worldPos;
                Vector3 size = Vector3.one * flowField.cellDiameter;
                Gizmos.DrawWireCube(cellPos, size);

                // Draw flow field direction arrows
                if (cell.cost != ushort.MaxValue && (cell.bestDirectionX != 0 || cell.bestDirectionY != 0))
                {
                    Gizmos.color = Color.blue;
                    Vector3 direction = new Vector3(cell.bestDirectionX, cell.bestDirectionY, 0).normalized;
                    Vector3 arrowEnd = cellPos + direction * flowField.cellRadius * 0.7f;

                    // Draw arrow line
                    Gizmos.DrawLine(cellPos, arrowEnd);

                    // Draw simple arrowhead
                    Vector3 arrowHeadSize = direction * flowField.cellRadius * 0.2f;
                    Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0) * flowField.cellRadius * 0.15f;
                    Gizmos.DrawLine(arrowEnd, arrowEnd - arrowHeadSize + perpendicular);
                    Gizmos.DrawLine(arrowEnd, arrowEnd - arrowHeadSize - perpendicular);
                }
            }
        }
#endif
    }
}
