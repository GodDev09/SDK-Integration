using System;
using System.Collections.Generic;
using System.Linq;
using GoogleMobileAds.Api;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

//  ---------------------------------------------
//  Author:     DatDz <steven@atsoft.io> 
//              AVT
//  Copyright (c) 2022 AT Soft
// ----------------------------------------------
namespace ATSoft.Ads
{
    public class AppOpenAdManager : Singleton<AppOpenAdManager>, IService
    {
        [SerializeField] private bool enableAds;
        [SerializeField] private bool isTestAds;

        [OnValueChanged("ChangeTypeAdvertiserHandle")] [SerializeField]
        private SupportedAdvertisers typeAdvertiser = SupportedAdvertisers.None;

        [Header("=== Settings Ads ===")] 
        public List<AppOpenAdvertiserSettings> appOpenAdvertiserSettings;
        
        [HideInInspector] public bool ResumeFromAds = false;
        [HideInInspector] public bool initialized;

        private AppOpenAdvertiser advertiser;
        private bool canRepeatLoad = false;
        private int countLoadAOA;
        private float timeOut = 3f;
        private float timer;


#if AOA_TYPE_ADMOB
        private void Update()
        {
            if (!enableAds) return;

            if (canRepeatLoad)
            {
                if (countLoadAOA >= 3) return;
                if (timer >= timeOut)
                {
                    Debug.Log("=== LoadAOAIfNotAvailable ===");
                    LoadAOAIfNotAvailable();
                    countLoadAOA++;
                    timer = 0;
                }
                else
                {
                    timer += Time.deltaTime;
                }
            }
        }
#endif

        public void Initialize()
        {
            Debug.Log("Initialize " + (typeof(AppOpenAdManager)));

            if (!enableAds) return;

            if (initialized == false)
            {
#if UNITY_ANDROID
                AppOpenAdvertiserSettings settings =
                    appOpenAdvertiserSettings.FirstOrDefault(cond => cond.platform == SupportedPlatforms.Android);
#elif UNITY_IOS
                AppOpenAdvertiserSettings settings =
                    appOpenAdvertiserSettings.FirstOrDefault(cond => cond.platform == SupportedPlatforms.IOS);
#endif
                switch (typeAdvertiser)
                {
                    case SupportedAdvertisers.Admob:
#if AOA_TYPE_ADMOB

                        var admobComp = gameObject.AddComponent<CustomAppOpenAdmob>();
                        advertiser = new AppOpenAdvertiser(admobComp, settings);
#endif
                        break;
                    case SupportedAdvertisers.AppLovin:
#if AOA_TYPE_APPLOVIN
                        var appLovinComp = gameObject.AddComponent<CustomAppOpenAppLovin>();
                        advertiser = new AppOpenAdvertiser(appLovinComp, settings);
#endif
                        break;
                    default:
                        Debug.LogWarning("AT Soft - Failed to Set up AOA Ads - UnKnow type Advertiser");
                        break;
                }

                advertiser?.advertiserScript.InitializeAds(settings, isTestAds);
                initialized = true;
                canRepeatLoad = true;
            }
        }

        private ICustomAppOpenAds GetAppOpenAdvertiser()
        {
            return advertiser?.advertiserScript;
        }

        private bool IsAppOpenAdAvailable()
        {
            return advertiser.advertiserScript.IsAppOpenAdAvailable();
        }

        private void LoadAOAIfNotAvailable()
        {
            Debug.Log("Call LoadAOAIfNotAvailable");
            if (!enableAds || !initialized) return;

            ICustomAppOpenAds selectedAdvertiser = GetAppOpenAdvertiser();
            selectedAdvertiser?.LoadAppOpenAd();
        }

        public bool ShowAppOpenAd(UnityAction appOpenAdClosed = null)
        {
            if (!enableAds)
            {
                appOpenAdClosed?.Invoke();
                return true;
            }
            
            if(!initialized)
            {
                return false;
            }

            ICustomAppOpenAds selectedAdvertiser = GetAppOpenAdvertiser();
            if (selectedAdvertiser != null)
            {
                Debug.Log("AOA loaded from " + selectedAdvertiser);
                
                var showSuccess = selectedAdvertiser.ShowAppOpenAd(appOpenAdClosed);
                if (showSuccess) canRepeatLoad = false;
                return showSuccess;
            }
            
            return false;
        }

        private bool didShowAoALoadingTime;
        private float lastCheck = 0;

        public void ShowAOAIfLoadedInLoadingTime()
        {
            if (didShowAoALoadingTime || lastCheck + 0.2f > Time.time)
                return;

            lastCheck = Time.time;
            didShowAoALoadingTime = ShowAppOpenAd();
        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause && !ResumeFromAds && enableAds)
            {
                ShowAppOpenAd();
            }
        }

#if UNITY_EDITOR
        private void ChangeTypeAdvertiserHandle()
        {
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);


            switch (typeAdvertiser)
            {
                case SupportedAdvertisers.None:
                    AddPreprocessorDirectiveAdType("", buildTargetGroup);
                    break;
                case SupportedAdvertisers.Admob:
                    AddPreprocessorDirectiveAdType("AOA_TYPE_ADMOB", buildTargetGroup);
                    break;
                case SupportedAdvertisers.AppLovin:
                    AddPreprocessorDirectiveAdType("AOA_TYPE_APPLOVIN", buildTargetGroup);
                    isTestAds = false; //Applovin not working with Test Ad
                    break;
                default:
                    AddPreprocessorDirectiveAdType("", buildTargetGroup);
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddPreprocessorDirectiveAdType(string directive, BuildTargetGroup target)
        {
            string textToWrite = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);

            switch (directive)
            {
                case "AOA_TYPE_ADMOB":
                    textToWrite = textToWrite.Replace("AOA_TYPE_APPLOVIN", "");
                    break;
                case "AOA_TYPE_APPLOVIN":
                    textToWrite = textToWrite.Replace("AOA_TYPE_ADMOB", "");
                    break;
                default:
                    textToWrite = textToWrite.Replace("AOA_TYPE_ADMOB", "").Replace("AOA_TYPE_APPLOVIN", "");
                    break;
            }

            if (!textToWrite.Contains(directive))
            {
                if (textToWrite == "")
                {
                    textToWrite += directive;
                }
                else
                {
                    textToWrite += ";" + directive;
                }
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, textToWrite);
        }
#endif
    }
}

public enum AppOpenAdState
{
    NONE,
    OPENING,
    CLOSED
}