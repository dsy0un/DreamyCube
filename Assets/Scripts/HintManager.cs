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
    public Material trailMaterial; // �帣�� ȿ���� �� Shader (��: Unlit/Transparent)
    public TextMeshProUGUI text;

    readonly float trailWidth = 0.15f; // �� �β�
    readonly float trailY = 0.55f; // ���� (�ٴڿ��� �ణ ���)
    Color startColor = Color.cyan; // ���� ����
    Color endColor = Color.blue; // �� ����

    LineRenderer line; // TrailRenderer ��� LineRenderer ���
    readonly List<Vector3> points = new();
    int currentStartIndex = 0; // �߷����� ���� �ε���

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

            if (d >= maxSteps) continue; // �� �̻� �̵� �Ұ� (���� �̵��� maxSteps �ʰ� ���ɼ�)

            foreach (var dir in dirs)
            {
                // �����̵� : cur���� dir�� �̵����� �� ���ߴ� ������ ���� ĭ�� ���
                Vector2Int next = cur + dir;
                Vector2Int lastValid = cur;

                while (GridManager.Instance.IsInside(next) && !GridManager.Instance.IsStaticallyBlocked(next))
                {
                    lastValid = next;
                    next += dir;
                }

                if (lastValid == cur) continue; // �� ĭ�� �������̸� ����

                if (!parent.ContainsKey(lastValid))
                {
                    parent[lastValid] = cur;
                    depth[lastValid] = d + 1;
                    q.Enqueue(lastValid);
                }

                // ��ǥ ���� �˻� (goal ��ġ�� ��Ȯ�� ����� ��)
                if (lastValid == goal && depth[lastValid] <= maxSteps)
                {
                    isFound = true;
                    // BFS���� �ִ� ��θ� ã�� ���̹Ƿ� ���⼭ ����
                    q.Clear();
                    break;
                }
            }
        }

        if (!isFound)
        {
            Debug.Log("��Ʈ ��θ� ã�� �� �����ϴ�.");
            return;
        }

        isHint = true;

        // ��� ������
        var path = new List<Vector2Int>();
        Vector2Int node = goal;
        while (node != start)
        {
            path.Add(node);
            node = parent[node].Value;
        }
        path.Reverse();

        // LineRenderer ����
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
        Debug.Log($"��Ʈ ǥ��: {path.Count}");
    }

    public void PlayerStepped(Vector2Int cell)
    {
        if (!line || points.Count <= 1) return;

        Vector3 playerPos = GridManager.Instance.GridToWorld(cell, trailY);

        // �÷��̾� ��ġ�� ���� ����� ����Ʈ ã��
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

        if (nearest == -1 || nearest <= currentStartIndex) return; // �̹� ������ ����Ʈ�ų� ����

        // ������ ���� �պκ� �߶󳻱�
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
