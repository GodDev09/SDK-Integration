using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class DailyLoginReward : MonoBehaviour
{
    public const string DailyRewardKey = "DailyRewardKey";

    public static int DailyRewardIndex
    {
        set => PlayerPrefs.SetInt(DailyRewardKey, value);
        get => PlayerPrefs.GetInt(DailyRewardKey, 0);
    }

    public const string DailyRewardTimeKey = "DailyRewardTimeKey";

    public static DateTime DailyRewardTime
    {
        set => PlayerPrefs.SetString(DailyRewardTimeKey,value.ToBinary().ToString());
        get
        {
            if (!PlayerPrefs.HasKey(DailyRewardTimeKey))
            {
                return DateTime.Today;
            }
            return DateTime.FromBinary(long.Parse(PlayerPrefs.GetString(DailyRewardTimeKey)));
        }
    }

    public const string GetDoubleTodayKey = "GetDoubleToday";

    public static bool GetDoubleToday
    {
        set => PlayerPrefs.SetInt(GetDoubleTodayKey, value ? 1 : 0);
        get => PlayerPrefs.GetInt(GetDoubleTodayKey, 0) == 1;
    }

    public static DateTime nextReward;

    public Button claimBtn;
    public Button watchAdsBtn;

    [SerializeField]
    private List<DailyRewardSlotItem> lsSlotItems = new List<DailyRewardSlotItem>();

    [SerializeField]
    private long[] configsDailyLogin;

    private int todayIndex;
    
    public void Init()
    {
        var curDayLogin = DailyRewardIndex;
        for(int i = 0; i < lsSlotItems.Count; i++)
        {
            long configDailyLogin = configsDailyLogin[i];

            bool isToday = DateTime.Compare(DailyRewardTime,DateTime.Today) == 0 && (i == curDayLogin);
            lsSlotItems[i].OnSetup(i, configDailyLogin);
            lsSlotItems[i].SetVisual(
                isToday ? 
                    DailyRewardSlotItem.State.CAN_CLAIM : 
                    i >= curDayLogin ? 
                        DailyRewardSlotItem.State.LOCK : 
                        DailyRewardSlotItem.State.CLAIMED);

            if (isToday)
            {
                Debug.Log("Not receive today yet");
                todayIndex = i;
                GetDoubleToday = false;
            }
        }

        claimBtn.interactable = CanClaim();
        watchAdsBtn.interactable = GetDoubleToday == false;
    }

    public void Claim()
    {
        lsSlotItems[todayIndex].SetVisual(DailyRewardSlotItem.State.CLAIMED);
        UICoinLabel.Instance.Flying(configsDailyLogin[todayIndex], transform.position);

        DailyRewardIndex = (DailyRewardIndex + 1) % 7;
        DailyRewardTime = DateTime.Today.AddDays(1);
        nextReward = DailyRewardTime;

        claimBtn.interactable = CanClaim();
    }

    public void ClaimWatchAds()
    {
        //todo: Call show ads here
        // var adAvailable = MAXManager.Instance.ShowRewardedAd("Daily Reward",delegate
        // {
        //     lsSlotItems[todayIndex].SetVisual(UIDailyRewardSlotItem.State.CLAIMED);
        //     if (claimBtn.interactable == true)
        //     {
        //         DailyRewardIndex = (DailyRewardIndex + 1) % 7;
        //         DailyRewardTime = DateTime.Today.AddDays(1);
        //         nextReward = DailyRewardTime;
        //
        //         claimBtn.interactable = CanClaim();
        //
        //         UICoinLabel.Instance.Flying(configsDailyLogin[todayIndex] * 2, transform.position);
        //     }
        //     else
        //     {
        //         UICoinLabel.Instance.Flying(configsDailyLogin[todayIndex], transform.position);
        //     }
        //     
        //     GetDoubleToday = true;
        //     watchAdsBtn.interactable = GetDoubleToday == false;
        // });
        // if (!adAvailable)
        // {
        //     //todo: Ad not available
        //     Debug.LogError("Show pop-up Ad not available here");
        // }
    }

    public static bool CanClaim()
    {
        if (nextReward == default)
        {
            nextReward = DailyRewardTime;
        }
        return DateTime.Compare(DailyRewardTime, DateTime.Today) == 0;
    }
}
