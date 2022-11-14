//  ---------------------------------------------
//  Author:     DatDz <steven@atsoft.io> 
//  Copyright (c) 2022 AT Soft
// ----------------------------------------------

using AppsFlyerSDK;
using UnityEngine;
using UnityEngine.Events;

#if AOA_TYPE_APPLOVIN
namespace ATSoft.Ads
{
    public class CustomAppOpenAppLovin : BaseAppOpenAds, ICustomAppOpenAds
    {
        public void InitializeAds(AppOpenAdvertiserSettings appOpenAdvertiserSettings, bool enableTestAd)
        {
            //apply settings
            this.enableTestAd = enableTestAd;
            appId = appOpenAdvertiserSettings.appId;
            ID_TIER_1 = appOpenAdvertiserSettings.ID_TIER_1;
            ID_TIER_2 = appOpenAdvertiserSettings.ID_TIER_2;
            ID_TIER_3 = appOpenAdvertiserSettings.ID_TIER_3;
            
            MaxSdkCallbacks.OnSdkInitializedEvent += ApplovinInitialized;
            
            //Initialize the SDK
            MaxSdk.SetSdkKey(appId);
            //Set UserId by AppsFlyer
            MaxSdk.SetUserId(AppsFlyer.getAppsFlyerId());
            MaxSdk.InitializeSdk();

            Debug.Log(this + " " + "Start Initialization");
            Debug.Log(this + " SDK key: " + appId);
        }

        public bool IsAppOpenAdAvailable()
        {
            return MaxSdk.IsAppOpenAdReady(ID_TIER_1);
        }

        public void LoadAppOpenAd()
        {
            MaxSdk.LoadAppOpenAd(ID_TIER_1);
        }

        public bool ShowAppOpenAd(UnityAction AppOpenAdClosed)
        {
            Debug.Log("ShowAppOpenAd " + (typeof(CustomAppOpenAppLovin)));
            
            if (enableTestAd)
            {
                appOpenAdClosed?.Invoke();
                return false;
            }

            if (IsAppOpenAdAvailable())
            {
                MaxSdk.ShowAppOpenAd(ID_TIER_1);
                return true;
            }

            LoadAppOpenAd();
            return false;
        }

        private void ApplovinInitialized(MaxSdk.SdkConfiguration obj)
        {
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenDismissedEvent;
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent += OnAppOpenClickedEvent;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAppOpenPaidEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += OnAdDisplayedEvent;
 
            LoadAppOpenAd();
        }
        
        public void OnAppOpenDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            isShowingAd = false;
            GameUtils.RaiseMessage(new GameMessages.AppOpenAdMessage {AppOpenAdState = AppOpenAdState.CLOSED});
            this.appOpenAdClosed?.Invoke();
            MaxSdk.LoadAppOpenAd(ID_TIER_1);
        }
        
        public void OnAppOpenClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Recorded ad impression");
            AnalyticsManager.LogEventClickAOAAds();
            
            MaxSdk.LoadAppOpenAd(ID_TIER_1);
        }
        
        private void OnAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Displayed app open ad");
            isShowingAd = true;
        }
        
        private void OnAppOpenPaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.LogFormat("Received paid event. (currency: {0}, value: {1}",
                adInfo.CreativeIdentifier, adInfo.Revenue);
        }
    }
}
#endif