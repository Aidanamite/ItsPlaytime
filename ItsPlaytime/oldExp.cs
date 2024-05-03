using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ScientificExperiment : MonoBehaviour
{
    public ScientificExperiment()
    {
    }

    public bool pMagnetActivated
    {
        get
        {
            return this.mMagnetActivated;
        }
        set
        {
            this.mMagnetActivated = value;
        }
    }

    public ParticleSystem pSplash
    {
        [CompilerGenerated]
        get
        {
            return this.< pSplash > k__BackingField;
        }
        [CompilerGenerated]
        set
        {
            this.< pSplash > k__BackingField = value;
        }
    }

    public bool pTitrationActive
    {
        get
        {
            return this.mAcidTitrationTimer > 0f || this.mBaseTitrationTimer > 0f;
        }
    }

    public LabTask pTimerActivatedTask
    {
        [CompilerGenerated]
        get
        {
            return this.< pTimerActivatedTask > k__BackingField;
        }
        [CompilerGenerated]
        set
        {
            this.< pTimerActivatedTask > k__BackingField = value;
        }
    }

    public bool pUserEnabledTimer
    {
        get
        {
            return this.pTimerActivatedTask == null;
        }
    }

    public bool pTimeEnabled
    {
        get
        {
            return this.mTimeEnabled;
        }
    }

    public GameObject pDragonFlameParticle
    {
        get
        {
            return this.mDragonFlameParticle;
        }
    }

    private ScientificExperiment.LabDragonAgeData pDragonAgeData
    {
        get
        {
            if (this.mDragonData == null)
            {
                SanctuaryPetTypeInfo typeInfo = this.mCurrentDragon.GetTypeInfo();
                if (typeInfo == null)
                {
                    return null;
                }
                this.mDragonData = this.GetDragonData(typeInfo._Name);
            }
            return this.mDragonData.GetAgeData(this.mCurrentDragon.pCurAgeData._Name);
        }
    }

    public Experiment pExperiment
    {
        get
        {
            return this.mExperiment;
        }
    }

    public ExperimentType pExperimentType
    {
        get
        {
            if (this.pExperiment != null)
            {
                return (ExperimentType)this.pExperiment.Type;
            }
            return ExperimentType.UNKNOWN;
        }
    }

    public bool pWaitingForAnimEvent
    {
        get
        {
            return this.mWaitingForAnimEvent;
        }
        set
        {
            this.mWaitingForAnimEvent = value;
        }
    }

    public static ScientificExperiment pInstance
    {
        get
        {
            return ScientificExperiment.mInstance;
        }
    }

    private bool pUsingFreezeIces
    {
        get
        {
            return this.pCrucible != null && this.pCrucible.pFreezing;
        }
    }

    public LabThermometer pThermometer
    {
        get
        {
            return this.mThermometer;
        }
    }

    public bool pShowClock
    {
        get
        {
            return this.mShowClock;
        }
        set
        {
            if (value && this.mShowClock != value)
            {
                this.ResetTime();
            }
            this.DisableTime();
            this.mShowClock = value;
            if (this._MainUI != null && this._MainUI.gameObject != null)
            {
                this._MainUI.TriggerTimerObject(this.mShowClock);
            }
        }
    }

    public float pOneClockTime
    {
        get
        {
            return 60f;
        }
    }

    public LabCrucible pCrucible
    {
        get
        {
            if (this.pExperimentType == ExperimentType.GRONCKLE_IRON && this._Gronckle != null)
            {
                return this._Gronckle.pCrucible ?? this.mCrucible;
            }
            if (this.pExperimentType == ExperimentType.SPECTRUM_LAB && this._Spectrum != null)
            {
                return this._Spectrum.pCrucible ?? this.mCrucible;
            }
            if (this.pExperimentType == ExperimentType.TITRATION_LAB && this._Titration != null)
            {
                return this._Titration.pCrucible ?? this.mCrucible;
            }
            return this.mCrucible;
        }
    }

    private bool pDragonDataInitialized
    {
        get
        {
            return this.mBreathParticleLoaded;
        }
    }

    public int pCrucibleItemCount
    {
        get
        {
            if (this.pCrucible != null && this.pCrucible.pTestItems != null)
            {
                return this.pCrucible.pTestItems.Count;
            }
            return 0;
        }
    }

    public SanctuaryPet pCurrentDragon
    {
        get
        {
            return this.mCurrentDragon;
        }
    }

    public void Start()
    {
        if (KAInput.pInstance.IsTouchInput())
        {
            this._TapTimeToStartText = this._TapTimeToStartMobText;
            this._RecordInJournalText = this._RecordInJournalMobText;
        }
        if (this._WaterStream != null)
        {
            this._WaterStream.Stop();
        }
        if (this._AcidStream != null)
        {
            this._AcidStream.emission.enabled = false;
        }
        if (this._BaseStream != null)
        {
            this._BaseStream.emission.enabled = false;
        }
        if (this._WaterSplashSteam != null)
        {
            this._WaterSplashSteam.Stop();
        }
        this.mXMLLoaded = false;
        this.mCurrentDragon = null;
        LabData.Load(new LabData.XMLLoaderCallback(this.XMLLoaded));
        this.pShowClock = false;
        ScientificExperiment.mInstance = this;
        SanctuaryManager.pInstance.pDisablePetSwitch = true;
    }

    private void XMLLoaded(bool inSuccess)
    {
        ScientificExperiment.mLabData = LabData.pInstance;
        this.mXMLLoaded = inSuccess;
    }

    private void Initialize()
    {
        if (!this.mXMLLoaded || KAUICursorManager.pCursorManager == null || this._MainUI == null || SanctuaryData.pInstance == null)
        {
            return;
        }
        if (ScientificExperiment.pUseExperimentCheat)
        {
            this.mExperiment = ScientificExperiment.mLabData.GetLabExperimentByID(ScientificExperiment.pActiveExperimentID);
        }
        else
        {
            this.mExperiment = ScientificExperiment.GetActiveExperiment();
            if (this.mExperiment == null && this.mKAUIGenericDB == null)
            {
                this.mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "NoActive");
                if (this.mKAUIGenericDB != null)
                {
                    this.mKAUIGenericDB.SetMessage(base.gameObject, string.Empty, string.Empty, string.Empty, string.Empty);
                    this.mKAUIGenericDB.SetButtonVisibility(false, false, true, false);
                    this.mKAUIGenericDB.SetText("No active scientific quest..", true);
                    this.mKAUIGenericDB.SetMessage(base.gameObject, string.Empty, string.Empty, "OnNoActiveQuestOk", string.Empty);
                    KAUI.SetExclusive(this.mKAUIGenericDB);
                }
            }
        }
        if (this.mCurrentDragon == null)
        {
            if (!this.mCreatedDragon)
            {
                if (SanctuaryManager.pCurPetInstance != null)
                {
                    if (this.mExperiment != null && this.mExperiment.Type == 1)
                    {
                        if (SanctuaryManager.pCurPetInstance.pAge == 0)
                        {
                            this.PlaceDragonAside();
                        }
                        this.InitGronckleExp();
                        this.mCreatedDragon = true;
                    }
                    else if (this.mExperiment.ForceDefaultDragon)
                    {
                        if (SanctuaryManager.pCurPetInstance.pAge == 0)
                        {
                            this.PlaceDragonAside();
                        }
                        this.CreateDragon(this.mExperiment.DragonType, ScientificExperiment.pDefaultDragonStage, ScientificExperiment.pDefaultDragonGender);
                        this.mCreatedDragon = true;
                    }
                    else if (SanctuaryManager.pCurPetInstance.pWeaponManager.GetCurrentWeapon()._AmmoType != (WeaponTuneData.AmmoType)this.mExperiment.BreathType || SanctuaryManager.pCurPetInstance.pAge == 0)
                    {
                        if (SanctuaryManager.pCurPetInstance.pAge == 0)
                        {
                            this.PlaceDragonAside();
                        }
                        this.CreateDragon(this.mExperiment.DragonType, ScientificExperiment.pDefaultDragonStage, ScientificExperiment.pDefaultDragonGender);
                        this.mCreatedDragon = true;
                    }
                    else
                    {
                        this.SetCurrentDragon(SanctuaryManager.pCurPetInstance);
                        this.mCreatedDragon = true;
                    }
                }
                else if (SanctuaryManager.pCurPetData == null)
                {
                    if (this.mExperiment != null && this.mExperiment.Type == 1)
                    {
                        this.InitGronckleExp();
                    }
                    else
                    {
                        this.CreateDragon(this.mExperiment.DragonType, ScientificExperiment.pDefaultDragonStage, ScientificExperiment.pDefaultDragonGender);
                    }
                    this.mCreatedDragon = true;
                }
            }
            if (this.mCurrentDragon == null)
            {
                return;
            }
        }
        if (!this.pDragonDataInitialized)
        {
            this.InitializeDragonData();
        }
        if (!this.mExperimentIntialized)
        {
            this.mExperimentIntialized = this._MainUI.InitializeExperiment(this.mExperiment);
        }
        if (!this.mTutorialInitialized && this.mExperimentIntialized && this._MainUI.pIsReady)
        {
            this.mCurrPlayingTutorial = null;
            if (this._TutorialList != null && this.mExperiment != null)
            {
                LabTutorial tutorial = this.GetTutorial(this.mExperiment.ID);
                if (tutorial != null)
                {
                    tutorial.gameObject.SetActive(true);
                    this.mCurrPlayingTutorial = tutorial;
                }
            }
            this.mTutorialInitialized = true;
        }
        if (this.mDragonPositioned && this.mExperimentIntialized && this.mTutorialInitialized)
        {
            if (this.mCurrPlayingTutorial != null)
            {
                ObStatus component = this.mCurrPlayingTutorial.GetComponent<ObStatus>();
                if (component != null && !component.pIsReady)
                {
                    return;
                }
            }
            this.OnLevelReady();
        }
    }

    private void InitGronckleExp()
    {
        if (this._Gronckle != null)
        {
            this._Gronckle.gameObject.SetActive(true);
            this._Gronckle.Init(this);
        }
        SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(this._GronckleId);
        string resName = string.Empty;
        int ageIndex = RaisedPetData.GetAgeIndex(ScientificExperiment.pDefaultDragonStage);
        if (sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[0]._Gender == ScientificExperiment.pDefaultDragonGender)
        {
            resName = sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[0]._Prefab;
        }
        else
        {
            resName = sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[1]._Prefab;
        }
        RaisedPetData pdata = RaisedPetData.CreateCustomizedPetData(this._GronckleId, ScientificExperiment.pDefaultDragonStage, resName, ScientificExperiment.pDefaultDragonGender, null, true);
        SanctuaryPet component = this._Gronckle.GetComponent<SanctuaryPet>();
        component.Init(pdata, false);
        this.SetCurrentDragon(component);
    }

    private void PlaceDragonAside()
    {
        SanctuaryManager.pCurPetInstance.PlayAnimation("IdleSit", WrapMode.Loop, 1f, 0.2f);
        Transform dragonMarker = this.GetDragonMarker(SanctuaryManager.pCurPetInstance.pTypeInfo._Name, SanctuaryManager.pCurPetInstance.pCurAgeData._Name, true);
        if (dragonMarker != null)
        {
            SanctuaryManager.pCurPetInstance.SetPosition(dragonMarker.position);
            SanctuaryManager.pCurPetInstance.transform.rotation = dragonMarker.rotation;
            SanctuaryManager.pCurPetInstance.transform.localScale = Vector3.one * SanctuaryManager.pCurPetInstance.pCurAgeData._LabScale;
        }
    }

    private void InitializeDragonData()
    {
        if (this.pDragonDataInitialized || this.mDragonDataInitializing || this.mCurrentDragon == null)
        {
            return;
        }
        this.mDragonDataInitializing = true;
        SanctuaryPetTypeInfo typeInfo = this.mCurrentDragon.GetTypeInfo();
        if (typeInfo != null && this.mCurrentDragon.pCurAgeData != null)
        {
            this.mDragonData = this.GetDragonData(typeInfo._Name);
            if (this.pDragonAgeData != null)
            {
                string[] array = this.pDragonAgeData._BreathFireParticleRes.Split(new char[]
                {
                    '/'
                });
                if (array.Length == 3)
                {
                    RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], new RsResourceEventHandler(this.OnBreathFireParticleDownloaded), typeof(GameObject), false, null);
                }
            }
        }
        LabDragonAnimEvents labDragonAnimEvents = this.mCurrentDragon.GetComponent<LabDragonAnimEvents>();
        if (labDragonAnimEvents == null)
        {
            labDragonAnimEvents = this.mCurrentDragon.gameObject.AddComponent<LabDragonAnimEvents>();
            if (this.mDragonData != null)
            {
                labDragonAnimEvents._Events = this.mDragonData._AnimEvents;
            }
        }
        this.PlayDragonAnim(this._IdleAnim, true, true, 1f, null);
    }

    public void Update()
    {
        if (!this.mInitialized)
        {
            this.Initialize();
        }
        else
        {
            if (!this.mExiting && MainStreetMMOClient.pInstance != null && !MainStreetMMOClient.pInstance.pAllDeactivated)
            {
                MainStreetMMOClient.pInstance.ActivateAll(false);
            }
            if (this.pUsingFreezeIces && !this._MainUI.pUserPromptOn)
            {
                if (this.mIceSetTimer < 0f)
                {
                    this.UseIceSet(false);
                }
                else
                {
                    this.mIceSetTimer -= Time.deltaTime;
                    if (this._CoolDuration - this.mIceSetTimer >= 1f)
                    {
                        this._IceSet.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.75f, 1f - this.mIceSetTimer / (this._CoolDuration - 1f));
                    }
                }
            }
            this.mTimeSinceExperimentStarted += Time.deltaTime;
            if (this.pCrucible != null)
            {
                this.pCrucible.DoUpdate();
            }
            if (this.mThermometer != null)
            {
                this.mThermometer.DoUpdate();
            }
            this.UpdateTime();
            this.UpdateAnimTimer();
            if (this.pExperimentType == ExperimentType.TITRATION_LAB)
            {
                this.UpdateTitration();
            }
            if (this.mFireAtTarget && this.mBreatheTargetMarker != null)
            {
                this.mDragonFlameParticle.transform.LookAt(this.mBreatheTargetMarker);
            }
            if (this.mCurrentDragon != null && this.mDragonData != null && this.mCurrentDragon.pHeadBonesLookAtData != null && this.mCurrentDragon.pHeadBonesLookAtData.Count > 0 && this.mCurrentDragon.pHeadBonesLookAtData[0].mHeadBone != null && this._Crucible != null && this._MainCamera != null && this.mDragonLookAtMouse)
            {
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = this._Crucible.position.z + 2f;
                Vector3 vector = this._MainCamera.ScreenToWorldPoint(mousePosition);
                Vector3 lhs = vector - this.mCurrentDragon.pHeadBonesLookAtData[0].mHeadBone.transform.position;
                lhs.Normalize();
                float num = Vector3.Dot(lhs, this.mCurrentDragon.transform.right);
                num = 90f - Mathf.Acos(num) * 57.29578f;
                float num2 = Vector3.Dot(lhs, this.mCurrentDragon.transform.up);
                num2 = 90f - Mathf.Acos(num2) * 57.29578f;
                if (num2 <= this.mDragonData._MaxTopLookAtAngle && num2 >= this.mDragonData._MaxDownLookAtAngle * -1f && num <= this.mDragonData._MaxLeftLookAtAngle && num >= this.mDragonData._MaxRightLookAtAngle * -1f)
                {
                    this.mCurrentDragon.SetLookAt(vector, true);
                }
            }
        }
    }

    private void UpdateTitration()
    {
        if (this.mBaseTitrationTimer > 0f || this.mAcidTitrationTimer > 0f)
        {
            this.mAcidTitrationTimer -= Time.deltaTime;
            this.mBaseTitrationTimer -= Time.deltaTime;
            if (this.mAcidTitrationTimer <= 0f)
            {
                this.mAcidTitrationTimer = 0f;
                if (this._AcidStream != null)
                {
                    this._AcidStream.emission.enabled = false;
                }
            }
            if (this.mBaseTitrationTimer <= 0f)
            {
                this.mBaseTitrationTimer = 0f;
                if (this._BaseStream != null)
                {
                    this._BaseStream.emission.enabled = false;
                }
            }
        }
    }

    public void ResetTime()
    {
        this.mTimer = 0f;
    }

    public void DisableTime()
    {
        if (!this.mTimeEnabled)
        {
            return;
        }
        this.CheckForProcedureHalt("Action", "Time");
        if (this._MainUI._TimerArrow != null)
        {
            this._MainUI._TimerArrow.enabled = false;
        }
        this.mTimeEnabled = false;
        SnChannel.StopPool("Timer_Pool");
        SnChannel.Play(this._TimeUpSFX, "SFX_Pool", true, null);
    }

    public void EnableTime(bool inUserActivated)
    {
        if (this.mTimeEnabled)
        {
            return;
        }
        if (this._MainUI._TimerArrow != null)
        {
            this._MainUI._TimerArrow.enabled = true;
        }
        this.mTimeEnabled = true;
        SnChannel.Play(this._TimeStartSFX, "SFX_Pool", true, null);
        SnChannel snChannel = SnChannel.Play(this._TimeTickSFX, "Timer_Pool", true, null);
        if (snChannel != null)
        {
            snChannel.pLoop = true;
        }
        if (inUserActivated)
        {
            this.pTimerActivatedTask = null;
        }
    }

    public float pTimer
    {
        get
        {
            return this.mTimer;
        }
    }

    public void UpdateTime()
    {
        if (this.mTimeEnabled && !this._MainUI.pUserPromptOn)
        {
            this.mTimer = Mathf.Min(this._MaxClockTimeInMinutes * 60f, this.mTimer + Time.deltaTime);
        }
    }

    public float pHeatTime
    {
        get
        {
            return this.mHeatTime;
        }
    }

    private LabTutorial GetTutorial(int experimentID)
    {
        ScientificExperiment.LabTutorialData labTutorialData = this._TutorialList.Find((ScientificExperiment.LabTutorialData data) => data._TaskID == experimentID);
        if (labTutorialData != null)
        {
            return labTutorialData._Tutorial;
        }
        return null;
    }

    public void OnLevelReady()
    {
        bool flag = false;
        if (this.mCurrPlayingTutorial != null)
        {
            this.mCurrPlayingTutorial.ShowTutorial();
            flag = true;
        }
        RsResourceManager.DestroyLoadScreen();
        KAUICursorManager.pVisibility = true;
        KAUICursorManager.SetDefaultCursor("Activate", true);
        if (RsResourceManager.pLastLevel != GameConfig.GetKeyData("ProfileScene") && RsResourceManager.pLastLevel != GameConfig.GetKeyData("StoreScene") && RsResourceManager.pLastLevel != GameConfig.GetKeyData("JournalScene"))
        {
            ScientificExperiment.mLastScene = RsResourceManager.pLastLevel;
        }
        if (this.mKAUIGenericDB != null)
        {
            UnityEngine.Object.Destroy(this.mKAUIGenericDB.gameObject);
        }
        this.mKAUIGenericDB = null;
        this.mHeatTime = this.GetHeatTime();
        this.mCrucible = new LabCrucible(this);
        if (this.mExperiment != null)
        {
            this.mThermometer = new LabThermometer(this.pCrucible, this.mExperiment.ThermometerMin, this.mExperiment.ThermometerMax);
        }
        else
        {
            this.mThermometer = new LabThermometer(this.pCrucible, 0f, 100f);
        }
        if (this.pExperimentType == ExperimentType.MAGNETISM_LAB)
        {
            this._MagnetGameObject.SetActive(true);
            for (int i = 0; i < this._ObjsToDisableForMagnetismLab.Length; i++)
            {
                this._ObjsToDisableForMagnetismLab[i].SetActive(false);
            }
            this.mMagnetOrgPos = this._MagnetGameObject.transform.position;
        }
        if (this.pExperimentType == ExperimentType.SPECTRUM_LAB)
        {
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(this._TestItemLayer), LayerMask.NameToLayer(this._TestItemLayer), true);
            this._SpectrumGameObject.SetActive(true);
            for (int j = 0; j < this._ObjsToDisableForSpectrumLab.Length; j++)
            {
                this._ObjsToDisableForSpectrumLab[j].SetActive(false);
            }
            if (this._SpectrumCrucible != null)
            {
                this._MainUI._Crucible = this._SpectrumCrucible;
                this._Crucible = this._SpectrumCrucible;
            }
            if (this._Spectrum != null)
            {
                this._Spectrum.gameObject.SetActive(true);
                this._Spectrum.Init(this);
            }
        }
        if (this.pExperimentType == ExperimentType.TITRATION_LAB)
        {
            this._TitrationGameObject.SetActive(true);
            for (int k = 0; k < this._ObjsToDisableForTitraionLab.Length; k++)
            {
                this._ObjsToDisableForTitraionLab[k].SetActive(false);
            }
            if (this._Titration != null)
            {
                this._Titration.Init(this);
                if (this._MainUI._TitrationWidgetGroup != null)
                {
                    this._MainUI._TitrationWidgetGroup.SetActive(true);
                }
            }
        }
        if (!flag)
        {
            this.SyncTasksWithMissionData();
        }
        this._MainUI.OnLevelReady();
        AvAvatar.SetActive(false);
        if (this._MainUI != null)
        {
            LabTask anyIncompleteTask = this.pExperiment.GetAnyIncompleteTask();
            if (anyIncompleteTask != null && !flag)
            {
                this._MainUI.ShowExperimentDirection(anyIncompleteTask);
            }
            else
            {
                this._MainUI.ShowExperimentDirection(0);
            }
        }
        this.StopDragonAnim();
        this.InitBreatheAtTargetInfo();
        this.mInitialized = true;
    }

    public void BreathFlame(bool inBreath)
    {
        if (this.pCrucible == null)
        {
            return;
        }
        if (inBreath)
        {
            if (this.pCrucible.CanHeat())
            {
                this.PlayDragonAnim(this._BreatheAnim, this.mDragonHeadTargetMarker, true);
            }
        }
        else
        {
            this.pCrucible.StopHeat();
        }
    }

    public void UseIceSet(bool inUseIceScoop)
    {
        if (this.pCrucible == null)
        {
            return;
        }
        if (inUseIceScoop)
        {
            this.pCrucible.Freeze(true);
            this.mIceSetTimer = this._CoolDuration;
            this._IceSet.transform.localScale = Vector3.one;
            return;
        }
        this.pCrucible.Freeze(false);
        this.mIceSetTimer = 0f;
    }

    public void BreatheElectricity(bool inBreathe)
    {
        if (inBreathe && !this._MainUI.pElectricFlow)
        {
            this.PlayDragonAnim(this._BreatheAnim, true, true, 1f, null);
        }
    }

    public void AddWater()
    {
        if (this.pCrucible != null)
        {
            this.pCrucible.AddWater(LabData.pInstance.GetItem("Water"));
        }
    }

    public void AddAcidity(int inUnit)
    {
        LabTitrationCrucible labTitrationCrucible = this.pCrucible as LabTitrationCrucible;
        if (labTitrationCrucible != null)
        {
            labTitrationCrucible.AddAcidity(inUnit);
        }
        if (inUnit < 0)
        {
            if (this._AcidStream != null)
            {
                this._AcidStream.emission.enabled = true;
            }
            this.mAcidTitrationTimer += this._TitrationMixTime * (float)Mathf.Abs(inUnit);
        }
        else
        {
            if (this._BaseStream != null)
            {
                this._BaseStream.emission.enabled = true;
            }
            this.mBaseTitrationTimer += this._TitrationMixTime * (float)inUnit;
        }
    }

    public void OnWaterLoaded(LabItem inLabItem, GameObject inGameObj, LabItem inParent)
    {
        this._MainUI.ActivateCursor(UiScienceExperiment.Cursor.DEFAULT);
        this.mWaterLabItem = inLabItem;
        this.mWaterGameObject = inGameObj;
        this.mWaterObjectParent = inParent;
        this.EnableUI(true);
        if (this._WaterPull != null)
        {
            this._WaterPull.Play();
        }
        this.PlayDragonAnim(this._WaterPullChainAnim, true, true, 1f, null);
    }

    private void OnClick(GameObject inGameObject)
    {
        if (this.pExperimentType != ExperimentType.TITRATION_LAB)
        {
            this.EnableClickOnPullDown(false);
            this.AddMagnet();
        }
    }

    public void AddMagnet()
    {
        if (this._WaterPull != null)
        {
            this._WaterPull.Play();
        }
        this.PlayDragonAnim(this._WaterPullChainAnim, true, true, 1f, null);
    }

    public void Reset()
    {
        if (this._MagnetGameObject != null)
        {
            this._MagnetGameObject.transform.position = this.mMagnetOrgPos;
        }
    }

    public void Pestle()
    {
        if (this.pCrucible != null)
        {
            this.pCrucible.Mix();
        }
    }

    public void EnableClickOnPullDown(bool isEnable)
    {
        if (this._RopePullGameObject != null)
        {
            ObClickable component = this._RopePullGameObject.GetComponent<ObClickable>();
            component.enabled = true;
            component._Active = isEnable;
        }
    }

    public void OnExperimentTaskDone(LabTask inExpTask)
    {
        if (inExpTask == null)
        {
            return;
        }
        if (this._MainUI != null)
        {
            this._MainUI.OnExperimentTaskDone(inExpTask);
        }
        if (this.pExperiment != null)
        {
            if (this.pExperiment.AreAllTasksDone())
            {
                SnChannel.Play(this._ExperimentCompleteSFX, "SFX_Pool", true, null);
                this._MainUI.OnExperimentCompleted();
                this.mCurrentDragon.UpdateActionMeters(PetActions.LAB, 1f, true, true);
            }
            else
            {
                SnChannel.Play(this._TaskCompletedSFX, "SFX_Pool", true, null);
            }
        }
        if (inExpTask.StopExciteOnRecordingInJournal)
        {
            if (this.mCurrentDragon.pAnimToPlay == "LabExcited")
            {
                this.StopDragonAnim();
            }
            SnChannel.StopPool("Default_Pool2");
        }
        if (ScientificExperiment.pUseExperimentCheat || MissionManager.pInstance == null)
        {
            return;
        }
        if (MissionManager.pInstance != null)
        {
            List<Task> tasks = MissionManager.pInstance.GetTasks("Action", "Name", inExpTask.Action);
            if (tasks != null)
            {
                foreach (Task task in tasks)
                {
                    task.pPayload.Set("Lab_Result", inExpTask.ResultText);
                    task.pPayload.Set("Lab_Result_ID", inExpTask.ResultTextID.ToString());
                }
            }
            MissionManager.pInstance.CheckForTaskCompletion("Action", inExpTask.Action);
            Mission mission = MissionManager.pInstance.GetMission(this.pExperiment.ID);
            if (this.pExperiment.AreAllTasksDone() && !string.IsNullOrEmpty(this.pExperiment.ResultImage) && mission != null)
            {
                object obj = null;
                RuleItemType ruleItemType = RuleItemType.Mission;
                if (MissionManagerDO.GetScientificQuestPhase(mission, RuleItemType.Mission, 4, out obj, out ruleItemType))
                {
                    Task task2 = obj as Task;
                    if (task2 != null)
                    {
                        task2.pPayload.Set("Lab_Image_Url", this.pExperiment.ResultImage);
                        task2.Save(false, null);
                    }
                }
            }
        }
    }

    public static Experiment GetActiveExperiment()
    {
        if (MissionManager.pInstance == null)
        {
            return null;
        }
        List<Task> pActiveTasks = MissionManager.pInstance.pActiveTasks;
        if (pActiveTasks == null || pActiveTasks.Count == 0)
        {
            return null;
        }
        Experiment experiment = null;
        foreach (Task task in pActiveTasks)
        {
            if (task != null && task.pData != null && task.pData.Objectives != null)
            {
                foreach (Experiment experiment2 in LabData.pInstance.Experiments)
                {
                    if (experiment2 != null && experiment2.Tasks != null && experiment2.Tasks.Length != 0)
                    {
                        foreach (LabTask labTask in experiment2.Tasks)
                        {
                            if (labTask != null && !string.IsNullOrEmpty(labTask.Action))
                            {
                                foreach (TaskObjective taskObjective in task.pData.Objectives)
                                {
                                    if (taskObjective != null)
                                    {
                                        string text = taskObjective.Get<string>("Name");
                                        if (!string.IsNullOrEmpty(text))
                                        {
                                            if (labTask.Action == text)
                                            {
                                                Experiment labExperimentByID = LabData.pInstance.GetLabExperimentByID(task._Mission.MissionID);
                                                if (experiment == null || experiment.Priority > labExperimentByID.Priority)
                                                {
                                                    experiment = labExperimentByID;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return experiment;
    }

    private void OnNoActiveQuestOk()
    {
        this.Exit();
    }

    public void Exit()
    {
        this.mExiting = true;
        this.mCurrentDragon.AIActor.SetState(AISanctuaryPetFSM.NORMAL);
        ProceduralMaterial.StopRebuilds();
        ScientificExperiment.pUseExperimentCheat = false;
        ScientificExperiment.pUseDragonCheat = false;
        this.StopDragonAnim();
        this._MainUI.SetVisibility(false);
        this._MainUI._ExperimentItemMenu.SetVisibility(false);
        if (!string.IsNullOrEmpty(this._ExitMarker))
        {
            AvAvatar.pStartLocation = this._ExitMarker;
        }
        RsResourceManager.LoadLevel(ScientificExperiment.mLastScene, -1f);
        if (this.mDragonFlameParticle != null)
        {
            UtDebug.Log("Lab: Destroying the particle " + this.mDragonFlameParticle.name, 8U);
            this.mDragonFlameParticle.transform.parent = null;
            UnityEngine.Object.Destroy(this.mDragonFlameParticle);
        }
        if (this.mCrucible != null)
        {
            this.mCrucible.OnExit();
        }
        LabData.pInstance = null;
        if (MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.pAllDeactivated)
        {
            MainStreetMMOClient.pInstance.ActivateAll(true);
        }
    }

    private void SyncTasksWithMissionData()
    {
        if (this.mExperiment == null || MissionManager.pInstance == null || !MissionManager.pIsReady)
        {
            return;
        }
        Mission mission = MissionManager.pInstance.GetMission(this.mExperiment.ID);
        if (mission == null || mission.Tasks == null || mission.Tasks.Count == 0)
        {
            return;
        }
        foreach (LabTask labTask in this.mExperiment.Tasks)
        {
            if (labTask != null)
            {
                if (ScientificExperiment.pUseExperimentCheat)
                {
                    labTask.pDone = false;
                }
                else if (!MissionManager.IsTaskActive("Action", "Name", labTask.Action))
                {
                    labTask.pDone = true;
                }
            }
        }
    }

    public static LabTool GetMappedLabTool(string inTool)
    {
        if (string.IsNullOrEmpty(inTool))
        {
            return LabTool.NONE;
        }
        if (inTool != null)
        {
            if (inTool == "CLOCK")
            {
                return LabTool.CLOCK;
            }
            if (inTool == "THERMOMETER")
            {
                return LabTool.THERMOMETER;
            }
            if (inTool == "WEIGHINGMACHINE")
            {
                return LabTool.WEIGHINGMACHINE;
            }
            if (inTool == "OHMMETER")
            {
                return LabTool.OHMMETER;
            }
        }
        return LabTool.NONE;
    }

    public void EnableUI(bool inEnable)
    {
        if (this._MainUI != null)
        {
            this._MainUI.SetInteractive(inEnable);
            if (this._MainUI._ExperimentItemMenu != null)
            {
                this._MainUI._ExperimentItemMenu.SetInteractive(inEnable);
            }
        }
    }

    public static void CopyProceduralMaterialProperites(ProceduralMaterial inSource, ProceduralMaterial inDest)
    {
        if (inSource == null || inDest == null)
        {
            return;
        }
        ProceduralPropertyDescription[] proceduralPropertyDescriptions = inSource.GetProceduralPropertyDescriptions();
        if (proceduralPropertyDescriptions == null || proceduralPropertyDescriptions.Length == 0)
        {
            return;
        }
        foreach (ProceduralPropertyDescription proceduralPropertyDescription in proceduralPropertyDescriptions)
        {
            if (proceduralPropertyDescription != null && inDest.HasProceduralProperty(proceduralPropertyDescription.name))
            {
                switch (proceduralPropertyDescription.type)
                {
                    case ProceduralPropertyType.Boolean:
                        inDest.SetProceduralBoolean(proceduralPropertyDescription.name, inSource.GetProceduralBoolean(proceduralPropertyDescription.name));
                        break;
                    case ProceduralPropertyType.Float:
                        inDest.SetProceduralFloat(proceduralPropertyDescription.name, inSource.GetProceduralFloat(proceduralPropertyDescription.name));
                        break;
                    case ProceduralPropertyType.Vector2:
                    case ProceduralPropertyType.Vector3:
                    case ProceduralPropertyType.Vector4:
                        inDest.SetProceduralVector(proceduralPropertyDescription.name, inSource.GetProceduralVector(proceduralPropertyDescription.name));
                        break;
                    case ProceduralPropertyType.Color3:
                    case ProceduralPropertyType.Color4:
                        inDest.SetProceduralColor(proceduralPropertyDescription.name, inSource.GetProceduralColor(proceduralPropertyDescription.name));
                        break;
                    case ProceduralPropertyType.Enum:
                        inDest.SetProceduralEnum(proceduralPropertyDescription.name, inSource.GetProceduralEnum(proceduralPropertyDescription.name));
                        break;
                    case ProceduralPropertyType.Texture:
                        inDest.SetProceduralTexture(proceduralPropertyDescription.name, inSource.GetProceduralTexture(proceduralPropertyDescription.name));
                        break;
                }
            }
        }
    }

    private void UpdateAnimTimer()
    {
        if (this.mAnimTimer == -999f)
        {
            return;
        }
        this.mAnimTimer -= Time.deltaTime;
        if (this.mAnimTimer <= 0f)
        {
            this.StopDragonAnim();
        }
    }

    public void StopDragonAnim()
    {
        this.mAnimTimer = -999f;
        if (this.mCurrentDragon != null)
        {
            this.mCurrentDragon.pAnimToPlay = this._IdleAnim;
            this.mDragonLookAtMouse = true;
        }
    }

    public float GetAnimLength(string inAnimName)
    {
        if (this.mCurrentDragon != null && !string.IsNullOrEmpty(inAnimName) && this.mCurrentDragon.animation != null && this.mCurrentDragon.animation[inAnimName] != null)
        {
            return this.mCurrentDragon.animation[inAnimName].length;
        }
        return -1f;
    }

    public bool PlayDragonAnim(string inAnimName, bool inPlayOnce = false, bool playIdleNext = true, float animSpeed = 1f, Transform lookAtObject = null)
    {
        if (this.mCurrentDragon != null && !string.IsNullOrEmpty(inAnimName) && this.mCurrentDragon.animation != null && this.mCurrentDragon.animation[inAnimName] != null && this.mCurrentDragon.pAnimToPlay != inAnimName && !this.pWaitingForAnimEvent)
        {
            this.StopDragonAnim();
            if (inPlayOnce)
            {
                this.mCurrentDragon.animation[inAnimName].time = 0f;
                if (playIdleNext)
                {
                    this.mAnimTimer = this.mCurrentDragon.animation[inAnimName].length;
                }
            }
            this.mCurrentDragon.animation[inAnimName].speed = animSpeed;
            this.mCurrentDragon.pAnimToPlay = inAnimName;
            this.mDragonLookAtMouse = false;
            this.mCurrentDragon.SetLookAtObject(lookAtObject, true, Vector3.zero);
            this.mCurrentDragon.SendMessage("StartAnimTrigger", inAnimName, SendMessageOptions.DontRequireReceiver);
            return true;
        }
        return false;
    }

    public bool PlayDragonAnim(string inAnimName, Transform lookAtObject, bool inPlayOnce = false)
    {
        return this.PlayDragonAnim(inAnimName, inPlayOnce, true, 1f, lookAtObject);
    }

    private ScientificExperiment.LabDragonData GetDragonData(string inDragonTypeName)
    {
        foreach (ScientificExperiment.LabDragonData labDragonData in this._DragonData)
        {
            if (labDragonData != null && labDragonData._Name == inDragonTypeName)
            {
                return labDragonData;
            }
        }
        return null;
    }

    private void OnBreathFireParticleDownloaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
    {
        if (inEvent != RsResourceLoadEvent.COMPLETE)
        {
            if (inEvent == RsResourceLoadEvent.ERROR)
            {
                UtDebug.Log("Lab: the particle bundle loaded error " + inURL, 8U);
                this.mBreathParticleLoaded = true;
            }
        }
        else
        {
            Transform transform = UtUtilities.FindChildTransform(this.mCurrentDragon.gameObject, this.pDragonAgeData._Bone);
            if (transform != null)
            {
                this.mDragonFlameParticle = UnityEngine.Object.Instantiate<GameObject>((GameObject)inObject);
                UtDebug.Log("Lab: Instantiated particle " + inURL, 8U);
                this.PlayParticle(this.mDragonFlameParticle, false);
                this.mDragonFlameParticle.transform.parent = transform;
                this.mDragonFlameParticle.transform.localPosition = this.pDragonAgeData._OffsetPos;
                this.mDragonFlameParticle.transform.localRotation = Quaternion.Euler(this.pDragonAgeData._OffsetRotation);
            }
            else
            {
                UtDebug.Log("Lab: The parent is null " + this.pDragonAgeData._Bone, 8U);
            }
            this.mBreathParticleLoaded = true;
        }
    }

    private void PlayParticle(GameObject inObj, bool inPlay)
    {
        UtDebug.Log("Lab: Play particle ", 8U);
        if (inObj == null)
        {
            UtDebug.Log("Lab: Error!! Unable to play the particle ", 8U);
            return;
        }
        ParticleSystem component = inObj.GetComponent<ParticleSystem>();
        if (component != null)
        {
            UtDebug.Log(string.Concat(new object[]
            {
                "Lab: Emitting particle ",
                inObj.name,
                " - ",
                inPlay
            }), 8U);
            this.mFireAtTarget = inPlay;
            if (inPlay)
            {
                component.Play(true);
            }
            else
            {
                component.Stop(true);
            }
        }
    }

    private void OnAnimEvent(AvAvatarAnimEvent inEvent)
    {
        if (inEvent == null || inEvent.mData == null)
        {
            return;
        }
        string dataString = inEvent.mData._DataString;
        if (dataString != null)
        {
            if (!(dataString == "BlowFire"))
            {
                if (!(dataString == "StopFire"))
                {
                    if (!(dataString == "BreatheElectricity"))
                    {
                        if (!(dataString == "StopElectricity"))
                        {
                            if (!(dataString == "PourWater"))
                            {
                                if (dataString == "Positioned")
                                {
                                    this.mDragonPositioned = true;
                                }
                            }
                            else if (this.pExperimentType == ExperimentType.MAGNETISM_LAB)
                            {
                                Vector3 pos = this.mMagnetOrgPos + this._MagnetTargetOffset;
                                TweenPosition tweenPosition = TweenPosition.Begin(this._MagnetGameObject.gameObject, this._MagnetMoveTime, pos);
                                tweenPosition.eventReceiver = base.gameObject;
                                tweenPosition.callWhenFinished = "MagnetTweenDone";
                            }
                            else if (this._WaterStream != null)
                            {
                                this._WaterStream.Play();
                                if (this._AddWaterSFX != null)
                                {
                                    SnChannel.Play(this._AddWaterSFX, "Default_Pool4", true, null);
                                }
                                if (this._WaterSplashSteam != null)
                                {
                                    this._WaterSplashSteam.Play();
                                }
                                this.pCrucible.AddWaterReal(this.mWaterLabItem, this.mWaterGameObject, this.mWaterObjectParent);
                            }
                        }
                        else
                        {
                            if (this.mDragonFlameParticle != null)
                            {
                                this.PlayParticle(this.mDragonFlameParticle, false);
                            }
                            this._MainUI.pElectricFlow = false;
                        }
                    }
                    else
                    {
                        this.PlayParticle(this.mDragonFlameParticle, true);
                        SnChannel snChannel = SnChannel.Play(this._SkrillBreatheSFX, "Default_Pool3", true, null);
                        snChannel.pLoop = false;
                        this._MainUI.pElectricFlow = true;
                    }
                }
                else
                {
                    if (this.mDragonFlameParticle != null)
                    {
                        this.PlayParticle(this.mDragonFlameParticle, false);
                    }
                    this.pCrucible.StopHeat();
                }
            }
            else
            {
                this.PlayParticle(this.mDragonFlameParticle, true);
                this.pCrucible.Heat();
                SnChannel snChannel = SnChannel.Play(this._DragonFireSFX, "Default_Pool3", true, null);
                snChannel.pLoop = false;
            }
        }
    }

    private void MagnetTweenDone()
    {
        if (this.mCrucible.pTestItems != null)
        {
            bool flag = false;
            for (int i = 0; i < this.mCrucible.pTestItems.Count; i++)
            {
                if (this.IsMagneticObject(this.mCrucible.pTestItems[i]))
                {
                    PendulumEffect component = this.mCrucible.pTestItems[i].gameObject.GetComponent<PendulumEffect>();
                    component.enabled = true;
                    flag = true;
                }
            }
            if (flag)
            {
                Animation component2 = this._MagnetGameObject.GetComponent<Animation>();
                component2.Play("Attract");
                base.Invoke("OnAttractAnimEnd", component2.clip.length / 2f);
            }
            else
            {
                this.SetMagnetTutorialComplete();
                this.mMagnetActivated = true;
            }
        }
    }

    private void OnAttractAnimEnd()
    {
        for (int i = 0; i < this.mCrucible.pTestItems.Count; i++)
        {
            LabTestObject labTestObject = this.mCrucible.pTestItems[i];
            if (this.IsMagneticObject(labTestObject))
            {
                PendulumEffect component = labTestObject.gameObject.GetComponent<PendulumEffect>();
                component.enabled = false;
                AttractableObject component2 = labTestObject.GetComponent<AttractableObject>();
                component2._AttractiveGameObj = this._MagnetAttachmentNode;
                component2._OnAttractiveObjHit = new AttractableObject.TargetGameObjectHit(this.OnTargetObjectHit);
                component2.enabled = true;
            }
        }
    }

    private void OnTargetObjectHit(GameObject inSourceObject, GameObject inTargetObject)
    {
        if (inTargetObject.gameObject.name == "AttractiveNode")
        {
            this.SetMagnetTutorialComplete();
            this.mMagnetActivated = true;
        }
    }

    private void SetMagnetTutorialComplete()
    {
        if (this.mCurrPlayingTutorial != null)
        {
            this.mCurrPlayingTutorial.TutorialManagerAsyncMessage("ObjectToMagnetTutComplete");
        }
    }

    private bool IsMagneticObject(LabTestObject inLabTestObject)
    {
        string propertyValueForKey = inLabTestObject.pTestItem.GetPropertyValueForKey("IsMagnetic");
        bool flag;
        return !string.IsNullOrEmpty(propertyValueForKey) && bool.TryParse(propertyValueForKey, out flag) && flag;
    }

    private void CreateDragon(int inPetTypeID, RaisedPetStage inStage, Gender inGender)
    {
        SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(inPetTypeID);
        string resName = string.Empty;
        int ageIndex = RaisedPetData.GetAgeIndex(inStage);
        if (sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[0]._Gender == inGender)
        {
            resName = sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[0]._Prefab;
        }
        else
        {
            resName = sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[1]._Prefab;
        }
        RaisedPetData pdata = RaisedPetData.InitDefault(inPetTypeID, inStage, resName, inGender, false);
        Transform dragonMarker = this.GetDragonMarker(sanctuaryPetTypeInfo._Name, sanctuaryPetTypeInfo._AgeData[ageIndex]._Name, false);
        if (dragonMarker == null)
        {
            SanctuaryManager.CreatePet(pdata, Vector3.zero, Quaternion.identity, base.gameObject, "Full");
        }
        else
        {
            SanctuaryManager.CreatePet(pdata, dragonMarker.position, dragonMarker.rotation, base.gameObject, "Full");
        }
    }

    public void OnPetReady(SanctuaryPet pet)
    {
        if (pet != null)
        {
            this.SetCurrentDragon(pet);
        }
    }

    private Transform GetDragonMarker(string inTypeName, string inAge, bool getIdlePetMarker = false)
    {
        foreach (ScientificExperiment.LabDragonData labDragonData in this._DragonData)
        {
            if (labDragonData._Name == inTypeName)
            {
                ScientificExperiment.LabDragonAgeData[] ageData = labDragonData._AgeData;
                int j = 0;
                while (j < ageData.Length)
                {
                    ScientificExperiment.LabDragonAgeData labDragonAgeData = ageData[j];
                    if (labDragonAgeData._Name == inAge)
                    {
                        if (getIdlePetMarker)
                        {
                            return labDragonAgeData._IdlePetMarker;
                        }
                        foreach (ScientificExperiment.MarkerMap markerMap2 in labDragonAgeData._MarkerMap)
                        {
                            if (markerMap2._Experiment == this.pExperimentType && markerMap2._Marker != null)
                            {
                                return markerMap2._Marker;
                            }
                        }
                        return labDragonAgeData._Marker;
                    }
                    else
                    {
                        j++;
                    }
                }
                break;
            }
        }
        return null;
    }

    private void InitBreatheAtTargetInfo()
    {
        foreach (ScientificExperiment.BreatheInfo breatheInfo2 in this._BreatheInfo)
        {
            if (breatheInfo2._Experiment == this.pExperimentType)
            {
                if (breatheInfo2._MarkerDragonHeadTarget != null)
                {
                    this.mDragonHeadTargetMarker = breatheInfo2._MarkerDragonHeadTarget;
                }
                if (breatheInfo2._MarkerBreatheTarget != null)
                {
                    this.mBreatheTargetMarker = breatheInfo2._MarkerBreatheTarget;
                }
                if (breatheInfo2._BreatheAnimation != null)
                {
                    this._BreatheAnim = breatheInfo2._BreatheAnimation;
                }
            }
        }
    }

    public ScientificExperiment.LabSubstanceMap GetProceduralMaterial(string inName)
    {
        if (this._SubstanceMap == null || this._SubstanceMap.Length == 0 || string.IsNullOrEmpty(inName))
        {
            return null;
        }
        foreach (ScientificExperiment.LabSubstanceMap labSubstanceMap in this._SubstanceMap)
        {
            if (labSubstanceMap != null && labSubstanceMap._TestItemName == inName)
            {
                return labSubstanceMap;
            }
        }
        return null;
    }

    private float GetHeatTime()
    {
        if (this.mCurrentDragon == null)
        {
            return 1.5f;
        }
        LabDragonAnimEvents component = this.mCurrentDragon.GetComponent<LabDragonAnimEvents>();
        if (component == null)
        {
            return 1.5f;
        }
        float num = -1f;
        float num2 = -1f;
        foreach (AvAvatarAnimEvent avAvatarAnimEvent in component._Events)
        {
            if (avAvatarAnimEvent != null && !(avAvatarAnimEvent._Animation != this._BreatheAnim) && avAvatarAnimEvent._Times != null && avAvatarAnimEvent._Times.Length >= 2)
            {
                foreach (AnimData animData in avAvatarAnimEvent._Times)
                {
                    if (animData != null)
                    {
                        if (animData._DataString == "BlowFire")
                        {
                            num = animData._Time;
                        }
                        else if (animData._DataString == "StopFire")
                        {
                            num2 = animData._Time;
                        }
                        if (num != -1f && num2 != -1f)
                        {
                            float num3 = num2 - num;
                            return (num3 >= 0f) ? num3 : 1.5f;
                        }
                    }
                }
                return 1.5f;
            }
        }
        return 1.5f;
    }

    private void SetCurrentDragon(SanctuaryPet inDragon)
    {
        this.mCurrentDragon = inDragon;
        if (this.mCurrentDragon != null)
        {
            if (this._PetSkinMapping != null && this.mCurrentDragon != SanctuaryManager.pCurPetInstance)
            {
                for (int i = 0; i < this._PetSkinMapping.Count; i++)
                {
                    if (this.mCurrentDragon.GetTypeInfo()._TypeID == this._PetSkinMapping[i]._PetTypeID)
                    {
                        this.mCurrentDragon.SetAccessory(RaisedPetAccType.Materials, this._PetSkinMapping[i]._Skin.gameObject, null);
                        break;
                    }
                }
            }
            if (this.mCurrentDragon._PlayMoodParticleInLab)
            {
                this.mCurrentDragon.PlayPetMoodParticle(SanctuaryPetMeterType.HAPPINESS, false);
            }
            if (this.mCurrentDragon.collider != null)
            {
                this.mCurrentDragon.collider.enabled = false;
            }
            if (this._DragonMarkers != null)
            {
                Transform dragonMarker = this.GetDragonMarker(this.mCurrentDragon.pTypeInfo._Name, this.mCurrentDragon.pCurAgeData._Name, false);
                if (dragonMarker != null)
                {
                    this.mCurrentDragon.SetPosition(dragonMarker.position);
                    this.mCurrentDragon.transform.rotation = dragonMarker.rotation;
                }
                else
                {
                    this.mCurrentDragon.SetPosition(Vector3.zero);
                    this.mCurrentDragon.transform.rotation = Quaternion.identity;
                }
                this.mCurrentDragon.transform.localScale = Vector3.one * this.mCurrentDragon.pCurAgeData._LabScale;
            }
            this.mCurrentDragon.SetState(Character_State.unknown);
            if (this.mCurrentDragon.AIActor != null)
            {
                this.mCurrentDragon.AIActor.SetState(AISanctuaryPetFSM.SCIENCE_LAB);
            }
        }
    }

    public bool CheckForProcedureHalt(string inName, string inValue)
    {
        if (this.pTimeEnabled && this.pTimerActivatedTask != null && this.pTimerActivatedTask.NeedHalt(inName, inValue))
        {
            this._MainUI.ShowUserPromptText(this._ProcedureHalted.GetLocalizedString(), true, null, false, true, null);
            this.pTimerActivatedTask = null;
            return true;
        }
        return false;
    }

    public static bool IsSolid(LabItemCategory inCategory)
    {
        return inCategory == LabItemCategory.SOLID || inCategory == LabItemCategory.SOLID_COMBUSTIBLE || inCategory == LabItemCategory.SOLID_POWDER;
    }

    private void OnDestroy()
    {
        this._SubstanceMap = null;
        if (SanctuaryManager.pInstance != null)
        {
            SanctuaryManager.pInstance.pDisablePetSwitch = false;
        }
    }

    public void OpenJournal(bool open)
    {
        if (this._Gronckle && this.pExperimentType == ExperimentType.GRONCKLE_IRON && this._Gronckle.pBellyItemCount > 0)
        {
            this._Gronckle.SetBellyPopup(!open);
        }
    }

    public void ShowRemoveFx(Transform pivot)
    {
        if (this._RemoveTestItemFx == null || pivot == null)
        {
            return;
        }
        ParticleSystem particleSystem = UnityEngine.Object.Instantiate<ParticleSystem>(this._RemoveTestItemFx, pivot.position, pivot.rotation);
        if (particleSystem != null)
        {
            particleSystem.Play();
            UnityEngine.Object.Destroy(particleSystem.gameObject, particleSystem.main.startDelay.constant + particleSystem.main.duration);
        }
    }

    // Note: this type is marked as 'beforefieldinit'.
    static ScientificExperiment()
    {
    }

    public const uint LOG_MASK = 8U;

    private const float DEFAULT_HEATTIME = 1.5f;

    public UiScienceExperiment _MainUI;

    public string _TestItemLayer = "DraggedObject";

    public string _ExitMarker;

    public float _CoolDuration = 2f;

    public Transform _KillMarker;

    public Transform _TestItemResetMarker;

    public float _CrucibleRadius = 0.35f;

    public int _MaxNumItemsAllowedInCrucible = 10;

    public LocaleString _TapTimeToStartText = new LocaleString("Click timer to start");

    public LocaleString _TapTimeToStartMobText = new LocaleString("Tap timer to start");

    public LocaleString _RecordInJournalText = new LocaleString("Click to record your observation in the Journal.");

    public LocaleString _RecordInJournalMobText = new LocaleString("Tap to record your observation in the Journal.");

    public LocaleString _ProcedureHalted = new LocaleString("Procedure halted.");

    public LocaleString _TitrationBaseNeutralText = new LocaleString("You've neutralized the {{ITEM}} by adding {{ACIDITY}} base droplets!");

    public LocaleString _TitrationAcidNeutralText = new LocaleString("You've neutralized the {{ITEM}} by adding {{ACIDITY}} acid droplets!");

    public float _DragonHeatTemperature = 50f;

    public float _IceScoopTemeprature = -50f;

    public float _DragonHeatMultiplier = 1f;

    public float _DragonCoolMultiplier = -1f;

    public GameObject _IceSet;

    public Collider _IceBox;

    public GameObject _IceOnCursor;

    public Transform _DragonMarkers;

    public float _WateringTime = 5f;

    public Transform[] _CrucibleMarkers;

    public Transform[] _SolidPowderMarkers;

    public ScientificExperiment.BreatheInfo[] _BreatheInfo;

    public float _CoolingConstant;

    public float _WarmingConstant;

    public float _FreezeRate;

    public float _WeighingMachineSpeed = 25f;

    public float _WeighingMachineBrake = 250f;

    public float _DefaultMaxWeight = 1000f;

    public float _WeighMachineLength = 1000f;

    public float _TemperatureResetTime = 3f;

    public ParticleSystem _WaterStream;

    public ParticleSystem _AcidStream;

    public ParticleSystem _BaseStream;

    public ParticleSystem _WaterSplashSteam;

    public Animation _WaterPull;

    public Vector3 _LiquidItemDefaultPos = new Vector3(0f, 0.862f, 0f);

    public float _ScaleTime = 5f;

    public Transform _ToolboxTrigger;

    public Transform _Toolbox;

    public Transform _Crucible;

    public Transform _SpectrumCrucible;

    public Transform _CrucibleTriggerSmall;

    public Transform _CrucibleTriggerBig;

    public Camera _MainCamera;

    public ScientificExperiment.LabDragonData[] _DragonData;

    public float _TestItemLifeTimeOnFloor = 6f;

    public LabGronckle _Gronckle;

    public LabSpectrum _Spectrum;

    public LabTitration _Titration;

    public int _GronckleId = 13;

    public ScientificExperiment.LabSubstanceMap[] _SubstanceMap;

    public Material _ShaderMaterial;

    public LabTutorial _Tutorial;

    public List<ScientificExperiment.LabTutorialData> _TutorialList;

    public List<ScientificExperiment.PetSkinMapping> _PetSkinMapping;

    public AudioClip _DragonFireSFX;

    public AudioClip _SkrillBreatheSFX;

    public AudioClip _AddWaterSFX;

    public AudioClip _CrucibleClickSFX;

    public AudioClip _TitrationClickSFX;

    public AudioClip _DragonExcitedSFX;

    public AudioClip _TaskCompletedSFX;

    public AudioClip _DragonDropSolidSFX;

    public AudioClip _ExperimentCompleteSFX;

    public AudioClip _FlameStartSFX;

    public AudioClip _FlameSFX;

    public AudioClip _IceButtonSFX;

    public AudioClip _IceDropSFX;

    public AudioClip _IceMeltSFX;

    public AudioClip _IceScoopSFX;

    public AudioClip _RemoveToolSFX;

    public AudioClip _SelectToolSFX;

    public AudioClip _TimeStartSFX;

    public AudioClip _TimeTickSFX;

    public AudioClip _TimeUpSFX;

    public AudioClip _ToolboxClickSFX;

    public AudioClip _WaterSteamSFX;

    public AudioClip _WaterSteamSuddenSFX;

    public AudioClip _PickupSFX;

    public AudioClip _ApprovalSFX;

    public AudioClip _DragonDisapprovalSFX;

    public AudioClip[] _SolidMoveSFX;

    public AudioClip[] _LiquidMoveSFX;

    public float _MinTemperartureToStartCooldownSFX = 160f;

    public string _WaterItemName = "Water";

    public string _WaterPullChainAnim = "LabPullChain";

    public string _BreatheAnim = "LabBlowFire";

    public string _IdleAnim = "LabIdle";

    public GameObject _MixingEffect;

    public ParticleSystem _Splash;

    public float _MaxClockTimeInMinutes = 99f;

    public Transform _ColliderGroup;

    public ParticleSystem _RemoveTestItemFx;

    public GameObject _RopePullGameObject;

    public GameObject _MagnetGameObject;

    public GameObject _SpectrumGameObject;

    public GameObject _TitrationGameObject;

    public GameObject _MagnetAttachmentNode;

    public GameObject[] _ObjsToDisableForMagnetismLab;

    public GameObject[] _ObjsToDisableForSpectrumLab;

    public GameObject[] _ObjsToDisableForTitraionLab;

    public float _TitrationMixTime = 2f;

    public float _MagnetMoveTime = 0.6f;

    public Vector3 _MagnetTargetOffset = new Vector3(0f, -0.01f, 0f);

    private Vector3 mMagnetOrgPos;

    private bool mMagnetActivated;

    private float mTimeSinceExperimentStarted;

    private bool mShowClock;

    private bool mFireAtTarget;

    private float mAcidTitrationTimer;

    private float mBaseTitrationTimer;

    private Experiment mExperiment;

    private LabCrucible mCrucible;

    private LabThermometer mThermometer;

    private bool mInitialized;

    private float mIceSetTimer;

    private KAUIGenericDB mKAUIGenericDB;

    private bool mXMLLoaded;

    [CompilerGenerated]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ParticleSystem<pSplash> k__BackingField;

    private static LabData mLabData;

    private bool mTimeEnabled = true;

    private bool mDragonPositioned;

    private bool mExperimentIntialized;

    private bool mTutorialInitialized;

    private Transform mBreatheTargetMarker;

    private Transform mDragonHeadTargetMarker;

    private LabTutorial mCurrPlayingTutorial;

    private static string mLastScene = string.Empty;

    [CompilerGenerated]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private LabTask<pTimerActivatedTask> k__BackingField;

    private float mTimer;

    private SanctuaryPet mCurrentDragon;

    private const float INVALID_ANIM_TIMER = -999f;

    private LabItem mWaterLabItem;

    private GameObject mWaterGameObject;

    private LabItem mWaterObjectParent;

    private float mAnimTimer;

    private GameObject mDragonFlameParticle;

    private bool mDragonDataInitializing;

    private bool mBreathParticleLoaded;

    private bool mDragonLookAtMouse = true;

    private ScientificExperiment.LabDragonData mDragonData;

    private bool mExiting;

    private bool mCreatedDragon;

    private bool mWaitingForAnimEvent;

    public static int pActiveExperimentID = 1009;

    public static bool pUseExperimentCheat = false;

    private static ScientificExperiment mInstance = null;

    public static bool pUseDragonCheat = false;

    public static int pDefaultDragonID = 17;

    public static RaisedPetStage pDefaultDragonStage = RaisedPetStage.ADULT;

    public static Gender pDefaultDragonGender = Gender.Male;

    private float mHeatTime;

    [Serializable]
    public class LabItemProceduralMat
    {
        public LabItemProceduralMat()
        {
        }

        public string _ItemName;

        public ProceduralMaterial _ProceduralMat;
    }

    [Serializable]
    public class LabDragonData
    {
        public LabDragonData()
        {
        }

        public ScientificExperiment.LabDragonAgeData GetAgeData(string inAgeName)
        {
            foreach (ScientificExperiment.LabDragonAgeData labDragonAgeData in this._AgeData)
            {
                if (labDragonAgeData != null && labDragonAgeData._Name == inAgeName)
                {
                    return labDragonAgeData;
                }
            }
            return null;
        }

        public string _Name;

        public ScientificExperiment.LabDragonAgeData[] _AgeData;

        public AvAvatarAnimEvent[] _AnimEvents;

        public float _MaxTopLookAtAngle = 100f;

        public float _MaxDownLookAtAngle = 100f;

        public float _MaxLeftLookAtAngle = 100f;

        public float _MaxRightLookAtAngle = 100f;
    }

    [Serializable]
    public class LabDragonAgeData
    {
        public LabDragonAgeData()
        {
        }

        public string _Name;

        public string _BreathFireParticleRes;

        public string _Bone = "ShootingPoint";

        public Vector3 _OffsetPos;

        public Vector3 _OffsetRotation;

        public ScientificExperiment.MarkerMap[] _MarkerMap;

        public Transform _Marker;

        public Transform _IdlePetMarker;
    }

    [Serializable]
    public class MarkerMap
    {
        public MarkerMap()
        {
        }

        public ExperimentType _Experiment;

        public Transform _Marker;
    }

    [Serializable]
    public class LabSubstanceMap
    {
        public LabSubstanceMap()
        {
        }

        public string _TestItemName;

        public ProceduralMaterial _Substance;

        public string _MaterialName;
    }

    [Serializable]
    public class LabTutorialData
    {
        public LabTutorialData()
        {
        }

        public int _TaskID;

        public LabTutorial _Tutorial;
    }

    [Serializable]
    public class PetSkinMapping
    {
        public PetSkinMapping()
        {
        }

        public int _PetTypeID;

        [Tooltip("Skin will not be applied to Player's Current Pet")]
        public DragonSkin _Skin;
    }

    [Serializable]
    public class BreatheInfo
    {
        public BreatheInfo()
        {
        }

        public ExperimentType _Experiment;

        public Transform _MarkerDragonHeadTarget;

        public Transform _MarkerBreatheTarget;

        public string _BreatheAnimation;
    }

    [CompilerGenerated]
    private sealed class <GetTutorial>c__AnonStorey0
	{
		public <GetTutorial>c__AnonStorey0()
    {
    }

    internal bool <>m__0(ScientificExperiment.LabTutorialData data)
    {
        return data._TaskID == this.experimentID;
    }

    internal int experimentID;
}
}
