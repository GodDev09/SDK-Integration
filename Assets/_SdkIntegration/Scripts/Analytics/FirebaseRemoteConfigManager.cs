using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.RemoteConfig;
using UnityEngine;
using UnityEngine.Events;

namespace ATSoft
{
    public class FirebaseRemoteConfigManager
    {
        private UnityAction mFetchSuccessCallback;

        private static void SetupDefaultConfigs(Dictionary<string, object> defaults)
        {
            if (defaults == null) return;
            try
            {
                FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);
                var cs = FirebaseRemoteConfig.DefaultInstance.ConfigSettings;
                FirebaseRemoteConfig.DefaultInstance.SetConfigSettingsAsync(cs);
            }
            catch (Exception)
            {
                // ignored
            }
        }
        public static ConfigValue GetValues(string key)
        {
            return FirebaseRemoteConfig.DefaultInstance.GetValue(key);
        }

        public void FetchData(UnityAction fetchSuccessCallback)
        {
            mFetchSuccessCallback = fetchSuccessCallback;
            // FetchAsync only fetches new data if the current data is older than the provided
            // timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
            // By default the timespan is 12 hours, and for production apps, this is a good
            // number.  For this example though, it's set to a timespan of zero, so that
            // changes in the console will always show up immediately.
            try
            {
                var fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                    TimeSpan.Zero);
                fetchTask.ContinueWith(FetchComplete);

            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void FetchComplete(Task fetchTask)
        {
            if (fetchTask.IsCanceled)
            {
                Debug.Log("Fetch canceled.");
            }
            else if (fetchTask.IsFaulted)
            {
                Debug.Log("Fetch encountered an error.");
            }
            else if (fetchTask.IsCompleted)
            {
                Debug.Log("Fetch completed successfully!");
            }
            var info = FirebaseRemoteConfig.DefaultInstance.Info;
            switch (info.LastFetchStatus)
            {
                case LastFetchStatus.Success:
                    FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync();
                    Debug.Log(String.Format("Remote data loaded and ready (last fetch time {0}).",
                        info.FetchTime));
                    if (mFetchSuccessCallback != null)
                    {
                        mFetchSuccessCallback();
                    }
                    break;
                case LastFetchStatus.Failure:
                    switch (info.LastFetchFailureReason)
                    {
                        case FetchFailureReason.Error:
                            Debug.Log("Fetch failed for unknown reason");
                            break;
                        case FetchFailureReason.Throttled:
                            Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }
                    break;
                case LastFetchStatus.Pending:
                    Debug.Log("Latest Fetch call still pending.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void SetupDefaultConfigs()
        {
            var defaults = new Dictionary<string, object>
            {
                {Keys.ConfigAdCappingTime, 0},
                {Keys.ConfigIntersStartLevel, 2},
                {Keys.ConfigAoaIdTier1And, "ca-app-pub-3101218177270519/2184139890"},
                {Keys.ConfigAoaIdTier2And, "ca-app-pub-3101218177270519/2184139890"},
                {Keys.ConfigAoaIdTier3And, "ca-app-pub-3101218177270519/2184139890"},
                {Keys.ConfigPopupRateUsLevel, "5,10,20,30,50,75,100,150,200,250,300,350,400"},
                {Keys.ConfigPositionShowInter, 0}
            };

            SetupDefaultConfigs(defaults);
        }
    }
}
