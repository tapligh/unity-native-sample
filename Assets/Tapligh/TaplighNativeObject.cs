using System;
using System.IO;
using UnityEngine;
using ArabicSupport;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class TaplighNativeObject : MonoBehaviour
{
    private Action<string> onAdClickListener = null;
    public Action<string> OnAdClickListener
    {
        get { return onAdClickListener; }
        set { onAdClickListener = value; }
    }

    public void SetBanner(TaplighNativeAd ad,Image img)
    {
        if(ad.BannerTexture != null)
        {
             img.sprite = Sprite.Create(ad.BannerTexture, new Rect(0, 0,ad.BannerTexture.width, ad.BannerTexture.height), Vector2.zero);
        }
    }

    public void SetIcon(TaplighNativeAd ad,Image icon)
    {
        if(ad.BannerTexture != null)
        {
             icon.sprite = Sprite.Create(ad.IconTexture, new Rect(0, 0,ad.IconTexture.width, ad.IconTexture.height), Vector2.zero);
        }
    }

    public void SetMarketIcon(TaplighNativeAd ad,Image marketIcon)
    {
        if(ad.BannerTexture != null)
        {
             marketIcon.sprite = Sprite.Create(ad.MarketIcon, new Rect(0, 0,ad.MarketIcon.width, ad.MarketIcon.height), Vector2.zero);
        }
    }

    public void SetTitle(TaplighNativeAd ad, Text title)
    {
        title.text=ArabicFixer.Fix(ad.nAd.title,true);
    }

    public void SetDescription(TaplighNativeAd ad,Text desc)
    {
        desc.text=ArabicFixer.Fix(ad.nAd.description,true);
    }

    public void SetButtonText(TaplighNativeAd ad,Text buttonText)
    {
        buttonText.text=ArabicFixer.Fix(ad.nAd.buttonText,true);
    }



}
