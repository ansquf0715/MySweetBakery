using GoogleMobileAds;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public GameObject menuPage;

    MoneyManager moneyManager;

    //string adUnitId;

#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-3940256099942544/5224354917";
#else
    private string _adUnitId = "unexpected_platform";
#endif

    RewardedAd _rewardedAd;

    // Start is called before the first frame update
    void Start()
    {
        moneyManager = FindObjectOfType<MoneyManager>();

        //모바일 광고 sdk 초기화(앱 실행시 한 번만 처리)
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            // This callback is called once the MobileAds SDK is initialized.
        });

        LoadRewardedAd();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadRewardedAd() //보상형 광고 로드
    {
        //Clean up the old ad before loading a new one.
        if(_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad");

        //create our request used to load the ad.
        var adRequest = new AdRequest();

        //send the request to load the ad.
        RewardedAd.Load(_adUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                //if error is not null, the load request failed
                if (error != null || ad == null)
                {
                    Debug.Log("Rewarded ad failed to load an ad " +
                        "with error: " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response:"
                    + ad.GetResponseInfo());

                _rewardedAd = ad;

                RegisterEventHandlers(_rewardedAd);
                RegisterReloadHandler(_rewardedAd);
            });
    }

    public void ShowRewardedAd() //보상형 광고 표시
    {
        const string rewardMsg =
            "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                Debug.Log("show rewarded Ad");
                // TODO: Reward the user.
                GiveReward();
                //Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
            });
        }
    }

    void GiveReward()
    {
        //moneyManager.money += 100;
        moneyManager.getMoneyFromAd();
        //Debug.Log($"Rewarded! Current money: {money}");
    }

    private void RegisterEventHandlers(RewardedAd ad) //보상형 광고 이벤트 수신
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
        };
    }

    private void RegisterReloadHandler(RewardedAd ad) //다음 보상형 광고 미리 로드
    {
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded Ad full screen content closed.");

            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);

            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };
    }

    public void ToggleMenu()
    {
        if(menuPage.activeSelf == false)
        {
            menuPage.SetActive(true);

            Time.timeScale = 0f;
        }
    }

    public void exitMenu()
    {
        Time.timeScale = 1f;

        menuPage.SetActive(false);
    }
}
