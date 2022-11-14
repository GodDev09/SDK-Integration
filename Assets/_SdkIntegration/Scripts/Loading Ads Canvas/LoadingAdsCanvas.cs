using UnityEngine;

namespace ATSoft
{
    public class LoadingAdsCanvas : Singleton<LoadingAdsCanvas>
    {
        [SerializeField] private GameObject prefab;
        private GameObject obj;

        public void ShowLoading()
        {
            Setup();
            if (obj != null) obj.SetActive(true);
        }

        public void HideLoading()
        {
            if (obj != null) obj.SetActive(false);
        }

        private LoadingAdsCanvas Setup()
        {
            if (obj == null)
            {
                // Create popup and attach it to UI
                obj = Instantiate(prefab);
                // Configure popup
            }

            return obj.GetComponent<LoadingAdsCanvas>();
        }
    }
}
