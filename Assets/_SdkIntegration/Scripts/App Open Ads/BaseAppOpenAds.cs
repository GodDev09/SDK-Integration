//  ---------------------------------------------
//  Author:     DatDz <steven@atsoft.io> 
//  Copyright (c) 2022 AT Soft
// ----------------------------------------------

using System;
using UnityEngine;
using UnityEngine.Events;

namespace ATSoft.Ads
{
    public class BaseAppOpenAds : MonoBehaviour
    {
        protected string appId;
        protected string ID_TIER_1;
        protected string ID_TIER_2;
        protected string ID_TIER_3;
        
        protected string ID_TEST;
        
        protected bool isShowingAd = false;
        protected int tierIndex = 1;
        protected bool enableTestAd;
        protected bool initialized;
        
        protected UnityAction appOpenAdClosed;
    }
}