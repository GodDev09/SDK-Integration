using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LuckySpin : MonoBehaviour
{
    #region  Properties
    [Header("Settings")]
    public List<SpinElementData> data;
    public Ease[] ease;

    [Header("Ref")] 
    [SerializeField] private AudioSource sfx;

    [SerializeField] private Button closeBtn;
    [SerializeField] TextMeshProUGUI[] piecesTxt;
    [SerializeField] private Transform wheel;
    [SerializeField] private TMP_Text freeLabel;
    //[SerializeField] private Transform flyingPos;

    private bool canPlay = true;
    private DateTime nextTime;
    
    private static DateTime nextTimeWheel;
    public const string WheelTimeKey = "WheelTimeKey";
    public static DateTime WheelTime
    {
        set => PlayerPrefs.SetString(WheelTimeKey,value.ToBinary().ToString());
        get
        {
            if (!PlayerPrefs.HasKey(WheelTimeKey))
            {
                return DateTime.Now;
            }
            return DateTime.FromBinary(long.Parse(PlayerPrefs.GetString(WheelTimeKey)));
        }
    }
    
    #endregion
    
    #region Core

    public static bool CanClaim()
    {
        if (nextTimeWheel == default)
        {
            nextTimeWheel = WheelTime;
        }
        var nextTime = nextTimeWheel;

        return (DateTime.Compare(nextTime, DateTime.Now) <= 0);
    }
    
    public void Play()
    {
        if (DateTime.Compare(nextTime, DateTime.Now) > 0 || !canPlay)
        {
           return;
        }
        
        Spin(onSpinComplete: delegate
        {
            nextTime = DateTime.Now.AddHours(2);
            WheelTime = nextTime;
            nextTimeWheel = WheelTime;
        });
    }

    public void PlayWithAds()
    {
        if (!canPlay)
            return;

        //todo: Call show ads here
        // bool adAvailable = MAXManager.Instance.ShowRewardedAd("Lucky Spin", delegate { Spin(null); });
        //
        // if (!adAvailable)
        // {
        //     //Ad not available
        //     //todo
        //     Debug.LogError("Show pop-up Ad not available here");
        // }
    }

    void Spin(Action onSpinComplete)
    {
        canPlay = false;
        closeBtn.interactable = false;
        
        sfx.Play();
        SpinElementData dat = data[Random.Range(0,data.Count)];
        float randAngle = dat.angle;
        float randTimer = Random.Range(2, 4f);
        float angle = 360 * Random.Range(2, 6) + randAngle - 45;
        
        Debug.Log(dat.angle+"-"+angle);
        
        Sequence sequence = DOTween.Sequence(); // create a sequence
        sequence.Append(wheel.transform.DORotate(
                new Vector3(0, 0f, angle),
                randTimer,
                RotateMode.FastBeyond360)
            .SetEase(ease[Random.Range(0, ease.Length)]));
        sequence.onComplete += delegate
        {
            canPlay = true; 
            Debug.Log(dat.coin);
            onSpinComplete?.Invoke();
            
            sfx.Stop();
            closeBtn.interactable = true;
            UICoinLabel.Instance.Flying(dat.coin, wheel.transform.position);
        };
    }

    #endregion

    #region  Unity Callbacks
    private void Start()
    {
        nextTime = WheelTime;

        if (DateTime.Compare(nextTime, DateTime.Now) <= 0)
        {
            Debug.Log("can claim");
        }
        else
        {
            Debug.Log("can't claim");
        }
        
        // Init Pieces Txt
        for (int i = 0; i < piecesTxt.Length; i++)
            piecesTxt[i].text = data[i].coin.ToString();
    }
    
    private void Update()
    {
        if (DateTime.Compare(nextTime, DateTime.Now) <= 0)
        {
            freeLabel.SetText("Free");
        }
        else
        {
            var time = nextTime.Subtract(DateTime.Now);
            freeLabel.SetText($"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}");
        }
    }
    #endregion
    
    
    [Serializable]
    public struct SpinElementData
    {
        public int coin;
        public float angle;
    }
}
