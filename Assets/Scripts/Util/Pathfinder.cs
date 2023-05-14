using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
* Finds a path between two cells
*/

public class CellData
{
    public Cell parent;
    public float distFromStart;
    public float heuristic;
    public bool visited;

    public float Score()
    {
        return distFromStart + heuristic;
    }
}

public class Pathfinder : Singleton<Pathfinder>
{
    [SerializeField] private bool logWarnings;

    public IEnumerator FindPath(Cell[] cells, Cell start, Cell destination, bool log, System.Action<List<Cell>> onComplete)
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        List<Cell> path = new();
        List<Cell> cellsToBeTested = new();

        // Make sure the start and end points are valid and can be travelled to
        if (start == null || destination == null)
        {
            if (logWarnings) Debug.LogWarning("Cannot find path for invalid nodes");
            onComplete(null);
            yield break;
        }

        // Nothing to solve if there is a direct connection between these two locations
        Cell directConnection = start.neighbours.Where(c => c == destination).FirstOrDefault();
        if (directConnection != null)
        {
            path.Add(start);
            path.Add(destination);
            onComplete(path);
            yield break;
        }

        stopwatch.Restart();

        // For each grid point store it's pathfinding data (parent, score, heuristic etc)
        Dictionary<Cell, CellData> cellDataDictionary = new();
        foreach (Cell cell in cells)
        {
            cellDataDictionary.Add(cell, new CellData());
        }

        // Maintain a list of nodes to be tested and begin with the start node, keep going as long as there are nodes to test and destination hasn't been reached
        Cell currentCell = start;
        cellsToBeTested.Add(start);

        // Keep track of ticks elapsed - if this gets too large then yield return null to skip to the next frame
        // This avoids stuttering as the function will never spend too long on one frame
        float timer = 0;

        // Keep looping as long as there are still nodes to check and the target hasn't been reached
        while (cellsToBeTested.Count > 0)
        {
            float time = stopwatch.ElapsedTicks;

            // Begin by sorting the list each time by the heuristic
            cellsToBeTested.Sort((a, b) => (int) (cellDataDictionary[a].Score() - cellDataDictionary[b].Score()));

            currentCell = cellsToBeTested[0];

            // Reached destination
            if (currentCell == destination)
            {
                cellDataDictionary[destination].visited = true;
                break;
            }

            // Remove any tiles that have already been visited
            cellsToBeTested.RemoveAll(o => cellDataDictionary[o].visited);

            // Check there are still have locations to visit
            if (cellsToBeTested.Count > 0)
            {
                // Mark this note visited and then process it
                currentCell = cellsToBeTested[0];
                CellData currentCellData = cellDataDictionary[currentCell];
                currentCellData.visited = true;

                // Check each neighbour, if it is accessible and hasn't already been processed then add it to the list to be tested 
                for (int i = 0; i < 4; i++)
                {
                    // If there is a building piece in the way
                    if (currentCell.edges[i].buildingPieceObject) continue;

                    Cell neighbour = currentCell.neighbours[i];
                    if (neighbour == null || neighbour.itemObject) continue;

                    CellData neighbourData = cellDataDictionary[neighbour];

                    if (neighbourData.visited || cellsToBeTested.Contains(neighbour)) continue;

                    neighbourData.distFromStart = currentCellData.distFromStart + distance(currentCell, neighbour);
                    neighbourData.heuristic = heuristic(neighbour, destination);
                    neighbourData.parent = currentCell;
                    cellsToBeTested.Add(neighbour);
                }
            }

            // If taking too long skip to the next frame to avoid freezing
            timer += stopwatch.ElapsedTicks - time;
            if (timer > 50000)
            {
                timer = 0;
                stopwatch.Restart();
                yield return null;
            }
        }

        stopwatch.Stop();

        // Trace the path back through the parents then reverse it to return the correct route
        if (cellDataDictionary[destination].visited)
        {
            Cell routeCell = destination;

            while (cellDataDictionary[routeCell].parent != null)
            {
                path.Add(routeCell);
                routeCell = cellDataDictionary[routeCell].parent;
                // yield return null;
            }
            path.Add(routeCell);
            path.Reverse();

            onComplete(path);

            float time = (float)stopwatch.ElapsedTicks / System.TimeSpan.TicksPerMillisecond;
            if (log) Debug.LogFormat("Found path with length of {0} in {1}ms", path.Count, time);
        }
        else
        {
            if (log) Debug.LogWarning("Path not found");
            onComplete(null);
        }
    }

    // Length of a connection between two nodes
    private float distance(Cell a, Cell b)
    {
        return Vector2.Distance(a.Coords, b.Coords);
    }

    // Estimate how close two points are, atm this is just line of sight as a quick but slightly innacurate solution
    private float heuristic(Cell a, Cell b)
    {
        return Vector2.Distance(a.Coords, b.Coords);
    }

    public static List<Vector3> SmoothPath(List<Vector3> path)
    {
        // Can't smooth a path without enouch points
        if (path == null || path.Count < 2) return path;

        List<Vector3> newPath = new()
        {
            path.First()
        };

        float dist = 0.4f;
        int steps = 3;

        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector3 prev = path[i - 1];
            Vector3 next = path[i + 1];

            Vector3 p0 = Vector3.Lerp(path[i], prev, dist);
            Vector3 p1 = Vector3.Lerp(path[i], next, dist);

            for (int s = 0; s < steps; s++)
            {
                float t = (float)s / (steps - 1);
                Vector3 p = Vector3.Lerp(Vector3.Lerp(p0, path[i], t), Vector3.Lerp(path[i], p1, t), t);
                newPath.Add(p);
            }
        }

        newPath.Add(path.Last());
        return newPath;
    }

    public static void RemoveRedundantNodes(List<Vector3> path)
    {
        List<Vector3> nodesToRemove = new List<Vector3>();

        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector3 dirFromPrevious = (path[i] - path[i - 1]).normalized;
            Vector3 dirToNext = (path[i + 1] - path[i]).normalized;

            if (dirToNext == dirFromPrevious)
            {
                nodesToRemove.Add(path[i]);
            }
        }

        foreach (Vector3 node in nodesToRemove)
        {
            path.Remove(node);
        }
    }
}