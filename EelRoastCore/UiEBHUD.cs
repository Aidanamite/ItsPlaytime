using System;
using UnityEngine;

[Serializable]
public class UiEBHUD : KAUI
{
    public int pScore
    {
        get
        {
            return this.mScore;
        }
        set
        {
            this.mScore = value;
        }
    }

    public int pEelsRemaining
    {
        get
        {
            return this.mEelsRemaining;
        }
        set
        {
            this.mEelsRemaining = value;
        }
    }

    public int pLifesCount
    {
        get
        {
            return this.mLifesCount;
        }
        set
        {
            this.mLifesCount = value;
        }
    }

    protected override void Start()
    {
        base.Start();
        this.mScoreTxt = this.FindItem("BtnPoints", true);
        if (this._CrossHairObj != null)
        {
            this.mCrossHair = UnityEngine.Object.Instantiate<GameObject>(this._CrossHairObj);
            this.mCrossHair.transform.parent = base.transform;
            if (KAInput.pInstance.pInputMode == KAInputMode.TOUCH)
            {
                this.mCrossHair.SetActive(false);
                return;
            }
        }
        else
        {
            UtDebug.LogError("CrossHair is not assigned in EBHUD");
        }
    }

    public void UpdateCrossHair()
    {
        bool flag = !this._CommonVariables._Paused && this._CommonVariables._Touchable;
        if (flag == this.mEnableCrossHair || !base.gameObject.activeSelf)
        {
            return;
        }
        if (this._CrossHairObj != null)
        {
            if (this.mCrossHair == null)
            {
                this.mCrossHair = UnityEngine.Object.Instantiate<GameObject>(this._CrossHairObj);
                this.mCrossHair.transform.parent = base.transform;
            }
        }
        else
        {
            UtDebug.LogError("CrossHair is not assigned in EBHUD");
        }
        this.mEnableCrossHair = flag;
        this.mCrossHair.SetActive(this.mEnableCrossHair);
        if (KAUICursorManager.pCursorManager)
        {
            KAUICursorManager.pCursorManager.SetVisibility(!this.mEnableCrossHair);
        }
        if (KAInput.pInstance.pInputMode == KAInputMode.TOUCH)
        {
            this.mCrossHair.SetActive(false);
        }
    }

    public void ShowLifes(bool Show)
    {
    }

    private void OnEnable()
    {
        if (KAUICursorManager.pCursorManager)
        {
            KAUICursorManager.pCursorManager.SetVisibility(false);
        }
        this.mChallengePoints = UtUtilities.FindChildTransform(base.gameObject, "ChallengePoints").GetComponent<KAWidget>();
    }

    private void OnDisable()
    {
        if (KAUICursorManager.pCursorManager)
        {
            KAUICursorManager.pCursorManager.SetVisibility(true);
        }
    }

    public void OnEelClicked()
    {
        this.mCrossHairRotSpeed = this._CrossHairRotSpeed;
    }

    public void SetColor(Color colorValue, string colorName)
    {
        this._EelImage.color = colorValue;
        this._EelColor.SetText(colorName);
    }

    public void UpdateChallengePoints(int points)
    {
        if (this.mChallengePoints != null)
        {
            this.mChallengePoints.SetText(points.ToString());
        }
    }

    public void ChallengeItemVisible(bool visible)
    {
        if (this.mChallengePoints != null && this.mChallengePoints.GetParentItem() != null)
        {
            this.mChallengePoints.GetParentItem().SetVisibility(visible);
        }
    }

    public bool GetChallengeItemVisibility()
    {
        return this.mChallengePoints != null && this.mChallengePoints.GetParentItem() != null && this.mChallengePoints.GetParentItem().GetVisibility();
    }

