using System;
using System.Collections.Generic;
using UnityEngine;

public class UiEBNextLevel : KAUI
{
    protected override void Start()
    {
        base.Start();
    }

    private void OnEnable()
    {
        this.mCurrNonInteractiveTime = 5f;
        this.UnlockType(UiEBNextLevel.LevelCompleteType.Normal);
        if (this.mCurrLevelCompleteType == UiEBNextLevel.LevelCompleteType.None)
        {
            this.mCurrLevelCompleteType = this.mUnlockedTypes[UnityEngine.Random.Range(0, this.mUnlockedTypes.Count)];
        }
        this.mDisplayMessage = true;
        this.SetVisibility(false);
    }

    private void OnDisable()
    {
        this.SetVisibility(false);
        this.mCurrLevelCompleteType = UiEBNextLevel.LevelCompleteType.None;
    }

    protected override void Update()
    {
        base.Update();
        if (this.mDisplayMessage)
        {
            this.mDisplayMessage = false;
            this.DisplayMessage();
        }
        if (this.mCurrNonInteractiveTime > 0f)
        {
            this.mCurrNonInteractiveTime -= Time.deltaTime;
            if (this.mCurrNonInteractiveTime <= 0f)
            {
                GameObject pDragonFlameParticle = this._MsgObject.GetComponent<EBGameManager>().pDragonFlameParticle;
                if (pDragonFlameParticle != null)
                {
                    pDragonFlameParticle.transform.parent = null;
                    UnityEngine.Object.Destroy(pDragonFlameParticle);
                }
                this.SetState(KAUIState.INTERACTIVE);
                this.SetVisibility(true);
                if (this._SndNextLevelMusic)
                {
                    SnChannel.Play(this._SndNextLevelMusic, "DEFAULT_POOL", true);
                    return;
                }
            }
            else
            {
                this.SetState(KAUIState.DISABLED);
                this.SetVisibility(false);
            }
        }
    }

    public void SetDisplayType(UiEBNextLevel.LevelCompleteType type, string UserMessagetxt, int LevelsCompletedCount, EBGameManager.EBEelColorValue NextLvlColor, int NextLvlEelsCount, int Score, int LivesLeft, float Accuracy)
    {
        this.mAccuracy = Accuracy;
        this.mLives = LivesLeft;
        this.mEelsRequiredCount = NextLvlEelsCount;
        this.mScore = Score;
        this.mCurrLevelCompleteType = type;
        this.mCurrUserMessage = UserMessagetxt;
        this.mLevelsCompletedCount = LevelsCompletedCount;
        this.mColor = NextLvlColor;
        if (type != UiEBNextLevel.LevelCompleteType.None)
        {
            this.UnlockType(type);
        }
    }

    public override void OnClick(KAWidget item)
    {
        base.OnClick(item);
        if (item.name == "YesBtn" || item.name == "YesSpBtn")
        {
            this._MsgObject.SendMessage("OnGenericDBButtonClicked", "Yes");
            return;
        }
        if (item.name == "NoBtn" || item.name == "NoSpBtn")
        {
            this._MsgObject.SendMessage("OnGenericDBButtonClicked", "No");
        }
    }

