using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using GoogleMobileAds.Api;
using UniRx;

//  ---------------------------------------------
//  Author:     DatDz <steven@atsoft.io> 
//  Copyright (c) 2022 AT Soft
// ----------------------------------------------
#if AD_TYPE_ADMOB
namespace ATSoft.Ads
{

    public class CustomAdmob : BaseAds, ICustomAds
    {
        private InterstitialAd interstitial;
        private BannerView banner;
        private RewardedAd rewardedVideo;
        
        private bool bannerLoaded;
        
        private const float reloadTime = 5;
        private bool triggerCompleteMethod;
        private bool triggerRewardedVideoCallback;
        private bool triggerInterstitialCallback;
        private bool interstitialFailedToLoad;
        private bool rewardedVideoFailedToLoad;
        
#if UNITY_ANDROID
        private string ID_TEST_BANNER = "ca-app-pub-3940256099942544/6300978111";
        private string ID_TEST_INTER = "ca-app-pub-3940256099942544/1033173712";
        private string ID_TEST_REWARD = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IOS
        private string ID_TEST_BANNER = "ca-app-pub-3940256099942544/6300978111";
        private string ID_TEST_INTER = "ca-app-pub-3940256099942544/1033173712";
        private string ID_TEST_REWARD = "ca-app-pub-3940256099942544/5224354917";
#else
        private string ID_TEST_BANNER = "unexpected_platform";
        private string ID_TEST_INTER = "unexpected_platform";
        private string ID_TEST_REWARD = "unexpected_platform";
#endif

        /// <summary>
        /// Initializing Admob
        /// </summary>
        /// <param name="platformSettings">contains all required settings for this publisher</param>
        public void InitializeAds(List<AdvertiserSettings> platformSettings, bool isTestAd, bool enableDebugger)
        {
            if (initialized == false)
            {
                Debug.Log("Admob Start Initialization");

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
                interstitialId = !isTestAd ? settings.idInterstitial : ID_TEST_INTER;
                bannerId = !isTestAd ? settings.idBanner : ID_TEST_BANNER;
                rewardedVideoId = !isTestAd ? settings.idRewarded :  ID_TEST_REWARD;

                // request target for children
                // RequestConfiguration.Builder requestConfiguration = new RequestConfiguration.Builder();
                // requestConfiguration.SetTagForChildDirectedTreatment(tagFororChildren);
                // requestConfiguration.SetMaxAdContentRating(contentRating);
                // requestConfiguration.SetTagForUnderAgeOfConsent(tagForUnderAge);
                // MobileAds.SetRequestConfiguration(requestConfiguration.build());

                //verify settings
                Debug.Log("Admob Banner ID: " + bannerId);
                Debug.Log("Admob Interstitial ID: " + interstitialId);
                Debug.Log("Admob Rewarded Video ID: " + rewardedVideoId);

                MobileAds.Initialize(InitComplete);

                initialized = true;
            }
        }

        private void InitComplete(InitializationStatus status)
        {
            Debug.Log(this + " Init Complete: ");
            
            MobileAds.SetiOSAppPauseOnBackground(true);
            Dictionary<string, AdapterStatus> adapterState = status.getAdapterStatusMap();
            foreach (var adapter in adapterState)
            {
                Debug.Log(adapter.Key + " " + adapter.Value.InitializationState + " " + adapter.Value.Description);
            }
            
            if (!string.IsNullOrEmpty(bannerId))
            {
                //start loading banner
                ShowBanner();
            }

            if (!string.IsNullOrEmpty(rewardedVideoId))
            {
                //start loading ads
                LoadRewardedVideo();
            }

            if (!string.IsNullOrEmpty(interstitialId))
            {
                LoadInterstitial();
            }
        }

        AdRequest CreateRequest()
        {
            AdRequest.Builder request = new AdRequest.Builder();
            return request.Build();
        }


        #region === INTERSTITIAL ===

        /// <summary>
        /// Check if Admob interstitial is available
        /// </summary>
        /// <returns>true if an interstitial is available</returns>
        public bool IsInterstitialAvailable()
        {
            if (interstitial != null)
            {
                return interstitial.IsLoaded();
            }

            return false;
        }

