using UnityEngine;

namespace ATSoft
{
    public class InternetNotAvailable : Singleton<InternetNotAvailable>
    {
        [SerializeField] private GameObject prefab;
        private GameObject obj;

        public void ShowLoading()
        {
            Setup();
            if (obj != null)
            {
                if (!obj.activeInHierarchy)
                {
                    obj.SetActive(true);
                    GameUtils.RaiseMessage(new GameMessages.InternetMessage {isConnected = false});
                }
            }
        }

        public void HideLoading()
        {
            if (obj != null)
            {
                if (obj.activeInHierarchy)
                {
                    obj.SetActive(false);
                    GameUtils.RaiseMessage(new GameMessages.InternetMessage {isConnected = true});
                }
            }
        }

        private InternetNotAvailable Setup()
        {
            if (obj == null)
            {
                // Create popup and attach it to UI
                obj = Instantiate(prefab);
                // Configure popup
            }

            return obj.GetComponent<InternetNotAvailable>();
        }
    }
}