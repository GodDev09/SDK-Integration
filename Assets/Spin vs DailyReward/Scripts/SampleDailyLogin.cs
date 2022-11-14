using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class SampleDailyLogin : MonoBehaviour
{
    [SerializeField] private DailyLoginReward dailyGift;
    [SerializeField] private Button btnDailyLogin;
    
    // Start is called before the first frame update
    void Start()
    {
        dailyGift.Init();
        btnDailyLogin.onClick.AddListener(OnClickDailyLogin);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnClickDailyLogin()
    {
        dailyGift.gameObject.SetActive(true);
    }

    [Button]
    public void ABCD()
    {
        //-8585348896854775808
        string temp = "-8585348896854775808";
        var a = DateTime.FromBinary(long.Parse(temp));
        Debug.Log(a);
    }
}
