using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class NativeController : MonoBehaviour {
    public readonly string TOKEN = "e8afd933-e40f-4163-b08e-13a7bcc1c1ba";
    readonly string UNIT = "dd8a4d9d-c283-4885-8d19-b02c0bd4442f";
    readonly int COUNT = 1;

    //Assign in Inspector for Ad
    public Image bannerImage;

    public Text title;
    public Text Description;
    public Image Icon;

    private TaplighNativeObject nativeBuilder = new TaplighNativeObject ();

    void Start () {
        TaplighNative.Native.Initialize (TOKEN);
        LoadAd ();
    }

    public void LoadAd () {
        TaplighNative.Native.SetRatio (BannerAspectRatio.AR16x9);

        TaplighNative.Native.OnAdLoadedListener = TaplighOnAdLoaded;
        TaplighNative.Native.OnLoadErrorListener = TaplighOnLoadError;

        //به ازای هر بار صدا زدن این متد اطلاعات تبلیغ در اختیار شما قرار خواهد گرفت
        //TaplighNative.Native.Load (UNIT, COUNT);

        //با تنظیم زمان شروع و زمان دوره ای کتابخانه اطلاعات تبلیغات جدید را در زمان مورد نظر در اختیار شما قرار میدهد
        TaplighNative.Native.RefreshLoad (UNIT, COUNT, 1, 10);
    }

    //Ref object for Ad
    TaplighNativeAd ads;

    //در صورتیکه تبلیغ آماده باشد و مشکلی در دریافت آن به وجود نیامده باشد اجرا می شود
    private void TaplighOnAdLoaded (TaplighNativeAd ad, string unit) {

        nativeBuilder.SetBanner (ad, bannerImage);
        ads = ad;

        nativeBuilder.SetTitle (ad, title);
        nativeBuilder.SetDescription (ad, Description);
        nativeBuilder.SetIcon (ad, Icon);

    }

    //در صورتیکه مشکلی در دریافت تبلیغ به وجود بیاید  اجرا می شود
    private void TaplighOnLoadError (NativeLoadErrorStatus error, string unit) {
        Debug.Log ("Controller Unity3D: Load Error " + error + " in unit " + unit);
        string message = "On Load Error : ";

        switch (error) {
            case NativeLoadErrorStatus.NO_INTERNET_ACCSSES:
                message += "No Internet Access";
                break;
            case NativeLoadErrorStatus.APP_NOT_FOUND:
                message += "App Not Found";
                break;
            case NativeLoadErrorStatus.AD_UNIT_DISABLED:
                message += "Ad Unit Disabled";
                break;
            case NativeLoadErrorStatus.AD_UNIT_NOT_FOUND:
                message += "Ad Unit Not Found";
                break;
            case NativeLoadErrorStatus.INTERNAL_ERROR:
                message += "Internal Error";
                break;
            case NativeLoadErrorStatus.NO_AD_READY:
                message += "No Ad Ready";
                break;
            case NativeLoadErrorStatus.AD_UNIT_NOT_READY:
                message += "Ad Unit Not Ready";
                break;
            case NativeLoadErrorStatus.IN_PROCESS:
                message += "In Process";
                break;

        }

        Debug.Log (message);
    }

    //Assign for Banner Click On Button Component
    public void OnBannerClick () {
        TaplighNative.Native.OnAdClick (ads.nAd.token);
    }
}