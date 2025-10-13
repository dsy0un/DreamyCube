using UnityEngine;

public class Obstacle : MonoBehaviour
{
    GridManager grid;

    void Awake()
    {
        if (grid == null) grid = FindFirstObjectByType<GridManager>();
    }

    void Start()
    {
        Vector2Int obstacleCell = grid.WorldToGrid(transform.position);
        grid.AddBlocked(obstacleCell);

    }
}
