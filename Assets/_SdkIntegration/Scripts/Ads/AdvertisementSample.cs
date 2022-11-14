using ATSoft;
using ATSoft.Ads;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class AdvertisementSample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            UnityAction actionComplete = delegate()
            {
                Debug.Log("ShowInterstitial");
                SceneName sceneNameToLoad = SceneName.Menu;
                SceneManager.LoadScene(sceneNameToLoad.ToString(), LoadSceneMode.Single);
            };
            Advertisements.Instance.ShowInterstitial(actionComplete);
        }
        else if (Input.GetKeyUp(KeyCode.B))
        {
            Advertisements.Instance.ShowBanner(BannerPosition.BOTTOM, BannerType.SmartBanner);
        }
        else if (Input.GetKeyUp(KeyCode.C))
        {
            UnityAction<bool> actionComplete = delegate(bool isSuccess)
            {
                Debug.Log("ShowRewardedVideo " + isSuccess);
            };
            Advertisements.Instance.ShowRewardedVideo(actionComplete, "placement");
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            UnityAction<bool, string> actionComplete = delegate(bool isSuccess, string networkAd)
            {
                Debug.Log($"ShowRewardedVideo {isSuccess} - {networkAd}");
            };
            Advertisements.Instance.ShowRewardedVideo(actionComplete, "placement");
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            RateGameManager.Instance.ShowRateGame();
        }
    }

    public void ShowBanner()
    {
        Advertisements.Instance.ShowBanner(BannerPosition.BOTTOM, BannerType.Banner);
    }
    
    public void ShowSmartBanner()
    {
        Advertisements.Instance.ShowBanner(BannerPosition.BOTTOM, BannerType.SmartBanner);
    }
    
    public void ShowAdaptiveBanner()
    {
        Advertisements.Instance.ShowBanner(BannerPosition.BOTTOM, BannerType.Adaptive);
    }
}