using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = new();
            return instance;
        }
    }

    public Vector3 goalPos;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        goalPos = new Vector3(2f, 0f, 0f);

        SoundManager.Instance.PlayBGM(SoundManager.BgmTypes.TITLE);

        Application.targetFrameRate = 60;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // period가 '이번 프레임에' 눌렸는지 확인하고, 그 순간 shift가 눌려있으면 10 증가
        if (keyboard.periodKey.wasPressedThisFrame)
        {
            bool shiftPressed = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            if (shiftPressed)
            {
                StageManager.Instance.currentStageIndex += 10;
                SkipStage();
            }
            else
            {
                SkipStage();
            }
        }
    }

    public void CheckGoalReached(Vector2 goalPos)
    {
        Vector2 playerPos = new(Player.Instance.transform.position.x, Player.Instance.transform.position.z);

        if (Vector2.Distance(playerPos, goalPos) < 0.1f)
        {
            if (SceneManager.GetActiveScene().name == "MainScene")
            {
                GoogleMobileAdsManager.Instance.ShowInterstitialAd();
            }
            else
            {
                Debug.Log($"{StageManager.Instance.GetCurrentStage().id} 스테이지 클리어");

                SoundManager.Instance.PlaySFX(SoundManager.SfxTypes.CLEAR);
                StageManager.Instance.NextStage();

                Player.Instance.stepCount = StageManager.Instance.GetCurrentStage().maxSteps;
                Player.Instance.moveHistory.Clear();
                Player.Instance.moveHistory.Push(Player.Instance.currentCell);

                if (HintManager.Instance.hintCount < 3 && !HintManager.Instance.isHint)
                    HintManager.Instance.hintCount++;
                HintManager.Instance.ClearHints();

                GoogleMobileAdsManager.Instance.LoadRewardedAd();
            }
        }
    }

    public void SkipStage()
    {
        Player.Instance.transform.position = new(StageManager.Instance.GetCurrentStage().goal.x, 1f, StageManager.Instance.GetCurrentStage().goal.y);
        Player.Instance.currentCell = GridManager.Instance.WorldToGrid(Player.Instance.transform.position);
        GridManager.Instance.UpdatePos(Player.Instance.gameObject, Player.Instance.currentCell);
        CheckGoalReached(new Vector2(StageManager.Instance.GetCurrentStage().goal.x, StageManager.Instance.GetCurrentStage().goal.y));
    }
}
