using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class Player : MonoBehaviour
{
    private static Player instance;
    public static Player Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<Player>();
            return instance;
        }
    }

    Camera cam;
    public float moveDuration = 0.2f; // 한 칸 이동 애니메이션 시간
    public float minSwipeDistance = 50f; // 너무 짧은 입력은 무시

    public Vector2Int currentCell;
    private float yLevel; // 큐브의 y 고정 높이
    private bool isMoving = false;

    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public int stepCount; // 현재 스테이지에서의 이동 횟수
    public Stack<Vector2Int> moveHistory = new(); // 이동 기록 스택
    public TextMeshProUGUI curStepsText;

    private InputAction touchAction;
    private Vector2 startTouch;
    private bool isTouching;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        if (cam == null) cam = Camera.main;

        touchAction = new InputAction("Touch", binding: "<Pointer>/press");
        touchAction.performed += OnTouchPerformed;
        touchAction.canceled += OnTouchCanceled;
    }

    void Start()
    {
        // 초기 그리드 좌표 등록
        yLevel = transform.position.y;
        currentCell = GridManager.Instance.WorldToGrid(transform.position);
        GridManager.Instance.Register(gameObject, currentCell);
        // 위치 스냅
        transform.position = GridManager.Instance.GridToWorld(currentCell, yLevel);

        moveHistory.Clear();
        moveHistory.Push(currentCell);

        stepCount = 1;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        touchAction.Enable();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        touchAction.Disable();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        yLevel = transform.position.y;
        currentCell = GridManager.Instance.WorldToGrid(transform.position);
        GridManager.Instance.Register(gameObject, currentCell);

        transform.position = GridManager.Instance.GridToWorld(currentCell, yLevel);

        moveHistory.Clear();
        moveHistory.Push(currentCell);

        if (scene.name == "GameScene")
            stepCount = StageManager.Instance.GetCurrentStage().maxSteps;
    }

    //void Update()
    //{
    //    if (Input.GetMouseButtonDown(0)) startTouch = Input.mousePosition;
    //    if (Input.GetMouseButtonUp(0))
    //    {
    //        Vector2 endTouch = Input.mousePosition;
    //        Vector2 delta = endTouch - startTouch;
    //        if (delta.magnitude < minSwipeDistance) return; // 너무 짧으면 무시

    //        Vector2Int dir;
    //        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
    //            dir = new Vector2Int(delta.x > 0 ? 1 : -1, 0); // 좌우
    //        else
    //            dir = new Vector2Int(0, delta.y > 0 ? 1 : -1); // 상하

    //        TryMove(dir);
    //    }
    //}

    void LateUpdate()
    {
        curStepsText.text = stepCount.ToString();
    }

    private void OnTouchPerformed(InputAction.CallbackContext ctx)
    {
        startTouch = Pointer.current.position.ReadValue();
        isTouching = true;
    }

    private void OnTouchCanceled(InputAction.CallbackContext ctx)
    {
        if (!isTouching) return;

        Vector2 endTouch = Pointer.current.position.ReadValue();
        Vector2 delta = endTouch - startTouch;
        isTouching = false;

        if (delta.magnitude < minSwipeDistance) return;

        Vector2Int dir;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            dir = new Vector2Int(delta.x > 0 ? 1 : -1, 0);
        else
            dir = new Vector2Int(0, delta.y > 0 ? 1 : -1);

        TryMove(dir);
    }

    public void TryMove(Vector2Int dir)
    {
        if (isMoving) return;
        if (dir == Vector2Int.zero) return;

        StartCoroutine(MoveToEdge(dir));
    }

    private IEnumerator MoveToEdge(Vector2Int dir)
    {
        isMoving = true;

        // 다음 칸부터 검사
        Vector2Int next = currentCell + dir;
        Vector2Int lastValid = currentCell;

        // 이동 가능한 끝 지점 찾기
        while (GridManager.Instance.IsInside(next) && !GridManager.Instance.IsBlocked(next))
        {
            lastValid = next;
            next += dir;
        }

        // 이동할 필요 없으면 종료
        if (lastValid == currentCell)
        {
            isMoving = false;
            yield break;
        }

        Vector3 from = GridManager.Instance.GridToWorld(currentCell, yLevel);
        Vector3 to = GridManager.Instance.GridToWorld(lastValid, yLevel);

        SoundManager.Instance.PlaySFX(SoundManager.SfxTypes.MOVE);

        // Lerp로 부드럽게 이동
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            float eased = moveCurve.Evaluate(t);
            transform.position = Vector3.Lerp(from, to, eased);

            yield return null;
        }

        // 최종 위치 보정
        transform.position = to;
        currentCell = lastValid;
        // 점유 갱신
        GridManager.Instance.UpdatePos(gameObject, currentCell);

        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            moveHistory.Push(currentCell);
            if (stepCount <= 0)
            {
                SoundManager.Instance.PlaySFX(SoundManager.SfxTypes.LOSE);
                StartCoroutine(RewindToStart());
                yield break;
            }

            stepCount--;

            // HintManager.Instance.PlayerStepped(currentCell);
        }

        // 골 도착 체크
        GameManager.Instance.CheckGoalReached(new Vector2(GameManager.Instance.goalPos.x, GameManager.Instance.goalPos.z));

        isMoving = false;
    }

    private IEnumerator RewindToStart()
    {
        isMoving = true;

        while (moveHistory.Count > 1)
        {
            moveHistory.Pop();
            Vector2Int toCell = moveHistory.Peek();

            Vector3 from = GridManager.Instance.GridToWorld(currentCell, yLevel);
            Vector3 to = GridManager.Instance.GridToWorld(toCell, yLevel);

            SoundManager.Instance.PlaySFX(SoundManager.SfxTypes.MOVE);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / moveDuration;
                float eased = moveCurve.Evaluate(t);
                transform.position = Vector3.Lerp(from, to, eased);
                yield return null;
            }

            transform.position = to;
            currentCell = toCell;
            GridManager.Instance.UpdatePos(gameObject, currentCell);
        }

        // 리셋
        stepCount = StageManager.Instance.GetCurrentStage().maxSteps;

        // moveHistory 초기화
        moveHistory.Clear();
        moveHistory.Push(currentCell);

        //if (HintManager.Instance.isHint == true)
        //    HintManager.Instance.ShowHint();

        isMoving = false;
    }
}
