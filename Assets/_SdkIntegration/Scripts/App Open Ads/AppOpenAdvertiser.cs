
//  ---------------------------------------------
//  Author:     DatDz <steven@atsoft.io> 
//  Copyright (c) 2022 AT Soft
// ----------------------------------------------

namespace ATSoft.Ads
{
    [System.Serializable]
    public class AppOpenAdvertiser
    {
        public ICustomAppOpenAds advertiserScript;
        //public SupportedAdvertisers advertiser;
        public AppOpenAdvertiserSettings appOpenAdvertiserSettings;
        
        public AppOpenAdvertiser(ICustomAppOpenAds advertiserScript, AppOpenAdvertiserSettings appOpenAdvertiserSetting)
        {
            this.appOpenAdvertiserSettings = appOpenAdvertiserSetting;
            this.advertiserScript = advertiserScript;
            //this.advertiser = advertiser;
        }
    }
    
    [System.Serializable]
    public class AppOpenAdvertiserSettings
    {
        public SupportedPlatforms platform;
        public string appId;
        public string ID_TIER_1;
        public string ID_TIER_2;
        public string ID_TIER_3;

        public AppOpenAdvertiserSettings(SupportedPlatforms platform, string appId, string ID_TIER_1, string ID_TIER_2, string ID_TIER_3)
        {
            this.platform = platform;
            this.appId = appId;
            this.ID_TIER_1 = ID_TIER_1;
            this.ID_TIER_2 = ID_TIER_2;
            this.ID_TIER_3 = ID_TIER_3;
        }
    }
}