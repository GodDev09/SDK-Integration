using System;
using UnityEngine;
using ATSoft.Ads;
using ATSoft.ATT;
using UniRx;

namespace ATSoft
{
    public class SdkAutoInitializer : MonoBehaviour
    {
        public bool initAppTrackingTransparency;
        public bool initAppOpenAd;
        public bool initAdvertisement;
        public bool initFirebase;

        private Subject<bool> InternetSubject = new Subject<bool>();
        private IObservable<bool> internetObservable;

        private void Start()
        {
            internetObservable = Observable.Interval(TimeSpan.FromSeconds(1), Scheduler.MainThreadIgnoreTimeScale)
                .Select(_ => InternetAvailable);

            internetObservable.DistinctUntilChanged().Subscribe(_ => { InternetSubject.OnNext(_); });
            
            internetObservable.Where(haveInternet => haveInternet)
                .Timeout(TimeSpan.FromSeconds(60))
                .Take(1)
                .Subscribe(_ =>
                {
                    //Debug.Log("Internet Ready");
                    if (initAppTrackingTransparency)
                        AppTrackingTransparency.Instance.Initialize();
                    if (initAppOpenAd)
                        AppOpenAdManager.Instance.Initialize();
                }).AddTo(this);

            if (initAdvertisement)
            {
                internetObservable?
                    .Take(1).Delay(TimeSpan.FromSeconds(1f))
                    .Subscribe(_ => Advertisements.Instance.Initialize());
            }

            if (initFirebase)
            {
                internetObservable?
                    .Take(1).Delay(TimeSpan.FromSeconds(2f))
                    .Subscribe(_ => FirebaseManager.Instance.Initialize());
            }


            internetObservable.Where(haveInternet => !haveInternet).Subscribe(_ =>
            {
                //Debug.LogError("Internet is not Ready !!!");
                InternetNotAvailable.Instance.ShowLoading();
            });
            
            internetObservable.Where(haveInternet => haveInternet).Subscribe(_ =>
            {
                //Debug.Log("Internet is Ready !!!");
                InternetNotAvailable.Instance.HideLoading();
            });
        }
        
        private bool InternetAvailable => Application.internetReachability != NetworkReachability.NotReachable;
    }
}
