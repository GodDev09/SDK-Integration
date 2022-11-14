using System;
using UnityEngine;
#if USE_ADJUST
using com.adjust.sdk;
#endif
namespace ATSoft
{
    public class AdjustEventKeys
    {
        public const string first_open_app = "";// "m8gceu";
        public const string open_app = "";
        
        public static string complete_level_1 = "";
        public static string complete_level_2 = "";
        public static string complete_level_3 = "";
        public static string complete_level_4 = "";
        public static string complete_level_5 = "";
        
        public static string ad_inters_ad_eligible = "";// "ashbh2";
        public static string ad_inters_api_called = "";
        public static string ad_inters_displayed = "";
        public static string ad_rewarded_ad_eligible = "";
        public static string ad_rewarded_api_called = "";
        public static string ad_rewarded_displayed = "";
        
        public const string purchase = "";// "m96foh";
        public const string purchase_failed = "";
        public const string purchase_notverified = "";
        public const string purchase_unknown = "";
    }
    
    public class AdjustManager : Singleton<AdjustManager>, IService
    {

        public void Initialize()
        {
            Debug.Log("Initialize " + (typeof(AdjustManager)));
#if USE_ADJUST
            TrackOpenAppEvent();
#endif
        }
        
#if USE_ADJUST
        public void TrackAdEvent(string token)
        {
            if(!Adjust.isEnabled()) return;
            
            try
            {
                AdjustEvent adjustEvent = new AdjustEvent(token);
                Adjust.trackEvent(adjustEvent);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        public void TrackOpenAppEvent()
        {
            if(!Adjust.isEnabled()) return;
            
            AdjustEvent adjustEvent = new AdjustEvent(AdjustEventKeys.open_app);
            Adjust.trackEvent(adjustEvent);
        }
        
        public void TrackFirstOpenAppEvent()
        {
            if(!Adjust.isEnabled()) return;
            
            AdjustEvent adjustEvent = new AdjustEvent(AdjustEventKeys.first_open_app);
            Adjust.trackEvent(adjustEvent);
        }

        public void TrackCompleteLevelEvent(int level)
        {
            if(!Adjust.isEnabled()) return;
            
            string key = AdjustEventKeys.complete_level_1;
            switch (level)
            {
                case 1:
                    key = AdjustEventKeys.complete_level_1;
                    break;
                case 2:
                    key = AdjustEventKeys.complete_level_2;
                    break;
                case 3:
                    key = AdjustEventKeys.complete_level_3;
                    break;
                case 4:
                    key = AdjustEventKeys.complete_level_4;
                    break;
                case 5:
                    key = AdjustEventKeys.complete_level_5;
                    break;
                default:
                    key = AdjustEventKeys.complete_level_1;
                    break;
            }
            AdjustEvent adjustEvent = new AdjustEvent(key);
            Adjust.trackEvent(adjustEvent);
        }
#endif
    }
}