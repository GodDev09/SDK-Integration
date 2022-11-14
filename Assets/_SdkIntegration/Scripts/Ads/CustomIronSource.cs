using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

//  ---------------------------------------------
//  Author:     DatDz <steven@atsoft.io> 
//  Copyright (c) 2022 AT Soft
// ----------------------------------------------

#if AD_TYPE_IS
namespace ATSoft.Ads
{
    public class CustomIronSource : BaseAds, ICustomAds
    {
        private bool bannerLoaded;
        
        private const float reloadTime = 2;
        private bool rewardedVideoCompleted;
        private bool directedForChildren;
        private bool enableDebugger;
        
#if UNITY_ANDROID
        private string ID_TEST_APP = "e6023921";
        private string ID_TEST_BANNER = "4562106";
        private string ID_TEST_INTER = "3711480";
        private string ID_TEST_REWARD = "3711476";
#elif UNITY_IOS
        private string ID_TEST_APP = "17455d1ed";
        private string ID_TEST_BANNER = "11068123";
        private string ID_TEST_INTER = "11068107";
        private string ID_TEST_REWARD = "11068103";
#else
        private string ID_TEST_APP = "unexpected_platform";
        private string ID_TEST_BANNER = "unexpected_platform";
        private string ID_TEST_INTER = "unexpected_platform";
        private string ID_TEST_REWARD = "unexpected_platform";
#endif

        /// <summary>
        /// Initializing IronSource
        /// </summary>
        /// <param name="platformSettings">contains all required settings for this publisher</param>
        public void InitializeAds(List<AdvertiserSettings> platformSettings, bool isTestAd, bool enableDebugger)
        {
            if (initialized == false)
            {
                Debug.Log("IronSource Start Initialization");
                
                //get settings
#if UNITY_ANDROID
                AdvertiserSettings settings =
                    platformSettings.First(cond => cond.platform == SupportedPlatforms.Android);
#elif UNITY_IOS
                AdvertiserSettings settings = platformSettings.First(cond => cond.platform == SupportedPlatforms.IOS);
#else
                AdvertiserSettings settings = new AdvertiserSettings(SupportedPlatforms.Windows,"", "", "","");
#endif
                //apply settings
                var appId = !isTestAd ? settings.appId : ID_TEST_APP;
                interstitialId = !isTestAd ? settings.idInterstitial : ID_TEST_INTER;
                bannerId = !isTestAd ? settings.idBanner : ID_TEST_BANNER;
                rewardedVideoId = !isTestAd ? settings.idRewarded : ID_TEST_REWARD;

                //verify settings
                Debug.Log("IronSource plugin Version: " + IronSource.pluginVersion());
                Debug.Log("IronSource Banner ID: " + bannerId);
                Debug.Log("IronSource Interstitial ID: " + interstitialId);
                Debug.Log("IronSource Rewarded Video ID: " + rewardedVideoId);
                
                //Dynamic config example
                IronSourceConfig.Instance.setClientSideCallbacks (true);

                string id = IronSource.Agent.getAdvertiserId ();
                Debug.Log ("unity-script: IronSource.Agent.getAdvertiserId : " + id);
		
                Debug.Log ("unity-script: IronSource.Agent.validateIntegration");
                IronSource.Agent.validateIntegration ();
                
                // if(directedForChildren)
                // {
                //     IronSource.Agent.setMetaData("is_child_directed", "true");
                // }
                // else
                // {
                //     IronSource.Agent.setMetaData("is_child_directed", "false");
                // }

                if (!string.IsNullOrEmpty(bannerId))
                {
                    IronSourceEvents.onBannerAdLoadedEvent += BannerAdLoadedEvent;
                    IronSourceEvents.onBannerAdLoadFailedEvent += BannerAdLoadFailedEvent;
                    IronSourceEvents.onBannerAdClickedEvent += BannerAdClickedEvent;
                    IronSourceEvents.onBannerAdScreenPresentedEvent += BannerAdScreenPresentedEvent;
                    IronSourceEvents.onBannerAdScreenDismissedEvent += BannerAdScreenDismissedEvent;
                    IronSourceEvents.onBannerAdLeftApplicationEvent += BannerAdLeftApplicationEvent;
                    IronSource.Agent.init(appId, IronSourceAdUnits.BANNER);
                }

                if (!string.IsNullOrEmpty(interstitialId))
                {
                    IronSourceEvents.onInterstitialAdReadyEvent += InterstitialAdReadyEvent;
                    IronSourceEvents.onInterstitialAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
                    IronSourceEvents.onInterstitialAdShowSucceededEvent += InterstitialAdShowSucceededEvent;
                    IronSourceEvents.onInterstitialAdShowFailedEvent += InterstitialAdShowFailedEvent;
                    IronSourceEvents.onInterstitialAdClickedEvent += InterstitialAdClickedEvent;
                    IronSourceEvents.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;
                    IronSourceEvents.onInterstitialAdClosedEvent += InterstitialAdClosedEvent;
                    IronSource.Agent.init(appId, IronSourceAdUnits.INTERSTITIAL);
                    LoadInterstitial();
                }

                if (!string.IsNullOrEmpty(rewardedVideoId))
                {
                    IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
                    IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
                    IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
                    IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
                    IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
                    IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;
                    IronSourceEvents.onRewardedVideoAdClickedEvent += RewardedVideoAdClickedEvent; 
                    IronSource.Agent.init(appId, IronSourceAdUnits.REWARDED_VIDEO);
                }

                initialized = true;
                LoadBanner();
                Debug.Log(this + " Init Complete: ");
            }
        }


