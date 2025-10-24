using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoogleMobileAdsManager : MonoBehaviour
{
    private static GoogleMobileAdsManager instance;
    public static GoogleMobileAdsManager Instance
    {
        get
        {
            if (instance == null) instance = new();
            return instance;
        }
    }

    string adInterstitialId;
    string adRewardedId;

    InterstitialAd interstitialAd;
    RewardedAd rewardedAd;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => { Debug.Log("Initialized Google Mobile Ads"); });
    }

    void Start()
    {
#if UNITY_ANDROID
        adInterstitialId = "ca-app-pub-3940256099942544/1033173712"; // Test Ad Interstitial ID for Android
        adRewardedId = "ca-app-pub-3940256099942544/5224354917"; // Test Ad Rewarded ID for Android
#elif UNITY_IPHONE
        adInterstitialId = "ca-app-pub-3940256099942544/1033173712"; // Test Ad Interstitial ID for Android
        adRewardedId = "ca-app-pub-3940256099942544/1033173712"; // Test Ad Rewarded ID for Android
#endif

        LoadInterstitialAd();
    }

    void LoadInterstitialAd()
    {
        interstitialAd?.Destroy();
        interstitialAd = null;

        var adRequest = new AdRequest();

        InterstitialAd.Load(adInterstitialId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null)
            {
                Debug.LogError("Failed to load interstitial ad: " + error);
                return;
            }

            Debug.Log($"Interstitial ad loaded with response : {ad.GetResponseInfo()}");

            interstitialAd = ad;
            InterstitialEventHandlers(interstitialAd);
        });
    }

    public void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Ad not ready to be shown.");
            LoadInterstitialAd();
        }
    }

    void InterstitialEventHandlers(InterstitialAd ad)
    {
        // 광고 지급 관련 이벤트
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log($"Interstitial ad paid {adValue.Value} {adValue.CurrencyCode}.");
        };

        // 중간 광고 노출 이벤트
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };

        // 광고 클릭 이벤트
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };

        // 전면 광고가 열렸을 때 이벤트
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };

        // 전면 광고가 닫혔을 때 이벤트
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");

            if (SceneManager.GetActiveScene().name == "MainScene")
            {
                Debug.Log("게임 시작");
                SceneManager.LoadScene("GameScene");

                SoundManager.Instance.StopBGM();
                SoundManager.Instance.PlayBGM(SoundManager.BgmTypes.GAME);
                SoundManager.Instance.PlaySFX(SoundManager.SfxTypes.CLEAR);

                LoadRewardedAd();
            }
        };

        // 전면 광고가 노출에 실패했을 때 이벤트
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content: " + error);
        };
    }

    public void LoadRewardedAd()
    {
        rewardedAd?.Destroy();
        rewardedAd = null;

        var adRequest = new AdRequest();

        RewardedAd.Load(adRewardedId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null)
            {
                Debug.LogError("Failed to load rewarded ad: " + error);
                return;
            }
            Debug.Log($"Rewarded ad loaded with response : {ad.GetResponseInfo()}");

            rewardedAd = ad;
            RewardedEventHandlers(rewardedAd);
        });
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            rewardedAd.Show((Reward reward) =>
            {
                HintManager.Instance.ShowHint();
            });
        }
        else
        {
            Debug.LogError("Ad not ready to be shown.");
            LoadRewardedAd();
        }
    }

    void RewardedEventHandlers(RewardedAd ad)
    {
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log($"Rewarded ad paid {adValue.Value} {adValue.CurrencyCode}.");
        };

        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };

        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };

        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content: " + error);
        };
    }
}
