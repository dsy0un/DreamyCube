using UnityEngine;

public class UIManager : MonoBehaviour
{
    public void OnClickHintButton()
    {
        if (HintManager.Instance.isHint) return;

        if (HintManager.Instance.hintCount > 0)
        {
            HintManager.Instance.ShowHint();
            HintManager.Instance.hintCount--;
        }
        else
        {
            GoogleMobileAdsManager.Instance.ShowRewardedAd(_ => {});
        }
    }
}
