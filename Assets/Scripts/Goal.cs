using UnityEngine;

public class Goal : MonoBehaviour
{
    GridManager grid;

    void Awake()
    {
        if (grid == null) grid = FindFirstObjectByType<GridManager>();
    }

    void Start()
    {
        Vector2Int goalCell = grid.WorldToGrid(transform.position);
        grid.AddGoal(goalCell);
    }
}