    private void DisplayMessage()
    {
        if (this.mCurrLevelCompleteType == UiEBNextLevel.LevelCompleteType.None)
        {
            return;
        }
        if (this.mCurrLevelCompleteType == UiEBNextLevel.LevelCompleteType.Normal)
        {
            this._SpecialMsgRoot.SetActive(false);
            this._NormalMsgRoot.SetActive(true);
            this._NormalMsgSet._TxtUserText.SetText(this.mCurrUserMessage);
            if (this._UserTips != null && this._UserTips.Length != 0)
            {
                this._NormalMsgSet._TxtTipDisplayInfo.SetText(this._UserTips[UnityEngine.Random.Range(0, this._UserTips.Length)].GetLocalizedString());
            }
            this._NormalMsgSet._TxtLevelCompletedCount.SetText(this._LevelCompletedMessage.GetLocalizedString().Replace("%%", this.mLevelsCompletedCount.ToString()));
            if (this._NormalMsgSet._TxtAccuracy != null)
            {
                this._NormalMsgSet._TxtAccuracy.SetText(this.mAccuracy.ToString("n1") + " %");
            }
        }
        else if (this.mCurrLevelCompleteType == UiEBNextLevel.LevelCompleteType.BonusEel)
        {
            this._SpecialMsgRoot.SetActive(true);
            this._NormalMsgRoot.SetActive(false);
            this._SpecialMsgSet._AniDisplayImage.SetTexture(this._BonusEelImage, false, null);
            if (KAInput.pInstance.IsTouchInput())
            {
                this._SpecialMsgSet._TxtDisplayInfo.SetText(this._BonusEelTextMobile.GetLocalizedString());
            }
            else
            {
                this._SpecialMsgSet._TxtDisplayInfo.SetText(this._BonusEelText.GetLocalizedString());
            }
            this._SpecialMsgSet._TxtUserText.SetText(this.mCurrUserMessage);
            if (this._UserTips != null && this._UserTips.Length != 0)
            {
                this._SpecialMsgSet._TxtTipDisplayInfo.SetText(this._UserTips[UnityEngine.Random.Range(0, this._UserTips.Length)].GetLocalizedString());
            }
            this._SpecialMsgSet._TxtLevelCompletedCount.SetText(this._LevelCompletedMessage.GetLocalizedString().Replace("%%", this.mLevelsCompletedCount.ToString()));
            if (this._SpecialMsgSet._TxtAccuracy != null)
            {
                this._SpecialMsgSet._TxtAccuracy.SetText(this.mAccuracy.ToString("n1") + " %");
            }
        }
        else if (this.mCurrLevelCompleteType == UiEBNextLevel.LevelCompleteType.ElectricEel)
        {
            this._SpecialMsgRoot.SetActive(true);
            this._NormalMsgRoot.SetActive(false);
            this._SpecialMsgSet._AniDisplayImage.SetTexture(this._ElectricEelImage, false, null);
            if (KAInput.pInstance.IsTouchInput())
            {
                this._SpecialMsgSet._TxtDisplayInfo.SetText(this._ElectricEelTextMobile.GetLocalizedString());
            }
            else
            {
                this._SpecialMsgSet._TxtDisplayInfo.SetText(this._ElectricEelText.GetLocalizedString());
            }
            this._SpecialMsgSet._TxtUserText.SetText(this.mCurrUserMessage);
            if (this._UserTips != null && this._UserTips.Length != 0)
            {
                this._SpecialMsgSet._TxtTipDisplayInfo.SetText(this._UserTips[UnityEngine.Random.Range(0, this._UserTips.Length)].GetLocalizedString());
            }
            this._SpecialMsgSet._TxtLevelCompletedCount.SetText(this._LevelCompletedMessage.GetLocalizedString().Replace("%%", this.mLevelsCompletedCount.ToString()));
            if (this._SpecialMsgSet._TxtAccuracy != null)
            {
                this._SpecialMsgSet._TxtAccuracy.SetText(this.mAccuracy.ToString("n1") + " %");
            }
        }
        else if (this.mCurrLevelCompleteType == UiEBNextLevel.LevelCompleteType.TerrorEffect)
        {
            this._SpecialMsgRoot.SetActive(true);
            this._NormalMsgRoot.SetActive(false);
            this._SpecialMsgSet._AniDisplayImage.SetTexture(this._TerrorImage, false, null);
            if (KAInput.pInstance.IsTouchInput())
            {
                this._SpecialMsgSet._TxtDisplayInfo.SetText(this._TerrorTextMobile.GetLocalizedString());
            }
            else
            {
                this._SpecialMsgSet._TxtDisplayInfo.SetText(this._TerrorText.GetLocalizedString());
            }
            this._SpecialMsgSet._TxtUserText.SetText(this.mCurrUserMessage);
            if (this._UserTips != null && this._UserTips.Length != 0)
            {
                this._SpecialMsgSet._TxtTipDisplayInfo.SetText(this._UserTips[UnityEngine.Random.Range(0, this._UserTips.Length)].GetLocalizedString());
            }
            this._SpecialMsgSet._TxtLevelCompletedCount.SetText(this._LevelCompletedMessage.GetLocalizedString().Replace("%%", this.mLevelsCompletedCount.ToString()));
            if (this._SpecialMsgSet._TxtAccuracy != null)
            {
                this._SpecialMsgSet._TxtAccuracy.SetText(this.mAccuracy.ToString("n1") + " %");
            }
        }
        else if (this.mCurrLevelCompleteType == UiEBNextLevel.LevelCompleteType.WaveEffect)
        {
            this._SpecialMsgRoot.SetActive(true);
            this._NormalMsgRoot.SetActive(false);
            this._SpecialMsgSet._AniDisplayImage.SetTexture(this._WaveEelImage, false, null);
            if (KAInput.pInstance.IsTouchInput())
            {
                this._SpecialMsgSet._TxtDisplayInfo.SetText(this._WaveTextMobile.GetLocalizedString());
            }
            else
            {
                this._SpecialMsgSet._TxtDisplayInfo.SetText(this._WaveText.GetLocalizedString());
            }
            this._SpecialMsgSet._TxtUserText.SetText(this.mCurrUserMessage);
            if (this._UserTips != null && this._UserTips.Length != 0)
            {
                this._SpecialMsgSet._TxtTipDisplayInfo.SetText(this._UserTips[UnityEngine.Random.Range(0, this._UserTips.Length)].GetLocalizedString());
            }
            this._SpecialMsgSet._TxtLevelCompletedCount.SetText(this._LevelCompletedMessage.GetLocalizedString().Replace("%%", this.mLevelsCompletedCount.ToString()));
            if (this._SpecialMsgSet._TxtAccuracy != null)
            {
                this._SpecialMsgSet._TxtAccuracy.SetText(this.mAccuracy.ToString("n1") + " %");
            }
        }
        this._TxtEelColorName.SetText(this.mColor._ColorNameText.GetLocalizedString());
        this._AniEelImage.color = this.mColor._ColorValue;
        this._TxtEelsRequiredCount.SetText(this.mEelsRequiredCount.ToString());
        this._TxtScore.SetText(this.mScore.ToString());
        for (int i = 0; i < this._AniLifes.Length; i++)
        {
            if (i < this.mLives)
            {
                this._AniLifes[i].SetVisibility(false);
            }
            else
            {
                this._AniLifes[i].SetVisibility(true);
            }
        }
    }