    public void FlashChallengeItem(float interval, int loopTimes)
    {
        if (this.mChallengePoints != null && this.mChallengePoints.GetParentItem() != null)
        {
            this.mChallengePointFlashTimes = loopTimes;
            this.mChallengePointFlashDuration = interval;
            this.mChallengePointFlashTimer = interval;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (this.mChallengePointFlashTimer > 0f)
        {
            this.mChallengePointFlashTimer -= Time.deltaTime;
            if (this.mChallengePointFlashTimer <= 0f)
            {
                if (this.mChallengePointFlashTimes > 0)
                {
                    bool challengeItemVisibility = this.GetChallengeItemVisibility();
                    this.ChallengeItemVisible(!challengeItemVisibility);
                    this.mChallengePointFlashTimer = this.mChallengePointFlashDuration;
                    if (!challengeItemVisibility)
                    {
                        this.mChallengePointFlashTimes--;
                    }
                }
                else
                {
                    this.ChallengeItemVisible(false);
                }
            }
        }
        this.UpdateCrossHair();
        this.mScoreTxt.SetText(this.mScore.ToString());
        this._EelCount.SetText(this.mEelsRemaining.ToString());
        for (int i = 0; i < this._AniLifes.Length; i++)
        {
            if (i < this.mLifesCount)
            {
                this._AniLifes[i].SetVisibility(true);
            }
            else
            {
                this._AniLifes[i].SetVisibility(false);
            }
        }
        this.UpdatePerfectBalance();
        if (this.mCrossHair != null && this.mEnableCrossHair)
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 position = this._Camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, this._CrossHairDepth));
            this.mCrossHair.transform.position = position;
            this.mCrossHair.transform.forward = -this._Camera.transform.forward;
            if (this.mCrossHairRotSpeed > 0f && this._CrossHairRotInterval > 0f)
            {
                this.mCrossHairRotValue += this.mCrossHairRotSpeed;
                this.mCrossHairRotSpeed -= this._CrossHairRotSpeed * Time.deltaTime / this._CrossHairRotInterval;
            }
            else
            {
                this.mCrossHairRotSpeed = 0f;
            }
            this.mCrossHair.transform.Rotate(0f, 0f, this.mCrossHairRotValue);
        }
    }

    private void UpdatePerfectBalance()
    {
        if (!this.mIsBonusRoundMsgDisplyd && this._AniBonusRoundWidget.GetVisibility())
        {
            this._AniBonusRoundWidget.SetVisibility(false);
            this.mBonusRoundCurrTime = 0f;
        }
        if (this.mIsBonusRoundMsgDisplyd)
        {
            if (!this._AniBonusRoundWidget.GetVisibility())
            {
                this._AniBonusRoundWidget.SetVisibility(true);
            }
            if (this.mBonusRoundCurrTime < this._BonusRoundTravelTime)
            {
                float num = this.mBonusRoundCurrTime / this._BonusRoundTravelTime;
                this._AniBonusRoundWidget.GetUITexture().color = new Color(1f, 1f, 1f, num);
                this._AniBonusRoundWidget.transform.position = Vector3.Lerp(this._AniBonusRoundMinMarker.transform.position, this._AniBonusRoundMaxMarker.transform.position, num);
            }
            else if (this.mBonusRoundCurrTime < this._BonusRoundTravelTime + this._BonusRoundStayTime)
            {
                this._AniBonusRoundWidget.transform.position = this._AniBonusRoundMaxMarker.transform.position;
                this._AniBonusRoundWidget.GetUITexture().color = new Color(1f, 1f, 1f, 1f);
            }
            else if (this.mBonusRoundCurrTime < this._BonusRoundTravelTime + this._BonusRoundStayTime + this._BonusRoundTravelTime)
            {
                float num = (this.mBonusRoundCurrTime - (this._BonusRoundTravelTime + this._BonusRoundStayTime)) / this._BonusRoundTravelTime;
                this._AniBonusRoundWidget.GetUITexture().color = new Color(1f, 1f, 1f, 1f - num);
                this._AniBonusRoundWidget.transform.position = Vector3.Lerp(this._AniBonusRoundMinMarker.transform.position, this._AniBonusRoundMaxMarker.transform.position, 1f - num);
            }
            else
            {
                this._AniBonusRoundWidget.transform.position = this._AniBonusRoundMinMarker.transform.position;
                this._AniBonusRoundWidget.GetUITexture().color = new Color(1f, 1f, 1f, 0f);
                this.mIsBonusRoundMsgDisplyd = false;
                this.mBonusRoundCurrTime = 0f;
            }
            this.mBonusRoundCurrTime += Time.deltaTime;
            return;
        }
        this.mBonusRoundCurrTime = 0f;
    }

    public void StartBonusLevelIntro()
    {
        this.mIsBonusRoundMsgDisplyd = true;
    }

    public bool IsBonusLevelIntroCompleted()
    {
        return !this.mIsBonusRoundMsgDisplyd;
    }

    public override void OnClick(KAWidget item)
    {
        base.OnClick(item);
        if (item.name == "BackBtn")
        {
            this._BackBtnCallBack();
        }
    }

    public UiEBHUD()
    {
    }

    public KAWidget _EelCount;

    public KAWidget _EelColor;

    public UITexture _EelImage;

    public GameObject _CrossHairObj;

    public float _CrossHairDepth = 10f;

    public float _CrossHairRotInterval = 0.5f;

    public float _CrossHairRotSpeed = 360f;

    [NonSerialized]
    public Camera _Camera;

    private KAWidget mScoreTxt;

    private GameObject mCrossHair;

    private float mCrossHairRotValue;

    private float mCrossHairRotSpeed;

    public KAWidget _AniBonusRoundWidget;

    public KAWidget _AniBonusRoundMinMarker;

    public KAWidget _AniBonusRoundMaxMarker;

    public KAWidget _AniLifesRoot;

    public KAWidget[] _AniLifes;

    public float _BonusRoundTravelTime = 0.5f;

    public float _BonusRoundStayTime = 1f;

    private bool mIsBonusRoundMsgDisplyd;

    private float mBonusRoundCurrTime;

    private bool mEnableCrossHair;

    private KAWidget mChallengePoints;

    private float mChallengePointFlashDuration;

    private float mChallengePointFlashTimer;

    private int mChallengePointFlashTimes;

    private Vector2 mBackBtnPos;

    private Vector2 mHelpBtnPos;

    private int mScore;

    private int mEelsRemaining;

    private int mLifesCount;

    public EBGameManager.UpdateFunction _BackBtnCallBack;

    public EBGameManager.EBCommonVariables _CommonVariables;
}
