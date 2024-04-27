using System;
using UnityEngine;

public class UiEBStartInfo : KAUI
{
    public void SetEelColor(EBGameManager.EBEelColorValue color, int Count)
    {
        if (this._txtColorName != null)
        {
            this._txtColorName.SetText(color._ColorNameText.GetLocalizedString());
        }
        if (this._AniEelImage != null)
        {
            this._AniEelImage.color = color._ColorValue;
        }
        if (this._txtColorCount != null)
        {
            this._txtColorCount.SetText(Count.ToString());
        }
    }

    public override void OnClick(KAWidget item)
    {
        base.OnClick(item);
        if (item.name == "YesBtn")
        {
            this._MsgObject.SendMessage("OnGenericDBButtonClicked", "Yes");
            return;
        }
        if (item.name == "NoBtn")
        {
            this._MsgObject.SendMessage("OnGenericDBButtonClicked", "No");
        }
    }

    public UiEBStartInfo()
    {
    }

    public GameObject _MsgObject;

    public KAWidget _txtColorCount;

    public KAWidget _txtColorName;

    public UITexture _AniEelImage;

    public LocaleString _InstructionsMobile = new LocaleString("");

    public KAWidget _TxtInfo;
}
