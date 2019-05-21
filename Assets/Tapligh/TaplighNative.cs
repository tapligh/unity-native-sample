using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum NativeLoadErrorStatus {
    NO_INTERNET_ACCSSES,
    APP_NOT_FOUND,
    AD_UNIT_DISABLED,
    AD_UNIT_NOT_FOUND,
    INTERNAL_ERROR,
    NO_AD_READY,
    AD_UNIT_NOT_READY,
    IN_PROCESS
}

//for Json Object
[System.Serializable]
public class NativeAds {
    public string title;
    public string description;
    public string buttonText;
    public string icon;
    public string marketIcon;
    public AspectRatio banners;
    public string token;
}

//for Json Object
[System.Serializable]
public class AspectRatio {
    public string AR9x16;
    public string AR16x9;
    public string AR1x1;
}

public class TaplighNativeAd {
    public NativeAds nAd;
    private Texture2D iconTexture;
    private Texture2D bannerTexture;
    private Texture2D marketIcon;

    public Texture2D IconTexture {
        get { return iconTexture; }
        set { iconTexture = value; }
    }

    public Texture2D BannerTexture {
        get { return bannerTexture; }
        set { bannerTexture = value; }
    }

    public Texture2D MarketIcon {
        get { return marketIcon; }
        set { marketIcon = value; }
    }

}

[System.Serializable]
public enum BannerAspectRatio
{
    AR1x1,
    AR9x16,
    AR16x9
}

public class TaplighNative : MonoBehaviour {
#if !UNITY_EDITOR && UNITY_ANDROID
    AndroidJavaClass taplighJavaInterface;
    AndroidJavaObject currentActivity;
#endif

    public bool initDone = true;

    private float timer = 0;
    private int startTime = 0;
    private int everyTime = 0;
    private int TaplighAdCount = 1;
    private static TaplighNative instance;
    private bool lockRefresh = false;
    private string TaplighUnit = null;
    private bool refreshAdAvailable = false;
    //private TaplighNativeSetting setting = new TaplighNativeSetting ();

    readonly private string TaplighJavaSrc = "com.tapligh.sdk.nativead.TaplighNativeUnity";

    private Action<TaplighNativeAd, string> onAdLoadedListener = null;
    public System.Action<TaplighNativeAd, string> OnAdLoadedListener {
        get { return onAdLoadedListener; }
        set { onAdLoadedListener = value; }
    }

    private Action<NativeLoadErrorStatus, string> onLoadErrorListener = null;
    public System.Action<NativeLoadErrorStatus, string> OnLoadErrorListener {
        get { return onLoadErrorListener; }
        set { onLoadErrorListener = value; }
    }

    public static TaplighNative Native {
        get {
            if (instance == null) {
                GameObject obj = new GameObject ("TaplighNativeUnityObject");
                instance = obj.AddComponent<TaplighNative> ();
            }
            return instance;
        }
    }

    private void Awake () {
        Debug.Log ("Unity3D Interface: Tapligh CREATED - CURRENT ACTIVITY IS DONE");
        DontDestroyOnLoad (this.gameObject);
    }

    private void Update () {
        if (refreshAdAvailable) {
            timer += Time.deltaTime;

            if (!lockRefresh && ((int) timer == startTime)) {
                Debug.Log ("Unity3D Interface: Tapligh init: (" + initDone + ")");
                if (initDone) {
                    Debug.Log ("Unity3D Interface: Load Native Ad Every " + everyTime + "s");
                    Load (TaplighUnit, TaplighAdCount);
                }
                lockRefresh = true;
            }

            if (timer >= everyTime) {
                Debug.Log ("Unity3D Interface: Reload Timer ");
                timer = 0;
                lockRefresh = false;
            }
        }
    }

    BannerAspectRatio Ratio;
    public void SetRatio (BannerAspectRatio ratio) {
        Ratio = ratio;
    }

    /******************************** Generate public methods for usage ********************************/

    public void Initialize (string token) {
        Debug.Log ("Unity3D Interface: Start Initializing In Java (" + TaplighJavaSrc + ")");

#if !UNITY_EDITOR && UNITY_ANDROID
        taplighJavaInterface = new AndroidJavaClass (TaplighJavaSrc);
        taplighJavaInterface.CallStatic ("initialize", token);
        Debug.Log ("Unity3D Interface: Tapligh Initilizing is Done");
        initDone = true;
#endif
    }

