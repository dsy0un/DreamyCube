using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    private static HintManager instance;
    public static HintManager Instance
    {
        get
        {
            if (instance == null) instance = new();
            return instance;
        }
    }

    [Header("Hint Settings")]
    public int hintCount = 3;
    public Material trailMaterial; // 흐르는 효과를 줄 Shader (예: Unlit/Transparent)
    public TextMeshProUGUI text;

    readonly float trailWidth = 0.15f; // 선 두께
    readonly float trailY = 0.55f; // 높이 (바닥에서 약간 띄움)
    Color startColor = Color.cyan; // 시작 색상
    Color endColor = Color.blue; // 끝 색상

    LineRenderer line; // TrailRenderer 대신 LineRenderer 사용
    readonly List<Vector3> points = new();
    int currentStartIndex = 0; // 잘려나간 시작 인덱스

    public bool isHint = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        text.text = hintCount.ToString();
    }

    public void ShowHint()
    {
        ClearHints();

        Vector2Int start = Player.Instance.currentCell;
        Vector2Int goal = GridManager.Instance.WorldToGrid(GameManager.Instance.goalPos);

        int maxSteps = StageManager.Instance.GetCurrentStage().maxSteps;

        var dirs = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        var q = new Queue<Vector2Int>();
        var parent = new Dictionary<Vector2Int, Vector2Int?>(); // predecessor (null = start)
        var depth = new Dictionary<Vector2Int, int>();

        q.Enqueue(start);
        parent[start] = null;
        depth[start] = 0;

        bool isFound = false;

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            int d = depth[cur];

            if (d >= maxSteps) continue; // 더 이상 이동 불가 (다음 이동은 maxSteps 초과 가능성)

            foreach (var dir in dirs)
            {
                // 슬라이딩 : cur에서 dir로 이동했을 때 멈추는 마지막 정지 칸을 계산
                Vector2Int next = cur + dir;
                Vector2Int lastValid = cur;

                while (GridManager.Instance.IsInside(next) && !GridManager.Instance.IsStaticallyBlocked(next))
                {
                    lastValid = next;
                    next += dir;
                }

                if (lastValid == cur) continue; // 한 칸도 못움직이면 무시

                if (!parent.ContainsKey(lastValid))
                {
                    parent[lastValid] = cur;
                    depth[lastValid] = d + 1;
                    q.Enqueue(lastValid);
                }

                // 목표 도달 검사 (goal 위치에 정확히 멈춰야 함)
                if (lastValid == goal && depth[lastValid] <= maxSteps)
                {
                    isFound = true;
                    // BFS지만 최단 경로를 찾는 것이므로 여기서 종료
                    q.Clear();
                    break;
                }
            }
        }

        if (!isFound)
        {
            Debug.Log("힌트 경로를 찾을 수 없습니다.");
            return;
        }

        isHint = true;

        // 경로 역추적
        var path = new List<Vector2Int>();
        Vector2Int node = goal;
        while (node != start)
        {
            path.Add(node);
            node = parent[node].Value;
        }
        path.Reverse();

        // LineRenderer 생성
        GameObject go = new("HintTrail");
        line = go.AddComponent<LineRenderer>();
        line.material = new Material(trailMaterial);
        line.textureMode = LineTextureMode.Tile;
        line.material.mainTextureScale = new Vector2(0.5f, 0.5f);
        line.numCapVertices = 8;
        line.numCornerVertices = 8;
        line.alignment = LineAlignment.View;
        line.widthCurve = AnimationCurve.Linear(0f, trailWidth, 1f, trailWidth);
        line.useWorldSpace = true;

        points.Clear();
        points.Add(GridManager.Instance.GridToWorld(start, trailY));

        foreach (var cell in path) points.Add(GridManager.Instance.GridToWorld(cell, trailY));

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());

        Gradient grad = new();
        grad.SetKeys(
            new GradientColorKey[] { new(startColor, 0f), new(endColor, 1f) },
            new GradientAlphaKey[] { new(1f, 0f), new(1f, 1f) }
        );
        line.colorGradient = grad;

        currentStartIndex = 0;
        Debug.Log($"힌트 표시: {path.Count}");
    }

    public void PlayerStepped(Vector2Int cell)
    {
        if (!line || points.Count <= 1) return;

        Vector3 playerPos = GridManager.Instance.GridToWorld(cell, trailY);

        // 플레이어 위치에 가장 가까운 포인트 찾기
        int nearest = -1;
        float nearestDist = float.MaxValue;
        for (int i = currentStartIndex; i < points.Count; i++)
        {
            float dist = Vector3.Distance(points[i], playerPos);
            if (dist < nearestDist)
            {
                nearest = i;
                nearestDist = dist;
            }
        }

        if (nearest == -1 || nearest <= currentStartIndex) return; // 이미 지나간 포인트거나 없음

        // 실제로 라인 앞부분 잘라내기
        currentStartIndex = nearest;
        int remaining = points.Count - currentStartIndex;
        //if (remaining <= 1)
        //{
        //    ClearHints();
        //    return;
        //}

        Vector3[] newPoints = new Vector3[remaining];
        points.CopyTo(currentStartIndex, newPoints, 0, remaining);
        line.positionCount = remaining;
        line.SetPositions(newPoints);
    }

    public void ClearHints()
    {
        if (line != null)
        {
            Destroy(line.gameObject);
            line = null;
        }

        points.Clear();
        currentStartIndex = 0;
        isHint = false;
    }
}
