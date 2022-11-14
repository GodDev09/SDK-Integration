using System;
using Firebase.Analytics;
using Firebase.RemoteConfig;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace ATSoft
{
    public class FirebaseManager : Singleton<FirebaseManager>, IService
    {
        public bool enableRemoteConfig;
        private bool isLoaded;
        public bool IsRemoteConfigReady { get; private set; }

        private Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
        private FirebaseRemoteConfigManager firebaseRemoteConfigManager;

        public void Initialize()
        {
            Debug.Log("Initialize " + (typeof(FirebaseManager)));
            
            firebaseRemoteConfigManager ??= new FirebaseRemoteConfigManager();
            
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    InitializeFirebase();
                    AnalyticsManager.SetUserPropertyDayPlaying();
                    Debug.Log("Firebase dependencyStatus: " + dependencyStatus);
                }
                else
                {
                    Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
                }
            });
        }

        private void InitializeFirebase()
        {
            isLoaded = true;
            if (!enableRemoteConfig) return;

            FirebaseRemoteConfigManager.SetupDefaultConfigs();
            FetchData(() =>
            {
                Debug.Log("Fetch Success");
                IsRemoteConfigReady = true;
                GameUtils.RaiseMessage(new GameMessages.FirebaseMessage
                    {FirebaseMessageAction = FirebaseAction.Initialized});
                FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
            });
        }

        private bool IsFirebaseReady()
        {
            return isLoaded;
        }

        #region Remote Config

        private void FetchData(UnityAction successCallback)
        {
            firebaseRemoteConfigManager.FetchData(successCallback);
        }

        public static ConfigValue GetConfigValue(string key)
        {
            return FirebaseRemoteConfigManager.GetValues(key);
        }

        #endregion

        #region Analytics

        public void LogAnalyticsEvent(string eventName, string parameterName, double value)
        {
            if (IsFirebaseReady())
            {
                FirebaseAnalytics.LogEvent(eventName, parameterName, value);
            }
        }

        public void LogAnalyticsEvent(string eventName, params Parameter[] param)
        {
            if (IsFirebaseReady())
            {
                FirebaseAnalytics.LogEvent(eventName, param);
            }
            else
            {
                Debug.Log("LogAnalyticsEvent IsFirebaseReady() = false");
            }
        }

        public void LogAnalyticsEvent(string eventName)
        {
            if (IsFirebaseReady())
            {
                FirebaseAnalytics.LogEvent(eventName);
            }
        }

        public void SetUserProperty(string propertyName, string property)
        {
            if (IsFirebaseReady())
            {
                FirebaseAnalytics.SetUserProperty(propertyName, property);
            }
        }

        #endregion
    }

    public enum FirebaseAction
    {
        None,
        Initialized,
        UpdateRemoteConfig
    }
}