        /// <summary>
        /// Show Admob interstitial
        /// </summary>
        /// <param name="InterstitialClosed">callback called when user closes interstitial</param>
        public bool ShowInterstitial(UnityAction InterstitialClosed, string Placement, bool autoDisableLoading  = true)
        {
            if (interstitial.IsLoaded())
            {
                OnInterstitialClosed = InterstitialClosed;
                triggerInterstitialCallback = true;
                this.autoDisableLoadingInter = autoDisableLoading;
                LoadingAdsCanvas.Instance.ShowLoading();
                obInter?.Dispose();
                obInter = Observable.Timer(TimeSpan.FromSeconds(0.5f), Scheduler.MainThreadIgnoreTimeScale)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        AppOpenAdManager.Instance.ResumeFromAds = true;
                        interstitial.Show();
                        Time.timeScale = 0; // Pause game
                    }).AddTo(this);
                return true;
            }
            
            InterstitialClosed?.Invoke();
            return false;
        }

        public bool ShowInterstitial(UnityAction<string> InterstitialClosed, string Placement, bool autoDisableLoading = true)
        {
            if (interstitial.IsLoaded())
            {
                OnInterstitialClosedWithAdvertiser = InterstitialClosed;
                triggerInterstitialCallback = true;
                this.autoDisableLoadingInter = autoDisableLoading;
                LoadingAdsCanvas.Instance.ShowLoading();
                obInter?.Dispose();
                obInter = Observable.Timer(TimeSpan.FromSeconds(0.5f), Scheduler.MainThreadIgnoreTimeScale)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        AppOpenAdManager.Instance.ResumeFromAds = true;
                        interstitial.Show();
                        Time.timeScale = 0; // Pause game
                    }).AddTo(this);
                return true;
            }
            
            InterstitialClosed?.Invoke("");
            return false;
        }

        /// <summary>
        /// Loads an Admob interstitial
        /// </summary>
        private void LoadInterstitial()
        {
            Debug.Log("Admob Start Loading Interstitial");

            if (interstitial != null)
            {
                interstitial.OnAdLoaded -= InterstitialLoaded;
                interstitial.OnAdClosed -= InterstitialClosed;
                interstitial.OnAdFailedToLoad -= InterstitialFailed;
                interstitial.OnAdFailedToShow -= InterstitialFailedToShow;
                interstitial.OnPaidEvent -= InterstitialPaidEvent;
                interstitial.Destroy();
            }

            //setup and load interstitial
            interstitial = new InterstitialAd(interstitialId);
            interstitial.OnAdLoaded += InterstitialLoaded;
            interstitial.OnAdClosed += InterstitialClosed;
            interstitial.OnAdFailedToLoad += InterstitialFailed;
            interstitial.OnAdFailedToShow += InterstitialFailedToShow;
            interstitial.OnPaidEvent += InterstitialPaidEvent;

            interstitial.LoadAd(CreateRequest());
        }

        /// <summary>
        /// Admob specific event triggered after an interstitial was loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InterstitialLoaded(object sender, EventArgs e)
        {
            Debug.Log("Admob Interstitial Loaded");

            currentRetryInterstitial = 0;
            OnInterstitialAvailable();
        }


        /// <summary>
        /// Admob specific event triggered after an interstitial was closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InterstitialClosed(object sender, EventArgs e)
        {
            Debug.Log("Admob Reload Interstitial");

            Time.timeScale = 1; // Resume game

            //reload interstitial
            LoadInterstitial();

            //trigger complete event
            StartCoroutine(CompleteMethodInterstitial());
        }

        /// <summary>
        ///  Because Admob has some problems when used in multi threading applications with Unity a frame needs to be skipped before returning to application
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
                OnInterstitialClosedWithAdvertiser(SupportedAdvertisers.Admob.ToString());
                OnInterstitialClosedWithAdvertiser = null;
            }
            
            if (autoDisableLoadingInter)
            {
                LoadingAdsCanvas.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Admob specific event triggered if an interstitial failed to load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InterstitialFailed(object sender, AdFailedToLoadEventArgs e)
        {
            LoadAdError loadAdError = e.LoadAdError;

            // Gets the domain from which the error came.
            //string domain = loadAdError.GetDomain();

            // Gets the error code. See
            // https://developers.google.com/android/reference/com/google/android/gms/ads/AdRequest
            // and https://developers.google.com/admob/ios/api/reference/Enums/GADErrorCode
            // for a list of possible codes.
            //int code = loadAdError.GetCode();

            // Gets an error message.
            // For example "Account not approved yet". See
            // https://support.google.com/admob/answer/9905175 for explanations of
            // common errors.
            //string message = loadAdError.GetMessage();

            // Gets the cause of the error, if available.
            //AdError underlyingError = loadAdError.GetCause();

            // All of this information is available via the error's toString() method.
            Debug.Log("Load error string: " + loadAdError.ToString());

            // Get response information, which may include results of mediation requests.
            ResponseInfo responseInfo = loadAdError.GetResponseInfo();
            Debug.Log("Response info: " + responseInfo.ToString());

            //try again to load a rewarded video
            if (currentRetryInterstitial < maxRetryCount)
            {
                currentRetryInterstitial++;
                Debug.Log("Admob RETRY " + currentRetryInterstitial);
                interstitialFailedToLoad = true;
            }
        }
        
        /// <summary>
        /// Admob specific event triggered if an interstitial failed to show
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InterstitialFailedToShow(object sender, AdErrorEventArgs e)
        {
            AdError adError = e.AdError;

            // Gets the domain from which the error came.
            //string domain = loadAdError.GetDomain();

            // Gets the error code. See
            // https://developers.google.com/android/reference/com/google/android/gms/ads/AdRequest
            // and https://developers.google.com/admob/ios/api/reference/Enums/GADErrorCode
            // for a list of possible codes.
            //int code = loadAdError.GetCode();

            // Gets an error message.
            // For example "Account not approved yet". See
            // https://support.google.com/admob/answer/9905175 for explanations of
            // common errors.
            //string message = loadAdError.GetMessage();

            // Gets the cause of the error, if available.
            //AdError underlyingError = loadAdError.GetCause();

            // All of this information is available via the error's toString() method.
            Debug.Log("Load error string: " + adError);

            Time.timeScale = 1; // Resume game
            LoadingAdsCanvas.Instance.HideLoading();

            OnInterstitialFailedToShow(adError.GetMessage());
            
            //try again to load a rewarded video
            if (currentRetryInterstitial < maxRetryCount)
            {
                currentRetryInterstitial++;
                Debug.Log("Admob RETRY " + currentRetryInterstitial);
                interstitialFailedToLoad = true;
            }
        }

        private void InterstitialPaidEvent(object sender, AdValueEventArgs e)
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
        /// Loads an Admob rewarded video
        /// </summary>
        private void LoadRewardedVideo()
        {
            Debug.Log("Admob Start Loading Rewarded Video");

            if (rewardedVideo != null)
            {
                rewardedVideo.OnAdLoaded -= RewardedVideoLoaded;
                rewardedVideo.OnUserEarnedReward -= RewardedVideoWatched;
                rewardedVideo.OnAdFailedToLoad -= RewardedVideoFailed;
                rewardedVideo.OnAdClosed -= OnAdClosed;
                rewardedVideo.OnAdOpening -= OnAdOpening;
                rewardedVideo.OnAdFailedToShow -= OnAdFailedToShow;
            }

            rewardedVideo = new RewardedAd(rewardedVideoId);
            rewardedVideo.OnAdLoaded += RewardedVideoLoaded;
            rewardedVideo.OnUserEarnedReward += RewardedVideoWatched;
            rewardedVideo.OnAdFailedToLoad += RewardedVideoFailed;
            rewardedVideo.OnAdClosed += OnAdClosed;
            rewardedVideo.OnAdOpening += OnAdOpening;
            rewardedVideo.OnAdFailedToShow += OnAdFailedToShow;

            rewardedVideo.LoadAd(CreateRequest());
        }

        /// <summary>
        /// Check if Admob rewarded video is available
        /// </summary>
        /// <returns>true if a rewarded video is available</returns>
        public bool IsRewardVideoAvailable()
        {
            if (rewardedVideo != null)
            {
                return rewardedVideo.IsLoaded();
            }

            return false;
        }

        public bool ShowRewardVideo(UnityAction<bool> CompleteMethod, string Placement)
        {
            if (IsRewardVideoAvailable())
            {
                OnCompleteMethod = CompleteMethod;
                triggerCompleteMethod = true;
                triggerRewardedVideoCallback = true;
                AppOpenAdManager.Instance.ResumeFromAds = true;
                Time.timeScale = 0; // Pause game
                rewardedVideo.Show();
                return true;
            }

            ShowMessage("Ads not ready yet!");
            return false;
        }

        public bool ShowRewardVideo(UnityAction<bool, string> CompleteMethod, string Placement)
        {
            if (IsRewardVideoAvailable())
            {
                OnCompleteMethodWithAdvertiser = CompleteMethod;
                triggerCompleteMethod = true;
                triggerRewardedVideoCallback = true;
                AppOpenAdManager.Instance.ResumeFromAds = true;
                Time.timeScale = 0; // Pause game
                rewardedVideo.Show();
                return true;
            }

            ShowMessage("Ads not ready yet!");
            return false;
        }

        private void OnAdFailedToShow(object sender, AdErrorEventArgs e)
        {
            Debug.LogError("Admob Rewarded Video OnAdFailedToShow " + e.AdError.GetMessage());
            
            Time.timeScale = 1; // Resume game
            AppOpenAdManager.Instance.ResumeFromAds = false;
            
            OnRewardVideoFailedToShow(e.AdError.GetMessage());
        }

        private void OnAdOpening(object sender, EventArgs e)
        {
            Debug.Log("Admob Rewarded Video OnAdOpening ");
        }


        /// <summary>
        /// Admob specific event triggered when a rewarded video was skipped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAdClosed(object sender, EventArgs e)
        {
            Debug.Log("Admob Rewarded Video OnAdClosed");

            Time.timeScale = 1; // Resume game

            //reload
            LoadRewardedVideo();

#if !UNITY_ANDROID || UNITY_EDITOR
            //if complete method was not already triggered, trigger complete method with ad skipped parameter
            if (triggerCompleteMethod == true)
            {
                StartCoroutine(CompleteMethodRewardedVideo(false));
            }
#endif
        }


        /// <summary>
        /// Because Admob has some problems when used in multi-threading applications with Unity a frame needs to be skipped before returning to application
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
                OnCompleteMethodWithAdvertiser(val, SupportedAdvertisers.Admob.ToString());
                OnCompleteMethodWithAdvertiser = null;
            }
            
            yield return new WaitForSeconds(1.0f);
            
            AppOpenAdManager.Instance.ResumeFromAds = false;
        }


        /// <summary>
        /// Admob specific event triggered if a rewarded video failed to load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RewardedVideoFailed(object sender, AdFailedToLoadEventArgs args)
        {
            LoadAdError loadAdError = args.LoadAdError;

            // Gets the domain from which the error came.
            //string domain = loadAdError.GetDomain();

            // Gets the error code. See
            // https://developers.google.com/android/reference/com/google/android/gms/ads/AdRequest
            // and https://developers.google.com/admob/ios/api/reference/Enums/GADErrorCode
            // for a list of possible codes.
            //int code = loadAdError.GetCode();

            // Gets an error message.
            // For example "Account not approved yet". See
            // https://support.google.com/admob/answer/9905175 for explanations of
            // common errors.
            //string message = loadAdError.GetMessage();

            // Gets the cause of the error, if available.
            //AdError underlyingError = loadAdError.GetCause();

            // All of this information is available via the error's toString() method.
            Debug.Log("Admob Rewarded Video -> Load error string: " + loadAdError.ToString());

            // Get response information, which may include results of mediation requests.
            ResponseInfo responseInfo = loadAdError.GetResponseInfo();
            Debug.Log("Admob Rewarded Video -> Response info: " + responseInfo.ToString());

            //try again to load a rewarded video
            if (currentRetryRewardedVideo < maxRetryCount)
            {
                currentRetryRewardedVideo++;
                Debug.Log("Admob Rewarded Video RETRY " + currentRetryRewardedVideo);
                rewardedVideoFailedToLoad = true;
            }
        }


        /// <summary>
        /// Admob specific event triggered after a rewarded video was fully watched
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RewardedVideoWatched(object sender, Reward e)
        {
            Debug.Log($"Admob Rewarded Video Watched: reward {e.Amount}");

            Time.timeScale = 1; // Resume game

            triggerCompleteMethod = false;
            
#if !UNITY_ANDROID || UNITY_EDITOR
            StartCoroutine(CompleteMethodRewardedVideo(true));
#endif
        }


        /// <summary>
        /// Admob specific event triggered after a rewarded video is loaded and ready to be watched
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RewardedVideoLoaded(object sender, EventArgs e)
        {
            Debug.Log("Admob Rewarded Video Loaded");
            currentRetryRewardedVideo = 0;
            
            OnRewardVideoAvailable();
        }

        #endregion

        #region === BANNER ===

        /// <summary>
        /// Check if Admob banner is available
        /// </summary>
        /// <returns>true if a banner is available</returns>
        public bool IsBannerAvailable()
        {
            return true;
        }

        /// <summary>
        /// Loads an Admob banner
        /// </summary>
        /// <param name="position">display position</param>
        /// <param name="bannerType">can be normal banner or smart banner</param>
        private void LoadBanner(BannerPosition position = BannerPosition.BOTTOM, BannerType bannerType = BannerType.SmartBanner)
        {
            Debug.Log("Admob  Start Loading Banner");
            
            //setup banner
            if (banner != null)
            {
                banner.OnAdLoaded -= BannerLoadSucces;
                banner.OnAdFailedToLoad -= BannerLoadFailed;
                banner.Destroy();
            }

            this.bannerPosition = position;
            this.bannerType = bannerType;

            switch (position)
            {
                case BannerPosition.BOTTOM:
                    if (bannerType == BannerType.SmartBanner)
                    {
                        banner = new BannerView(bannerId, AdSize.SmartBanner, AdPosition.Bottom);
                    }
                    else
                    {
                        if (bannerType == BannerType.Adaptive)
                        {
                            AdSize adaptiveSize =
                                AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                            banner = new BannerView(bannerId, adaptiveSize, AdPosition.Bottom);
                        }
                        else
                        {
                            banner = new BannerView(bannerId, AdSize.Banner, AdPosition.Bottom);
                        }
                    }

                    break;
                case BannerPosition.TOP:
                    if (bannerType == BannerType.SmartBanner)
                    {
                        banner = new BannerView(bannerId, AdSize.SmartBanner, AdPosition.Top);
                    }
                    else
                    {
                        if (bannerType == BannerType.Adaptive)
                        {
                            AdSize adaptiveSize =
                                AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                            banner = new BannerView(bannerId, adaptiveSize, AdPosition.Top);
                        }
                        else
                        {
                            banner = new BannerView(bannerId, AdSize.Banner, AdPosition.Top);
                        }
                    }

                    break;
            }

            //add listeners
            if (banner != null)
            {
                banner.OnAdLoaded += BannerLoadSucces;
                banner.OnAdFailedToLoad += BannerLoadFailed;

                //request banner

                banner.LoadAd(CreateRequest());
            }
        }

        /// <summary>
        /// Show Admob banner
        /// </summary>
        /// <param name="position"> can be TOP or BOTTOM</param>
        ///  /// <param name="bannerType"> can be Banner or SmartBanner</param>
        public void ShowBanner(BannerPosition position = BannerPosition.BOTTOM, BannerType bannerType = BannerType.SmartBanner,
            UnityAction<bool, BannerPosition, BannerType> DisplayResult = null)
        {
            bannerLoaded = false;
            this.DisplayResult = DisplayResult;
            if (banner != null)
            {
                if (this.bannerPosition == position && this.bannerType == bannerType)
                {
                    Debug.Log("Admob Show banner");

                    bannerLoaded = true;
                    banner.Show();
                    if (this.DisplayResult != null)
                    {
                        this.DisplayResult(true, position, bannerType);
                        this.DisplayResult = null;
                    }
                }
                else
                {
                    LoadBanner(position, bannerType);
                }
            }
            else
            {
                LoadBanner(position, bannerType);
            }
        }

        /// <summary>
        /// Hides Admob banner
        /// </summary>
        public void HideBanner()
        {
            Debug.Log("Admob Hide banner");

            if (banner != null)
            {
                if (bannerLoaded == false)
                {
                    //if banner is not yet loaded -> destroy so it cannot load later in the game
                    if (DisplayResult != null)
                    {
                        DisplayResult(false, bannerPosition, bannerType);
                        DisplayResult = null;
                    }

                    banner.OnAdLoaded -= BannerLoadSucces;
                    banner.OnAdFailedToLoad -= BannerLoadFailed;
                    banner.Destroy();
                }
                else
                {
                    //hide the banner -> will be available later without loading
                    banner.Hide();
                }
            }
        }

        /// <summary>
        /// Admob specific event triggered after banner was loaded 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BannerLoadSucces(object sender, EventArgs e)
        {
            Debug.Log("Admob Banner Loaded");

            bannerLoaded = true;
            if (DisplayResult != null)
            {
                DisplayResult(true, bannerPosition, bannerType);
                DisplayResult = null;
            }
        }


        /// <summary>
        /// Admob specific event triggered after banner failed to load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BannerLoadFailed(object sender, AdFailedToLoadEventArgs e)
        {
            LoadAdError loadAdError = e.LoadAdError;

            // Gets the domain from which the error came.
            //string domain = loadAdError.GetDomain();

            // Gets the error code. See
            // https://developers.google.com/android/reference/com/google/android/gms/ads/AdRequest
            // and https://developers.google.com/admob/ios/api/reference/Enums/GADErrorCode
            // for a list of possible codes.
            //int code = loadAdError.GetCode();

            // Gets an error message.
            // For example "Account not approved yet". See
            // https://support.google.com/admob/answer/9905175 for explanations of
            // common errors.
            //string message = loadAdError.GetMessage();

            // Gets the cause of the error, if available.
            //AdError underlyingError = loadAdError.GetCause();

            // All of this information is available via the error's toString() method.
            Debug.Log("Admob Banner -> Load error string: " + loadAdError.ToString());

            // Get response information, which may include results of mediation requests.
            ResponseInfo responseInfo = loadAdError.GetResponseInfo();
            Debug.Log("Admob Banner -> Response info: " + responseInfo.ToString());

            if (banner != null)
            {
                banner.OnAdLoaded -= BannerLoadSucces;
                banner.OnAdFailedToLoad -= BannerLoadFailed;
                banner.Destroy();
            }

            banner = null;
            bannerLoaded = false;
            if (DisplayResult != null)
            {
                DisplayResult(false, bannerPosition, bannerType);
                DisplayResult = null;
            }
        }

        #endregion

        /// <summary>
        /// Used to delay the admob events for multi-threading applications
        /// </summary>
        private void Update()
        {
            if (interstitialFailedToLoad)
            {
                interstitialFailedToLoad = false;
                Invoke("LoadInterstitial", reloadTime);
            }

            if (rewardedVideoFailedToLoad)
            {
                rewardedVideoFailedToLoad = false;
                Invoke("LoadRewardedVideo", reloadTime);
            }
        }

        /// <summary>
        /// Method triggered by Unity Engine when application is in focus.
        /// Because Admob uses multi-threading, there are some errors when you create coroutine outside the main thread so we want to make sure the app is on main thread.
        /// </summary>
        /// <param name="focus"></param>
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

                if (IsRewardVideoAvailable() == false)
                {
                    if (currentRetryRewardedVideo == maxRetryCount)
                    {
                        LoadRewardedVideo();
                    }
                }

                if (triggerRewardedVideoCallback)
                {
#if UNITY_ANDROID
                    triggerRewardedVideoCallback = false;
                    StartCoroutine(triggerCompleteMethod == true
                        ? CompleteMethodRewardedVideo(false)
                        : CompleteMethodRewardedVideo(true));
#endif
                }

                if (triggerInterstitialCallback)
                {
#if UNITY_ANDROID
                    triggerInterstitialCallback = false;
                    StartCoroutine(CompleteMethodInterstitial());
#endif
                }
            }
        }
    }
}
#endif