using System.Collections.Generic;
using UnityEngine.Events;

//  ---------------------------------------------
//  Author:     DatDz <steven@atsoft.io> 
//  Copyright (c) 2022 AT Soft
// ----------------------------------------------
namespace ATSoft.Ads
{
    public interface ICustomAds
    {
        void InitializeAds(List<AdvertiserSettings> PlatformSettings, bool enableTestAd, bool enableDebugger);
        bool IsRewardVideoAvailable();
        bool ShowRewardVideo(UnityAction<bool> CompleteMethod, string Placement);
        bool ShowRewardVideo(UnityAction<bool,string> CompleteMethod, string Placement);
        bool IsInterstitialAvailable();
        bool ShowInterstitial(UnityAction InterstitialClosed, string Placement, bool autoDisableLoading);
        bool ShowInterstitial(UnityAction<string> InterstitialClosed, string Placement, bool autoDisableLoading);
        bool IsBannerAvailable();
        void ShowBanner(BannerPosition position, BannerType bannerType, UnityAction<bool, BannerPosition, BannerType> DisplayResult);
        void HideBanner();
        
    }
    
    public enum BannerPosition
    {
        TOP,
        BOTTOM
    }

    public enum BannerType
    {
        Banner,
        SmartBanner,
        /// <summary>
        /// Only works for Admob
        /// </summary>
        Adaptive
    }
}