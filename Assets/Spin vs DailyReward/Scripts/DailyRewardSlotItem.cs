using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardSlotItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtCoin;
    [SerializeField] private TextMeshProUGUI txtDay;

    [SerializeField] private GameObject tickImg;
    [SerializeField] private Image bg;
    [SerializeField] private GameObject lockImg;
    
    private long valueCoin;
    private int indexLogin;

    [SerializeField]
    private Sprite spriteOn;
    [SerializeField]
    private Sprite spriteOff;

    public void OnSetup(int index, long coin)
    {
        valueCoin = coin;
        indexLogin = index;
        
        txtDay.text = "Day " + (index + 1);
        txtCoin.text = valueCoin.ToString();
    }

    public void SetVisual(State state)
    {
        txtCoin.gameObject.SetActive(state != State.LOCK);
        lockImg.SetActive(state == State.LOCK);
        tickImg.SetActive(state == State.CLAIMED);
        
        bg.sprite = state == State.LOCK ? spriteOff : spriteOn;
    }

    private void OnLoginClick()
    {
        DisableSlot();
        //DiamondLabel.AddFlying(valueCoin,transform.position);
//        DiamondLabel.Add(valueCoin);
        indexLogin++;
        if (indexLogin >= 7)
        {
            indexLogin = 0;
        }
        DailyLoginReward.DailyRewardIndex = indexLogin;
        DailyLoginReward.DailyRewardTime = DateTime.Today.AddDays(1);
        DailyLoginReward.nextReward = DailyLoginReward.DailyRewardTime;
    }

    private void DisableSlot()
    {
        bg.sprite = spriteOff;
        //goFade.SetActive(true);
        //btnClick.interactable = false;       
    }
    
    public enum State
    {
        LOCK,
        CAN_CLAIM,
        CLAIMED
    }
}
