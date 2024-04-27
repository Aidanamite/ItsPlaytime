using UnityEngine;

public class UiEBGameOver : KAUI
{
    public int pTotalEelRoasted
    {
        set
        {
            this.mTotalEelRoasted = value;
            this.mUpdateValues = true;
        }
    }

    public int pTotalBonusEelRoasted
    {
        set
        {
            this.mTotalBonusEelRoasted = value;
            this.mUpdateValues = true;
        }
    }

    public int pTotalScore
    {
        set
        {
            this.mTotalScore = value;
            this.mUpdateValues = true;
        }
    }

    public int pLevelsCompleted
    {
        set
        {
            this.mLevelsCompleted = value;
            this.mUpdateValues = true;
        }
    }

    public int pTotalLevels
    {
        set
        {
            this.mTotalLevels = value;
            this.mUpdateValues = true;
        }
    }

    public int pPerfectLevels
    {
        set
        {
            this.mPerfectLevels = value;
            this.mUpdateValues = true;
        }
    }

    public int pGoldEarned
    {
        get
        {
            return this.mGoldEarned;
        }
        set
        {
            this.mGoldEarned = value;
            this.mUpdateValues = true;
        }
    }

    private void OnEnable()
    {
        this.mCurrNonInteractiveTime = 3f;
        this.SetVisibility(false);
    }

    protected override void Update()
    {
        base.Update();
        if (this.mUpdateValues)
        {
            this.mUpdateValues = false;
            this.FindItem("TxtTotalRoastedinfo", true).SetText(this.mTotalEelRoasted.ToString());
            this.FindItem("TxtBonusRoastedinfo", true).SetText(this.mTotalBonusEelRoasted.ToString());
            this.FindItem("TxtTotalScoreinfo", true).SetText(this.mTotalScore.ToString());
            this.FindItem("TxtLevelsCompletedinfo", true).SetText(this.mLevelsCompleted.ToString());
            this.FindItem("TxtTotalLevelsinfo", true).SetText(this.mTotalLevels.ToString());
            this.FindItem("TxtPerfectLevelsinfo", true).SetText(this.mPerfectLevels.ToString());
            this.FindItem("TxtGoldEarnednfo", true).SetText(this.mGoldEarned.ToString());
            this.FindItem("TxtMemberBonus", true).SetVisibility(!SubscriptionInfo.pIsMember);
        }
        if (this.mCurrNonInteractiveTime > 0f)
        {
            this.mCurrNonInteractiveTime -= Time.deltaTime;
            if (this.mCurrNonInteractiveTime <= 0f)
            {
                this.SetState(KAUIState.INTERACTIVE);
                this.SetVisibility(true);
                return;
            }
            this.SetState(KAUIState.DISABLED);
            this.SetVisibility(false);
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
        if (item.name == "OKBtn")
        {
            this._MsgObject.SendMessage("OnGenericDBButtonClicked", "No");
            return;
        }
        if (item.name == "RePlayBtn")
        {
            this._MsgObject.SendMessage("OnGenericDBButtonClicked", "Replay");
        }
    }

    public UiEBGameOver()
    {
    }

    public GameObject _MsgObject;

    private int mTotalEelRoasted;

    private int mTotalBonusEelRoasted;

    private int mTotalScore;

    private int mLevelsCompleted;

    private int mTotalLevels;

    private int mPerfectLevels;

    private int mGoldEarned;

    private bool mUpdateValues;

    private float mCurrNonInteractiveTime;
}
