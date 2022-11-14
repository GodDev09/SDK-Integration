using System.Collections.Generic;
using UnityEngine;

//  ---------------------------------------------
//  Author:     DatDz <steven@atsoft.io> 
//  Copyright (c) 2022 AT Soft
// ----------------------------------------------
namespace ATSoft.Ads
{
    [System.Serializable]
    public class Advertiser
    {
        public ICustomAds advertiserScript;
        public SupportedAdvertisers advertiser;
        public List<AdvertiserSettings> advertiserSettings;

        public Advertiser(ICustomAds _advertiserScript, SupportedAdvertisers _advertiser, List<AdvertiserSettings> _advertiserSettings)
        {
            this.advertiserScript = _advertiserScript;
            this.advertiser = _advertiser;
            this.advertiserSettings = _advertiserSettings;
        }
    }
    
    public enum SupportedAdvertisers
    {
        None = 0,
        Admob = 1,
        AppLovin = 2,
        IronSource = 3
    }
    
    public enum SupportedPlatforms
    {
        Android,
        IOS,
        Windows
    }

    [System.Serializable]
    public class AdvertiserSettings
    {
        public SupportedPlatforms platform;
        [Tooltip("~ SDK key in MAX Applovin")]public string appId;
        public string idBanner;
        public string idInterstitial;
        public string idRewarded;

        public AdvertiserSettings(SupportedPlatforms platform, string appId, string idBanner, string idInterstitial, string idRewarded)
        {
            this.platform = platform;
            this.idBanner = idBanner;
            this.idInterstitial = idInterstitial;
            this.idRewarded = idRewarded;
        }
    }
}