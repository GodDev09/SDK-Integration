using System;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;

//  ---------------------------------------------
//  Author:     DatDz <steven@atsoft.io> 
//  Copyright (c) 2022 AT Soft
// ----------------------------------------------
#if AOA_TYPE_ADMOB
namespace ATSoft.Ads
{
    public class CustomAppOpenAdmob : BaseAppOpenAds, ICustomAppOpenAds
    {
        private AppOpenAd ad;
        private DateTime loadTime;

        public void InitializeAds(AppOpenAdvertiserSettings appOpenAdvertiserSettings, bool enableTestAd)
        {
            this.enableTestAd = enableTestAd;
            ID_TIER_1 = appOpenAdvertiserSettings.ID_TIER_1;
            ID_TIER_2 = appOpenAdvertiserSettings.ID_TIER_2;
            ID_TIER_3 = appOpenAdvertiserSettings.ID_TIER_3;
            ID_TEST = "ca-app-pub-3940256099942544/3419835294";
            MobileAds.Initialize(InitComplete);
        }

        private void InitComplete(InitializationStatus status)
        {
            Debug.Log("InitComplete AOA Admob");
            MobileAds.SetiOSAppPauseOnBackground(true);
            LoadAppOpenAd();
        }

        public bool IsAppOpenAdAvailable()
        {
            var available = ad != null && initialized;
            return available;
        }

        public void LoadAppOpenAd()
        {
            var id = tierIndex switch
            {
                2 => ID_TIER_2,
                3 => ID_TIER_3,
                _ => ID_TIER_1
            };

            if (enableTestAd)
            {
                id = ID_TEST;
            }
            
            Debug.Log($"Start request Open App Ads: Test {enableTestAd}- Tier{tierIndex}- ID:{id}");

            AdRequest request = new AdRequest.Builder().Build();

            AppOpenAd.LoadAd(id, ScreenOrientation.Portrait, request, ((appOpenAd, error) =>
            {
                if (error != null)
                {
                    // Handle the error.
                    Debug.LogFormat(
                        $"Failed to load AOA tier {tierIndex} - id: {id}. Reason: {error.LoadAdError.GetMessage()}");
                    tierIndex++;
                    if (tierIndex <= 3)
                        LoadAppOpenAd();
                    else
                        tierIndex = 1;

                    return;
                }

                Debug.Log("LoadAppOpenAd App open ad is loaded");
                // App open ad is loaded.
                ad = appOpenAd;
                initialized = true;
                tierIndex = 1;
                loadTime = DateTime.UtcNow;
            }));
        }

        public bool ShowAppOpenAd(UnityAction appOpenAdClosed)
        {
            if (!IsAppOpenAdAvailable())
            {
                return false;
            }

            if (isShowingAd)
            {
                return false;
            }

            ad.OnAdDidDismissFullScreenContent += HandleAdDidDismissFullScreenContent;
            ad.OnAdFailedToPresentFullScreenContent += HandleAdFailedToPresentFullScreenContent;
            ad.OnAdDidPresentFullScreenContent += HandleAdDidPresentFullScreenContent;
            ad.OnAdDidRecordImpression += HandleAdDidRecordImpression;
            ad.OnPaidEvent += HandlePaidEvent;
            this.appOpenAdClosed = appOpenAdClosed;

            ad.Show();
            Debug.Log("=== Show AOA ===");
            return true;
        }

        private void HandleAdDidDismissFullScreenContent(object sender, EventArgs args)
        {
            Debug.Log("Closed app open ad");
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            ad = null;
            isShowingAd = false;

            GameUtils.RaiseMessage(new GameMessages.AppOpenAdMessage {AppOpenAdState = AppOpenAdState.CLOSED});
            this.appOpenAdClosed?.Invoke();
            LoadAppOpenAd();
        }

        private void HandleAdFailedToPresentFullScreenContent(object sender, AdErrorEventArgs args)
        {
            Debug.LogFormat("Failed to present the ad (reason: {0})", args.AdError.GetMessage());
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            ad = null;
            LoadAppOpenAd();
        }

        private void HandleAdDidPresentFullScreenContent(object sender, EventArgs args)
        {
            Debug.Log("Displayed app open ad");
            isShowingAd = true;
        }

        private void HandleAdDidRecordImpression(object sender, EventArgs args)
        {
            Debug.Log("Recorded ad impression");

            AnalyticsManager.LogEventClickAOAAds();
        }

        private void HandlePaidEvent(object sender, AdValueEventArgs args)
        {
            Debug.LogFormat("Received paid event. (currency: {0}, value: {1}",
                args.AdValue.CurrencyCode, args.AdValue.Value);
        }
    }
}
#endif