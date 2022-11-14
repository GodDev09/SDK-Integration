using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace ATSoft
{
    public class RateGameManager : Singleton<RateGameManager>
    {
        [SerializeField] private GameObject prefab;
        private GameObject obj;

        public string packageName;
        public string appleAppId;

        private const string CAN_SHOW_RATE = "CAN_SHOW_RATE";

        public static bool CanShowRate
        {
            get => PlayerPrefs.GetInt(CAN_SHOW_RATE, 0) == 0;
            set
            {
                PlayerPrefs.SetInt(CAN_SHOW_RATE, value ? 0 : 1);
                PlayerPrefs.Save();
            }
        }

        [Button]
        public void ShowRateGame()
        {
            if (!CanShowRate)
            {
                Debug.Log("=== CanShowRate: false ===");
                return;
            }
            
            Setup();
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        public void HideRateGame()
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        private RateGameCanvas Setup()
        {
            if (obj == null)
            {
                // Create popup and attach it to UI
                obj = Instantiate(prefab);
                // Configure popup
            }

            return obj.GetComponent<RateGameCanvas>();
        }
    }
}