        #region === INTERSTITIAL ===

        /// <summary>
        /// Check if IronSource interstitial is available
        /// </summary>
        /// <returns>true if an interstitial is available</returns>
        public bool IsInterstitialAvailable()
        {
            return !string.IsNullOrEmpty(interstitialId) && initialized && IronSource.Agent.isInterstitialReady();
        }

        /// <summary>
        /// Show IronSource interstitial
        /// </summary>
        /// <param name="InterstitialClosed">callback called when user closes interstitial</param>
        public bool ShowInterstitial(UnityAction InterstitialClosed, string Placement, bool autoDisableLoading  = true)
        {
            return ShowInterstitial((s) => InterstitialClosed(), Placement, autoDisableLoading);
            // if (IsInterstitialAvailable())
            // {
            //     OnInterstitialClosed = InterstitialClosed;
            //     this.autoDisableLoadingInter = autoDisableLoading;
            //     LoadingAdsCanvas.Instance.ShowLoading();
            //     obInter?.Dispose();
            //     obInter = Observable.Timer(TimeSpan.FromSeconds(0.5f), Scheduler.MainThreadIgnoreTimeScale)
            //         .Take(1)
            //         .Subscribe(_ =>
            //         {
            //             AppOpenAdManager.Instance.ResumeFromAds = true;
            //             IronSource.Agent.showInterstitial(interstitialId);
            //             Time.timeScale = 0; // Pause game
            //         }).AddTo(this);
            //     return true;
            // }
            //
            // InterstitialClosed?.Invoke();
            // return false;
        }

        public bool ShowInterstitial(UnityAction<string> InterstitialClosed, string Placement, bool autoDisableLoading = true)
        {
            if (IsInterstitialAvailable())
            {
                OnInterstitialClosedWithAdvertiser = InterstitialClosed;
                this.autoDisableLoadingInter = autoDisableLoading;
                LoadingAdsCanvas.Instance.ShowLoading();
                obInter?.Dispose();
                obInter = Observable.Timer(TimeSpan.FromSeconds(0.5f), Scheduler.MainThreadIgnoreTimeScale)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        AppOpenAdManager.Instance.ResumeFromAds = true;
                        IronSource.Agent.showInterstitial(interstitialId);
                        Time.timeScale = 0; // Pause game
                    }).AddTo(this);
                return true;
            }
            
