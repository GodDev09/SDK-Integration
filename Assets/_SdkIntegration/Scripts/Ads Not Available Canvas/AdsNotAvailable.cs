using System;
using UniRx;
using UnityEngine;

namespace ATSoft
{
    public class AdsNotAvailable : Singleton<AdsNotAvailable>
    {
        [SerializeField] private GameObject prefab;

        private float timeDuration = 1f;
        private GameObject obj;
        private GameObject content;
        private IDisposable timer;

        public void Show()
        {
            Setup();
            if (obj != null)
            {
                obj.SetActive(true);
                timer?.Dispose();
                timer = Observable.Timer(TimeSpan.FromSeconds(timeDuration)).Subscribe(_ => { Hide(); })
                    .AddTo(this);
            }
        }

        private void Hide()
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        private AdsNotAvailable Setup()
        {
            if (obj == null)
            {
                // Create popup and attach it to UI
                obj = Instantiate(prefab);
                // Configure popup
            }

            return obj.GetComponent<AdsNotAvailable>();
        }
    }
}