using GoogleMobileAds.Api;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoogleMobileAdsManager : MonoBehaviour
{
    [SerializeField] bool isRaunch;
    [SerializeField] bool isDetailLog;
    [SerializeField] string[] testDeviceIds;
    [SerializeField] string bannerIdAndroid;
    [SerializeField] string bannerIdIos;
    //[SerializeField] string nativeIdAndroid;
    //[SerializeField] string nativeIdIos;
    [SerializeField] string frontIdAndroid;
    [SerializeField] string frontIdIos;
    [SerializeField] string rewardedIdAndroid;
    [SerializeField] string rewardedIdIos;

    SynchronizationContext context;

    private static GoogleMobileAdsManager instance;
    public static GoogleMobileAdsManager Instance
    {
        get
        {
            if (instance == null) instance = new();
            return instance;
        }
    }

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
        context = SynchronizationContext.Current;
        if (IsAndroidAPK() && isRaunch)
        {
            isRaunch = false;
            if (isDetailLog) Debug.Log("Android APK detected, disabling raunch mode.");
        }

        RequestConfiguration requestConfiguration = new();
        requestConfiguration.TestDeviceIds.AddRange(testDeviceIds);
        MobileAds.SetRequestConfiguration(requestConfiguration);

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => 
        { 
            Debug.Log("Initialized Google Mobile Ads");

            //if (!string.IsNullOrEmpty(bannerIdAndroid) || !string.IsNullOrEmpty(bannerIdIos))
            //{ 
            //    LoadBannerAd();
            //}
            LoadBannerAd();
            //if (!string.IsNullOrEmpty(nativeIdAndroid) || !string.IsNullOrEmpty(nativeIdIos))
            //{ 
            //    LoadNativeAd();
            //}
            //LoadNativeAd();
            //if (!string.IsNullOrEmpty(frontIdAndroid) || !string.IsNullOrEmpty(frontIdIos))
            //{
            //    LoadFrontAd();
            //}
            LoadFrontAd();
            //if (!string.IsNullOrEmpty(rewardedIdAndroid) || !string.IsNullOrEmpty(rewardedIdIos))
            //{
            //    LoadRewardedAd();
            //}
            LoadRewardedAd();
        });
    }

    bool IsAndroidAPK()
    {
        string bundleId = Application.identifier;
        return bundleId.EndsWith(".apk");
    }

    #region Banner Ad

    BannerView bannerView;

    public void LoadBannerAd()
    {
#if UNITY_ANDROID
        string bannerId = bannerIdAndroid;
        string bannerIdTest = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE || UNITY_IOS
        string bannerId = bannerIdIos;
        string bannerIdTest = "ca-app-pub-3940256099942544/2934735716";
#endif
        string finalBannerId = isRaunch ? bannerId : bannerIdTest;

        bannerView?.Destroy();

        bannerView = new BannerView(finalBannerId, AdSize.Banner, AdPosition.Bottom);
        bannerView.LoadAd(new AdRequest());

        if (isDetailLog) Debug.Log("Banner ad loading...");
    }

    public void ShowBannerAd(bool isShow)
    {
        if (bannerView != null)
        {
            if (isShow) bannerView.Show();
            else bannerView.Hide();
        }
        else
        {
            if (isDetailLog) Debug.LogError("Banner ad not loaded.");
            if (isShow) LoadBannerAd();
        }
    }
    #endregion

    #region Front Ad

    InterstitialAd frontAd;

    public void LoadFrontAd()
    {
#if UNITY_ANDROID
        string frontId = frontIdAndroid;
        string frontIdTest = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE || UNITY_IOS
        string frontId = frontIdIos;
        string frontIdTest = "ca-app-pub-3940256099942544/4411468910";
#endif
        string finalFrontId = isRaunch ? frontId : frontIdTest;

        InterstitialAd.Load(finalFrontId, new AdRequest(), (InterstitialAd ad, LoadAdError error) =>
        {
            if (ad == null || error != null)
            {
                if (isDetailLog) Debug.LogWarning($"Interstitial ad failed to load: {error?.GetMessage()}");
                return;
            }

            if (isDetailLog) Debug.Log("Interstitial ad loaded.");
            frontAd = ad;
            InterstitialEventHandlers(ad);
        });
    }

    public void ShowFrontAd()
    {
        if (frontAd != null && frontAd.CanShowAd()) frontAd.Show();
        else
        {
            if (isDetailLog) Debug.LogError("Interstitial ad cannot be shown.");
        }

        LoadFrontAd();
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

                LoadBannerAd();
                LoadRewardedAd();
            }
        };

        // 전면 광고가 노출에 실패했을 때 이벤트
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content: " + error);
        };
    }
    #endregion

    #region Rewarded Ad

    RewardedAd rewardedAd;

    public void LoadRewardedAd()
    {
#if UNITY_ANDROID
        string rewardedId = rewardedIdAndroid;
        string rewardedIdTest = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE || UNITY_IOS
		string rewardedId = rewardIdIos;
		string rewardedIdTest = "ca-app-pub-3940256099942544/1712485313";
#endif
        string finalRewardedId = isRaunch ? rewardedId : rewardedIdTest;

        RewardedAd.Load(finalRewardedId, new AdRequest(), (RewardedAd ad, LoadAdError error) =>
        {
            if (ad == null || error != null)
            {
                if (isDetailLog) Debug.LogWarning($"Rewarded ad failed to load: {error?.GetMessage()}");
                return;
            }

            if (isDetailLog) Debug.Log("Rewarded ad loaded.");
            rewardedAd = ad;
            RewardedEventHandlers(ad);
        });
    }

    public void ShowRewardedAd(Action<bool> completed)
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                context.Post(o =>
                {
                    if (isDetailLog) Debug.Log($"Rewarded ad granted a reward: {reward.Amount}");
                    HintManager.Instance.ShowHint();
                    completed?.Invoke(true);
                }, null);
            });
        }
        else
        {
            if (isDetailLog) Debug.LogError("Rewarded ad cannot be shown.");
            completed?.Invoke(false);
        }

        LoadRewardedAd();
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
    #endregion
}
