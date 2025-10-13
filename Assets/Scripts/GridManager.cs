using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    private static GridManager instance;
    public static GridManager Instance
    {
        get
        {
            if (instance == null) instance = new();
            return instance;
        }
    }

    [Header("Board Settings")]
    public int width;
    public int height;
    float cellSize = 1f;
    Vector3 origin;
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] Material finishMaterial;

    public GameObject map;

    // 장애물(고정 오브젝트) 좌표
    public HashSet<Vector2Int> blocked = new();
    // 성공 오브젝트 좌표
    public HashSet<Vector2Int> goalCells = new();
    // 현재 점유 좌표(동적 오브젝트)
    private readonly Dictionary<GameObject, Vector2Int> occupiers = new();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (map == null) map = GameObject.FindGameObjectWithTag("Map");
        switch (map.name)
        {
            case "3x3":
                width = 3;
                height = 3;
                break;
            case "5x5":
                width = 5;
                height = 5;
                break;
            default:
                break;
        }

        origin = new Vector3(-(width - 1) / 2f * cellSize, 0f, -(height - 1) / 2f * cellSize);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (map == null) map = GameObject.FindGameObjectWithTag("Map");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 local = worldPos - origin;
        int x = Mathf.RoundToInt(local.x / cellSize);
        int y = Mathf.RoundToInt(local.z / cellSize); // z축을 그리드 y로 취급

        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int gridPos, float yLevel = 0f)
    {
        return new Vector3(
            origin.x + gridPos.x * cellSize,
            yLevel,
            origin.z + gridPos.y * cellSize
        );
    }

    public bool IsInside(Vector2Int p)
    {
        return p.x >= 0 && p.y >= 0 && p.x < width && p.y < height;
    }

    public bool IsBlocked(Vector2Int p)
    {
        return blocked.Contains(p) || IsOccupied(p);
    }

    public bool IsStaticallyBlocked(Vector2Int p)
    {
        return blocked.Contains(p);
    }

    public bool IsOccupied(Vector2Int p)
    {
        foreach (var kv in occupiers)
        {
            if (kv.Value == p) return true;
        }
        return false;
    }

    public void AddBlocked(Vector2Int p)
    {
        if (IsInside(p)) blocked.Add(p);
    }

    public void AddGoal(Vector2Int cell)
    {
        goalCells.Add(cell);
    }


    public void Register(GameObject go, Vector2Int gridPos)
    {
        occupiers[go] = gridPos;
    }

    public void UnRegister(GameObject go)
    {
        if (occupiers.ContainsKey(go))
            occupiers.Remove(go);
    }

    public void UpdatePos(GameObject go, Vector2Int gridPos)
    {
        if (occupiers.ContainsKey(go))
            occupiers[go] = gridPos;
        else
            occupiers.Add(go, gridPos);
    }
}
