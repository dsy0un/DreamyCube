using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageData
{
    public int id;
    public int maxSteps;
    public Vector2Int goal;
    public List<Vector2Int> obstacles;
}

public static class JsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> array;
    }
}

public class StageManager : MonoBehaviour
{
    private static StageManager instance;
    public static StageManager Instance
    {
        get
        {
            if (instance == null) instance = new();
            return instance;
        }
    }

    public TextAsset puzzleJson;
    public GameObject obstaclePrefab;
    public GameObject goalPrefab;
    public GameObject cubePrefab;

    private List<StageData> stages;
    public int currentStageIndex;

    private List<GameObject> obstacleInstances = new();
    private GameObject goalInstance;
    private List<GameObject> cubeInstances = new();

    void Awake()
    {
        if (instance == null) instance = this;

        stages = JsonHelper.FromJson<StageData>(puzzleJson.text);
    }

    void Start()
    {
        NextStage();
    }

    public void LoadStage(int index)
    {
        if (index < 0 || index >= stages.Count)
        {
            Debug.LogError("Invalid stage index");
            return;
        }

        ClearStage();

        StageData stage = stages[index];

        // goal 위치 설정
        GameManager.Instance.goalPos = new(stage.goal.x, 0f, stage.goal.y);
        goalInstance = Instantiate(goalPrefab, GameManager.Instance.goalPos, Quaternion.identity);

        // 바닥 타일 생성
        for (int x = 0; x < GridManager.Instance.width; x++)
        {
            for (int y = 0; y < GridManager.Instance.height; y++)
            {
                Vector2Int cell = new(x, y);
                Vector2Int goalCell = GridManager.Instance.WorldToGrid(GameManager.Instance.goalPos);

                if (goalCell == cell) continue;

                Vector3 worldPos = GridManager.Instance.GridToWorld(cell, 0f);
                GameObject cube = Instantiate(cubePrefab, worldPos, Quaternion.identity, GridManager.Instance.map.transform);
                cubeInstances.Add(cube);
            }
        }

        // 장애물 생성
        foreach (var obs in stage.obstacles)
        {
            Vector3 obsPos = new(obs.x, 1f, obs.y);
            GameObject obsInstance = Instantiate(obstaclePrefab, obsPos, Quaternion.identity, GridManager.Instance.map.transform.GetChild(1));
            obstacleInstances.Add(obsInstance);
        }
    }

    public void ClearStage()
    {
        // goal 제거
        if (goalInstance != null) Destroy(goalInstance);

        // 장애물 제거
        foreach (var obs in obstacleInstances) if (obs != null) Destroy(obs);

        // 바닥 타일 제거
        foreach (var cube in cubeInstances) if (cube != null) Destroy(cube);

        obstacleInstances.Clear();
        cubeInstances.Clear();
        goalInstance = null;
        GridManager.Instance.blocked.Clear();
        GridManager.Instance.goalCells.Clear();
    }

    public void NextStage()
    {
        if (currentStageIndex < stages.Count)
        {
            LoadStage(currentStageIndex);
        }
        else
        {
            Debug.Log("모든 스테이지 클리어!");
        }

        currentStageIndex++;
    }

    public StageData GetCurrentStage()
    {
        return stages[Mathf.Clamp(currentStageIndex - 1, 0, stages.Count - 1)];
    }
}
