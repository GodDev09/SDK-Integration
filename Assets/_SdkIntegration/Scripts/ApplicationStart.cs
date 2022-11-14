using System;
using ATSoft.ATT;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ATSoft
{
    public static class ApplicationStart
    {
        public static Subject<bool> InternetSubject = new Subject<bool>();
        private static IObservable<bool> internetObservable;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnStartBeforeSceneLoad()
        {
            RegisterSceneEvent();
            //RegisterServices();
        }

        private static void RegisterServices()
        {
            Application.runInBackground = true;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            internetObservable = Observable.Interval(TimeSpan.FromSeconds(1), Scheduler.MainThreadIgnoreTimeScale)
                .Select(_ => IntenetAvaiable);

            internetObservable.DistinctUntilChanged().Subscribe(_ => { InternetSubject.OnNext(_); });
            internetObservable.Where(haveInternet => haveInternet)
                .Timeout(TimeSpan.FromSeconds(60))
                .Take(1)
                .DoOnCompleted(() =>
                {
                   //
                })
                .Subscribe(_ =>
                {
                    Debug.Log("Internet Ready");
                    RegisterService<AppTrackingTransparency>();
                }, ex =>
                {
                    Debug.LogError("Internet is not Ready");
                },() =>
                {
                    // RegisterService<Advertisements>();
                    // RegisterService<FirebaseManager>();
                });
        }

        private static void RegisterSceneEvent()
        {
            ServiceFactory.Instance.RegisterSingleton<MessageRouter>();
            SceneManager.sceneUnloaded += _ =>
            {
                ServiceFactory.Instance.Resolve<MessageRouter>().Reset();
                ServiceFactory.Instance.Reset();
            };
        }

        private static T RegisterService<T>() where T : class, IService, new()
        {
            SingletonServiceClass<T>.Instance.Initialize();
            return SingletonServiceClass<T>.Instance;
        }

        private static T RegisterMonoService<T>() where T : Component
        {
            var fullName = typeof(T).FullName;
            var obj = new GameObject();
            if (!string.IsNullOrEmpty(fullName))
                obj.name = fullName;
            Object.DontDestroyOnLoad(obj);
            var _instance = obj.AddComponent<T>();
            return _instance;
        }

        private static bool IntenetAvaiable => Application.internetReachability != NetworkReachability.NotReachable;
    }
}