    public void Load (string unit, int count) {
        Debug.Log ("Unity3D Interface: Load Native Ad");
#if !UNITY_EDITOR && UNITY_ANDROID
        if (taplighJavaInterface != null) {
            taplighJavaInterface.CallStatic ("load", this.gameObject.name, unit, count, "onAdLoaded", "onLoadError");
        } else {
            Debug.Log ("Unity3D Interface: Tapligh Object in Unity is Null");
        }
#endif
    }

    public void RefreshLoad (string unit, int count, int start, int every) {
        startTime = start;
        everyTime = every;
        TaplighUnit = unit;
        TaplighAdCount = count;
        refreshAdAvailable = true;
    }

    public void OnAdClick (string token) {
        Debug.Log ("Unity3D Interface: On Click Download Ad With Token " + token);
#if !UNITY_EDITOR && UNITY_ANDROID
        if (taplighJavaInterface != null) {
            taplighJavaInterface.CallStatic ("click", this.gameObject.name, token);
        } else {
            Debug.Log ("Unity3D Interface: Tapligh Object in Unity is Null");
        }
#endif
    }

    /******************************** Generate private methods for functionality ********************************/

    private void onAdLoaded (string response) {
        Debug.Log ("Unity3D Interface: Ad Loaded" + response);
        List<string> results = ResponseParser (response);
        String unit = results[0];
        String data = results[1];
        SetNativeDTO (data, unit);
    }

    private void onLoadError (string response) {
        Debug.Log ("Unity3D Interface: Load Error" + response);
        List<string> results = ResponseParser (response);
        String unit = results[0];
        String status = results[1];
        NativeLoadErrorStatus errorStatus = (NativeLoadErrorStatus) (Int32.Parse (status));
        if (onLoadErrorListener != null) { onLoadErrorListener (errorStatus, unit); }
    }

    private TaplighNativeAd SetNativeDTO (string data, string unit) {
        TaplighNativeAd nativeAd = new TaplighNativeAd ();

        //new edit
        string json=data;
        int position = data.IndexOf ("***");
        if (!(position < 0)) {
            var A = data.Substring (0, position);
            json = data.Substring (position + 1);
        }
        Debug.Log ("Json :" + json);

        NativeAds nAdJson = JsonUtility.FromJson<NativeAds> (json);
        
        nativeAd.nAd = nAdJson;
        Debug.Log (nAdJson.banners.AR9x16);

        StartCoroutine (SetNativeAdImages (nativeAd, unit));

        return nativeAd;
    }

    private IEnumerator SetNativeAdImages (TaplighNativeAd ad, string unit) {

        string ratioLink = "";
        Debug.Log (ad.nAd.banners);
        if (ad.nAd.banners != null) {
            switch (Ratio) {
                case BannerAspectRatio.AR16x9:
                    ratioLink = ad.nAd.banners.AR16x9;
                    break;
                case BannerAspectRatio.AR9x16:
                    ratioLink = ad.nAd.banners.AR9x16;
                    break;
                case BannerAspectRatio.AR1x1:
                    ratioLink = ad.nAd.banners.AR1x1;
                    break;
            }

            Debug.Log(ratioLink);

            WWW wwwBanner = new WWW (ratioLink);
            yield return wwwBanner;
            ad.BannerTexture = wwwBanner.texture;
            Debug.Log("banner downloaded");
        }

        if (ad.nAd.icon != null) {
            WWW wwwIcon = new WWW (ad.nAd.icon);
            yield return wwwIcon;
            ad.IconTexture = wwwIcon.texture;
            Debug.Log("Icon downloaded");
        }

        if (ad.nAd.marketIcon != null) {
            WWW wwwMarketIcon = new WWW (ad.nAd.marketIcon);
            yield return wwwMarketIcon;
            ad.MarketIcon = wwwMarketIcon.texture;
            Debug.Log("Market Icon downloaded");
        }

        if (onAdLoadedListener != null) { onAdLoadedListener (ad, unit); }
    }

    private List<string> ResponseParser (string result) {
        List<string> arguments = new List<string> ();

        int deviderIndex = result.IndexOf ('*');
        arguments.Add (result.Substring (0, deviderIndex));

        for (; deviderIndex < result.Length; deviderIndex++) {
            if (result[deviderIndex] != '*') { break; }
        }

        arguments.Add (result.Substring (deviderIndex));

        return arguments;
    }

}