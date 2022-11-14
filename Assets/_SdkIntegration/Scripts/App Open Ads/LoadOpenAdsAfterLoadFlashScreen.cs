using UnityEngine;

namespace ATSoft.Ads
{
    public class LoadOpenAdsAfterLoadFlashScreen : MonoBehaviour
    {
        [SerializeField] private SceneAutoLoad sceneAutoLoad;

        private void OnValidate()
        {
            if (sceneAutoLoad == false)
                sceneAutoLoad = GetComponent<SceneAutoLoad>();
        }

        private void Start()
        {
            sceneAutoLoad.onLoadingProgress.AddListener((t) => AppOpenAdManager.Instance.ShowAOAIfLoadedInLoadingTime());
            GameUtils.AddHandler<GameMessages.AppOpenAdMessage>(HandleAppOpenAdMessage);
        }

        private void SkipFlashScreen()
        {
            sceneAutoLoad.skipMinLoadingTime = true;
        }

        private void HandleAppOpenAdMessage(GameMessages.AppOpenAdMessage msg)
        {
            if (msg.AppOpenAdState != AppOpenAdState.CLOSED)
                return;
            
            SkipFlashScreen();
        }
    }
}