            InterstitialClosed?.Invoke("");
            return false;
        }

        /// <summary>
        /// Loads IronSource interstitial
        /// </summary>
        private void LoadInterstitial()
        {
            Debug.Log("IronSource Start Loading Interstitial");

            IronSource.Agent.loadInterstitial();
        }

        /// <summary>
        /// IronSource specific event triggered after an interstitial was loaded
        /// </summary>
        private void InterstitialAdReadyEvent()
        {
            Debug.Log("IronSource Interstitial Loaded");

            currentRetryInterstitial = 0;
            OnInterstitialAvailable();
        }
        
        //IronSource: Invoked right before the Interstitial screen is about to open.
        void InterstitialAdShowSucceededEvent()
        {
            currentRetryInterstitial = 0;
        }

        /// <summary>
        /// IronSource: Invoked when the interstitial ad closed and the user goes back to the application screen.
        /// </summary>
        private void InterstitialAdClosedEvent()
        {
            Debug.Log("IronSource Reload Interstitial");

            Time.timeScale = 1; // Resume game

            //reload interstitial
            LoadInterstitial();

            //trigger complete event
            StartCoroutine(CompleteMethodInterstitial());
        }

        /// <summary>
        ///  Because IronSource has some problems when used in multi threading applications with Unity a frame needs to be skipped before returning to application
        /// </summary>
        /// <returns></returns>
        private IEnumerator CompleteMethodInterstitial()
        {
            yield return new WaitForSeconds(1f);
            
            AppOpenAdManager.Instance.ResumeFromAds = false;

            if (OnInterstitialClosed != null)
            {
                OnInterstitialClosed();
                OnInterstitialClosed = null;
            }

            if (OnInterstitialClosedWithAdvertiser != null)
            {
                OnInterstitialClosedWithAdvertiser(SupportedAdvertisers.IronSource.ToString());
                OnInterstitialClosedWithAdvertiser = null;
            }
            
            if (autoDisableLoadingInter)
            {
                LoadingAdsCanvas.Instance.HideLoading();
            }
        }

        /// <summary>
        /// IronSource: Invoked when the initialization process has failed.
        /// </summary>
        /// <param name="error"></param>
        private void InterstitialAdLoadFailedEvent(IronSourceError error)
        {
            Debug.Log(this + " Interstitial Failed To Load " + error.getCode());

            //try again to load a rewarded video
            if (currentRetryInterstitial < maxRetryCount)
            {
                currentRetryInterstitial++;
                Debug.Log("IronSource RETRY " + currentRetryInterstitial);
                StartCoroutine(ReloadInterstitial(reloadTime));
            }
        }
        
        /// <summary>
        /// IronSource specific event triggered if an interstitial failed to show
        /// </summary>
        /// <param name="error"></param>
        private void InterstitialAdShowFailedEvent(IronSourceError error)
        {
            Time.timeScale = 1; // Resume game
            LoadingAdsCanvas.Instance.HideLoading();
            
            OnInterstitialFailedToShow(error.getCode().ToString());
            
            //try again to load a rewarded video
            if (currentRetryInterstitial < maxRetryCount)
            {
                currentRetryInterstitial++;
                Debug.Log("IronSource RETRY " + currentRetryInterstitial);
                StartCoroutine(ReloadInterstitial(reloadTime));
            }
        }
        
        //IronSource: Invoked when end user clicked on the interstitial ad
        void InterstitialAdClickedEvent()
        {
        }
        
        //IronSource: Invoked when the Interstitial Ad Unit has opened
        void InterstitialAdOpenedEvent()
        {
        }

        private IEnumerator ReloadInterstitial(float reloadTime)
        {
            yield return new WaitForSeconds(reloadTime);
            LoadInterstitial();
        }

        #endregion

        #region === REWARDED VIDEO ===

        /// <summary>
        /// Check if IronSource rewarded video is available
        /// </summary>
        /// <returns>true if a rewarded video is available</returns>
        public bool IsRewardVideoAvailable()
        {
            return !string.IsNullOrEmpty(rewardedVideoId) && initialized && IronSource.Agent.isRewardedVideoAvailable();
        }

        public bool ShowRewardVideo(UnityAction<bool> CompleteMethod, string Placement)
        {
            return ShowRewardVideo((b, s) => CompleteMethod(b), Placement);
            // if (IsRewardVideoAvailable())
            // {
            //     OnCompleteMethod = CompleteMethod;
            //     AppOpenAdManager.Instance.ResumeFromAds = true;
            //     Time.timeScale = 0; // Pause game
            //     IronSource.Agent.showRewardedVideo(rewardedVideoId);
            //     return true;
            // }
            //
            // ShowMessage("Ads not ready yet!");
            // return false;
        }

        public bool ShowRewardVideo(UnityAction<bool, string> CompleteMethod, string Placement)
        {
            if (IsRewardVideoAvailable())
            {
                OnCompleteMethodWithAdvertiser = CompleteMethod;
                AppOpenAdManager.Instance.ResumeFromAds = true;
                Time.timeScale = 0; // Pause game
                IronSource.Agent.showRewardedVideo(rewardedVideoId);
                return true;
            }

            ShowMessage("Ads not ready yet!");
            return false;
        }

        //IronSource: Invoked when the Rewarded Video failed to show
        private void RewardedVideoAdShowFailedEvent(IronSourceError error)
        {
            Debug.LogError("IronSource Rewarded Video OnAdFailedToShow " + error.getCode());
            
            Time.timeScale = 1; // Resume game
            AppOpenAdManager.Instance.ResumeFromAds = false;
            
            OnRewardVideoFailedToShow(error.getCode().ToString());
        }

        //IronSource: Invoked when the video ad starts playing.
        private void RewardedVideoAdStartedEvent()
        {
            Debug.Log("IronSource Rewarded Video OnAdOpening");
        }
        
        //IronSource: Invoked when the RewardedVideo ad view has opened.
        void RewardedVideoAdOpenedEvent()
        {
        }

        /// <summary>
        ///IronSource: Invoked when the RewardedVideo ad view is about to be closed. Your activity will now regain its focus.
        /// </summary>
        private void RewardedVideoAdClosedEvent()
        {
            Debug.Log("IronSource Rewarded Video OnAdClosed");

            // Time.timeScale = 1; // Resume game
            //
            // StartCoroutine(CompleteMethodRewardedVideo(false));
        }

        /// <summary>
        /// IronSource: specific event triggered after a rewarded video was fully watched
        /// </summary>
        /// <param name="placement"></param>
        private void RewardedVideoAdRewardedEvent(IronSourcePlacement placement)
        {
            Debug.Log($"IronSource Rewarded Video Watched: reward {placement.getRewardAmount()}");

            Time.timeScale = 1; // Resume game
            
            StartCoroutine(CompleteMethodRewardedVideo(true));
        }
        
        /// <summary>
        /// Because IronSource has some problems when used in multi-threading applications with Unity a frame needs to be skipped before returning to application
        /// </summary>
        /// <returns></returns>
        private IEnumerator CompleteMethodRewardedVideo(bool val)
        {
            yield return null;
            if (OnCompleteMethod != null)
            {
                OnCompleteMethod(val);
                OnCompleteMethod = null;
                OnRewardVideoCompleted();
            }

            if (OnCompleteMethodWithAdvertiser != null)
            {
                OnCompleteMethodWithAdvertiser(val, SupportedAdvertisers.IronSource.ToString());
                OnCompleteMethodWithAdvertiser = null;
            }
            
            yield return new WaitForSeconds(1.0f);
            
            AppOpenAdManager.Instance.ResumeFromAds = false;
        }

        /// <summary>
        /// IronSource: Invoked when there is a change in the ad availability status.
        /// </summary>
        /// <param name="available">value will change to true when rewarded videos are available</param>
        private void RewardedVideoAvailabilityChangedEvent(bool available)
        {
            if (available)
            {
                Debug.Log("IronSource Rewarded Video Loaded");
                //Value will change to true when rewarded videos are available
                OnRewardVideoAvailable();
            }
            else
            {
                //Value will change to false when no videos are available
            }
        }
        
        void RewardedVideoAdClickedEvent (IronSourcePlacement placement)
        {
            Debug.Log ("IronSource Rewarded Video Clicked, name = " + placement.getRewardName());
        }

        #endregion

        #region === BANNER ===

        /// <summary>
        /// Check if IronSource banner is available
        /// </summary>
        /// <returns>true if a banner is available</returns>
        public bool IsBannerAvailable()
        {
            return true;
        }

        /// <summary>
        /// Loads an IronSource banner
        /// </summary>
        /// <param name="position">display position</param>
        /// <param name="bannerType">can be normal banner or smart banner</param>
        private void LoadBanner(BannerPosition position = BannerPosition.BOTTOM, BannerType bannerType = BannerType.SmartBanner)
        {
            Debug.Log("IronSource Start Loading Banner");
            
            if(bannerLoaded) HideBanner();
            
            var bannerPosition = position == BannerPosition.BOTTOM ? IronSourceBannerPosition.BOTTOM : IronSourceBannerPosition.TOP;
            var bannerSize = bannerType == BannerType.Banner ? IronSourceBannerSize.BANNER : IronSourceBannerSize.SMART;
            IronSource.Agent.loadBanner(bannerSize, bannerPosition, bannerId);
        }

        /// <summary>
        /// Show IronSource banner
        /// </summary>
        /// <param name="position"> can be TOP or BOTTOM</param>
        ///  /// <param name="bannerType"> can be Banner or SmartBanner</param>
        public void ShowBanner(BannerPosition position = BannerPosition.BOTTOM, BannerType bannerType = BannerType.SmartBanner,
            UnityAction<bool, BannerPosition, BannerType> DisplayResult = null)
        {
            LoadBanner(position, bannerType);
        }

        /// <summary>
        /// Hides IronSource banner
        /// </summary>
        public void HideBanner()
        {
            Debug.Log("IronSource Hide banner");
            
            if (DisplayResult != null)
            {
                DisplayResult(false, bannerPosition, bannerType);
                DisplayResult = null;
            }
            IronSource.Agent.destroyBanner();
        }

        /// <summary>
        /// Invoked once the banner has loaded
        /// </summary>
        private void BannerAdLoadedEvent()
        {
            Debug.Log("IronSource Banner Loaded");

            bannerLoaded = true;
            if (DisplayResult != null)
            {
                DisplayResult(true, bannerPosition, bannerType);
                DisplayResult = null;
            }
        }

        /// <summary>
        /// IronSource specific event triggered after banner failed to load
        /// </summary>
        /// <param name="error"></param>
        private void BannerAdLoadFailedEvent(IronSourceError error)
        {
            Debug.Log("IronSource Banner -> Load error string: " + error.getCode());
            
            bannerLoaded = false;
            if (DisplayResult != null)
            {
                DisplayResult(false, bannerPosition, bannerType);
                DisplayResult = null;
            }
        }
        
        //Invoked when end user clicks on the banner ad
        void BannerAdClickedEvent()
        {
        }
        
        //Notifies the presentation of a full screen content following user click
        void BannerAdScreenPresentedEvent()
        {
        }

        //Notifies the presented screen has been dismissed
        void BannerAdScreenDismissedEvent()
        {
        }

        //Invoked when the user leaves the app
        void BannerAdLeftApplicationEvent()
        {
        }

        #endregion
        
        private void OnApplicationFocus(bool focus)
        {
            if (focus == true)
            {
                if (IsInterstitialAvailable() == false)
                {
                    if (currentRetryInterstitial == maxRetryCount)
                    {
                        LoadInterstitial();
                    }
                }
            }
        }
        
        private void OnApplicationPause(bool isPaused)
        {
            IronSource.Agent.onApplicationPause(isPaused);
        }
    }
}
#endif