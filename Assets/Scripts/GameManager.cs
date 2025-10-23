using UnityEngine;
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

        Application.targetFrameRate = 30;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Period))
        {
            StageManager.Instance.currentStageIndex += 10;
            SkipStage();
        }
        else if (Input.GetKeyDown(KeyCode.Period))
        {
            SkipStage();
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
