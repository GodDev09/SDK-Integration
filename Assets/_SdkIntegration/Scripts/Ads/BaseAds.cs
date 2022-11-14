using System;
using UnityEngine;
using UnityEngine.Events;

namespace ATSoft.Ads
{
    public class BaseAds : MonoBehaviour
    {
        protected bool initialized;
        protected UnityAction<bool, BannerPosition, BannerType> DisplayResult;
        protected UnityAction OnInterstitialClosed;
        protected UnityAction<string> OnInterstitialClosedWithAdvertiser;
        protected UnityAction<bool> OnCompleteMethod;
        protected UnityAction<bool, string> OnCompleteMethodWithAdvertiser;
        protected string bannerId;
        protected string interstitialId;
        protected string rewardedVideoId;
        protected BannerPosition bannerPosition;
        protected BannerType bannerType;
        protected int currentRetryInterstitial;
        protected int currentRetryRewardedVideo;
        protected bool autoDisableLoadingInter;
        protected IDisposable obInter;

        protected readonly int maxRetryCount = 10;

        protected void ShowMessage(string msg)
        {
            AdsNotAvailable.Instance.Show();
            return;
#if !UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaObject @static =
 new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject androidJavaObject = new AndroidJavaClass("android.widget.Toast");
        androidJavaObject.CallStatic<AndroidJavaObject>("makeText", new object[]
        {
                @static,
                msg,
                androidJavaObject.GetStatic<int>("LENGTH_SHORT")
        }).Call("show", Array.Empty<object>());
#endif
        }

        protected virtual void OnDestroy()
        {
        }

        protected void OnInterstitialAvailable()
        {
            //LogEvent
#if PUB_RK //Rocket
#elif PUB_AD1 //Ad1
#elif PUB_FAL //Fal
            AnalyticsManager.LogEventInterstitialAvailable();
#else //In-house
            AnalyticsManager.LogEventInterstitialAvailable();
#endif
            Debug.Log(gameObject.name + "OnInterstitialAvailable()");
        }

        protected void OnInterstitialFailedToShow(string err)
        {
            //LogEvent
#if PUB_RK //Rocket
#elif PUB_AD1 //Ad1
#elif PUB_FAL //Fal 1
            AnalyticsManager.LogEventInterstitialFailToShow(err);
#else //In-house
            AnalyticsManager.LogEventInterstitialFailToShow(err);
#endif
            Debug.Log(gameObject.name + "LogEventInterstitialFailToShow() - " + err);
        }

        protected void OnRewardVideoAvailable()
        {
            //LogEvent
#if PUB_RK //Rocket
#elif PUB_AD1 //Ad1
#elif PUB_FAL //Fal
            AnalyticsManager.LogEventRewardVideoAvailable();
#else //In-house
            AnalyticsManager.LogEventRewardVideoAvailable();
#endif
            Debug.Log(gameObject.name + "OnRewardVideoAvailable()");
        }

        protected void OnRewardVideoFailedToShow(string err)
        {
            //LogEvent
#if PUB_RK //Rocket
#elif PUB_AD1 //Ad1
#elif PUB_FAL //Fal 1
            AnalyticsManager.LogEventRewardVideoFailToShow(err);
#else //In-house
            AnalyticsManager.LogEventRewardVideoFailToShow(err);
#endif
            Debug.Log(gameObject.name + "LogEventRewardVideoFailToShow() - " + err);
        }

        protected void OnRewardVideoCompleted()
        {
            //LogEvent
#if PUB_RK //Rocket
#elif PUB_AD1 //Ad1
#elif PUB_FAL //Fal 1
            AnalyticsManager.LogEventRewardAdComplete();
#else //In-house
            AnalyticsManager.LogEventRewardAdComplete();
#endif
            Debug.Log(gameObject.name + "LogEventRewardAdComplete()");
        }
    }
}