    public void UnlockType(UiEBNextLevel.LevelCompleteType type)
    {
        for (int i = 0; i < this.mUnlockedTypes.Count; i++)
        {
            if (this.mUnlockedTypes[i] == type)
            {
                return;
            }
        }
        this.mUnlockedTypes.Add(type);
    }

    public void ShowDidYouKnow(string inText)
    {
        Vector3 localPosition = this._AniDidYouKnow.transform.localPosition;
        localPosition.x = -111f;
        TweenPosition.Begin(this._AniDidYouKnow.gameObject, 0.5f, localPosition);
        this._AniDidYouKnow.SetText(inText);
    }

    public void HideDidYouKnow()
    {
        Vector3 localPosition = this._AniDidYouKnow.transform.localPosition;
        localPosition.x = 125f;
        this._AniDidYouKnow.transform.localPosition = localPosition;
    }

    public UiEBNextLevel()
    {
    }

    public KAWidget _TxtEelColorName;

    public UITexture _AniEelImage;

    public KAWidget _TxtEelsRequiredCount;

    public KAWidget _TxtScore;

    public KAWidget[] _AniLifes;

    public KAWidget _AniDidYouKnow;

    public GameObject _NormalMsgRoot;

    public GameObject _SpecialMsgRoot;

    public AudioClip _SndNextLevelMusic;

    public UiEBNextLevel.WidgetSet _NormalMsgSet;

    public UiEBNextLevel.WidgetSet _SpecialMsgSet;

    public LocaleString _BonusEelText = new LocaleString("");

    public LocaleString _ElectricEelText = new LocaleString("");

    public LocaleString _WaveText = new LocaleString("");

    public LocaleString _TerrorText = new LocaleString("");

    public LocaleString _BonusEelTextMobile = new LocaleString("");

    public LocaleString _ElectricEelTextMobile = new LocaleString("");

    public LocaleString _WaveTextMobile = new LocaleString("");

    public LocaleString _TerrorTextMobile = new LocaleString("");

    public LocaleString _LevelCompletedMessage = new LocaleString("");

    public Texture _BonusEelImage;

    public Texture _ElectricEelImage;

    public Texture _WaveEelImage;

    public Texture _TerrorImage;

    public LocaleString[] _UserTips;

    public GameObject _MsgObject;

    private List<UiEBNextLevel.LevelCompleteType> mUnlockedTypes = new List<UiEBNextLevel.LevelCompleteType>();

    private UiEBNextLevel.LevelCompleteType mCurrLevelCompleteType;

    private string mCurrUserMessage;

    private int mLevelsCompletedCount;

    private bool mDisplayMessage;

    private float mCurrNonInteractiveTime;

    private EBGameManager.EBEelColorValue mColor;

    private int mEelsRequiredCount;

    private int mScore;

    private int mLives;

    private float mAccuracy;

    public enum LevelCompleteType
    {
        None,
        Normal,
        BonusEel,
        ElectricEel,
        WaveEffect,
        TerrorEffect
    }

    [Serializable]
    public class WidgetSet
    {
        public WidgetSet()
        {
        }

        public KAWidget _TxtUserText;

        public KAWidget _AniDisplayImage;

        public KAWidget _TxtDisplayInfo;

        public KAWidget _TxtTipDisplayInfo;

        public KAWidget _TxtLevelCompletedCount;

        public KAWidget _TxtAccuracy;

        public KAWidget _YesBtn;

        public KAWidget _NoBtn;
    }
}
