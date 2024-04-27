using System;
using System.Collections.Generic;
using UnityEngine;

public class EBGameManager : MonoBehaviour
{
    public GameObject pDragonFlameParticle
    {
        get
        {
            return mDragonFlameParticle;
        }
    }

    public bool pIsJumping
    {
        get
        {
            return mIsJumping;
        }
        set
        {
            mIsJumping = value;
        }
    }

    public bool pIsDucking
    {
        get
        {
            return mIsDucking;
        }
        set
        {
            mIsDucking = value;
        }
    }

    public void Initialize(SanctuaryPet pet)
    {
        if (mInitialized)
        {
            UtDebug.LogError("Eel Blast Double initialization", 10);
        }
        _CurrentPet = pet;
        if (!mInitialized)
        {
            KAInput.pInstance.EnableInputType("Jump", InputType.ALL, true);
            if (_CurrentPet == null)
            {
                _CurrentPet = SanctuaryManager.pCurPetInstance;
            }
            if (_CurrentPet != null)
            {
                _CurrentPet.AIActor.SetState(AISanctuaryPetFSM.SCIENCE_LAB);
                if (_CurrentPet.pIsMounted)
                {
                    _CurrentPet.OnFlyDismountImmediate(AvAvatar.pObject, true);
                }
                _CurrentPet.SetCanBePetted(false);
                mOldHover = _CurrentPet._Hover;
                _CurrentPet._Hover = false;
                mOldFly = _CurrentPet._CanFly;
                _CurrentPet._CanFly = false;
                mOldEnablePetAnim = _CurrentPet.pEnablePetAnim;
                _CurrentPet.pEnablePetAnim = false;
                mOldIdleAnimationName = _CurrentPet._IdleAnimName;
                _CurrentPet._IdleAnimName = _CurrentPet._MountIdleAnim;
                if (_PetMarker != null)
                {
                    _CurrentPet.transform.position = _PetMarker.position;
                    _CurrentPet.transform.rotation = _PetMarker.rotation;
                    _CurrentPet.transform.forward = _PetMarker.forward;
                    _CurrentPet.SetFollowAvatar(false);
                    SanctuaryManager.pInstance.pSetFollowAvatar = true;
                }
                mOldCam = _CurrentPet.GetCamera();
                _CurrentPet.SetCamera(_MainCamera);
                _CurrentPet.SetState(Character_State.idle);
                _CurrentPet.SetEelBlastScale();
                _CurrentPet._Move2D = false;
                if (SanctuaryManager.pInstance.pPetMeter != null)
                {
                    SanctuaryManager.pInstance.pPetMeter.SetVisibility(true);
                }
                for (int i = 0; i < _CelebrationInfo._DragonCelebrationParticles.Length; i++)
                {
                    if (_CurrentPet.pTypeInfo._TypeID == _CelebrationInfo._DragonCelebrationParticles[i]._DragonID)
                    {
                        _CelebrationInfo._CurrDragonInfo = _CelebrationInfo._DragonCelebrationParticles[i];
                        break;
                    }
                }
            }
            if (MissionManager.pInstance != null)
            {
                MissionManager.pInstance.SetTimedTaskUpdate(false, false);
            }
            if (MainStreetMMOClient.pInstance != null)
            {
                MainStreetMMOClient.pInstance.SetBusy(true);
            }
            mInitialized = true;
            AvAvatar.SetActive(false);
            if (_CurrentPet != null)
            {
                _CurrentPet.SetLookAtObject(_MainCamera.transform, true, Vector3.zero);
            }
            _HUD._BackBtnCallBack = new EBGameManager.UpdateFunction(ShowQuitMessage);
            _HUD._CommonVariables = mCommonVariables;
            _Wave._CommonVariables = mCommonVariables;
            _Terror._CommonVariables = mCommonVariables;
            _HUD._Camera = _MainCamera;
            SnChannel.PausePool("AmbSFX_Pool");
        }
    }

    public void ExitGame()
    {
        if (mGenericDBUi != null)
        {
            UnityEngine.Object.Destroy(mGenericDBUi.gameObject);
        }
        if (ChallengeInfo.pActiveChallenge != null)
        {
            ChallengeInfo.CheckForChallengeCompletion(_GameID, 1, 0, mTotalScore, false);
            ChallengeInfo.pActiveChallenge = null;
        }
        if (_CurrentPet != null)
        {
            _CurrentPet.AIActor.SetState(AISanctuaryPetFSM.NORMAL);
        }
        if (SanctuaryManager.pInstance.pPetMeter != null)
        {
            SanctuaryManager.pInstance.pPetMeter.SetVisibility(true);
        }
        if (MainStreetMMOClient.pInstance != null)
        {
            MainStreetMMOClient.pInstance.SetBusy(false);
        }
        if (MissionManager.pInstance != null)
        {
            MissionManager.pInstance.SetTimedTaskUpdate(true, false);
        }
        Input.ResetInputAxes();
        AvAvatar.SetActive(true);
        AvAvatar.pState = AvAvatarState.IDLE;
        transform.root.gameObject.SetActive(false);
        if (_CurrentPet.animation[_DragonJumpAnimName] != null)
        {
            _CurrentPet.animation[_DragonJumpAnimName].speed = 1f;
        }
        if (_CurrentPet.animation[_DragonDuckAnimName] != null)
        {
            _CurrentPet.animation[_DragonDuckAnimName].speed = 1f;
        }
        if (_CurrentPet != null)
        {
            _CurrentPet.SetTOW(null);
            _CurrentPet.SetCanBePetted(false);
            _CurrentPet.SetCamera(mOldCam);
            _CurrentPet._Move2D = true;
            _CurrentPet._ActionDoneMessageObject = null;
            _CurrentPet.RestoreScale();
            _CurrentPet.StopLookAtObject();
            _CurrentPet._Hover = mOldHover;
            _CurrentPet._CanFly = mOldFly;
            _CurrentPet.pEnablePetAnim = mOldEnablePetAnim;
            _CurrentPet._IdleAnimName = mOldIdleAnimationName;
            SanctuaryManager.pCheckPetAge = true;
        }
        KAUICursorManager.SetCursor("Arrow", true);
        SnChannel.PlayPool("AmbSFX_Pool");
        UnityEngine.Object.Destroy(transform.root.gameObject);
        //AvAvatar.pStartLocation = AvAvatar.pSpawnAtSetPosition;
        if (string.IsNullOrEmpty(RsResourceManager.pLastLevel))
        {
            UtDebug.LogError("Exit scene name is empty", 10);
        }
        UtUtilities.LoadLevel(RsResourceManager.pLastLevel);
    }

    void Start()
    {
        if (_eelObject == null || _SpawnPoints == null || _SpawnPoints.Length == 0 || _ScoreGO == null)
        {
            UtDebug.LogError("Invalid Values in Eel Manager", 10);
        }
        ConstructLevelsArray();
        mCurrLevelIndex = 0;
        mTotalLevelsCompleted = 0;
        mPerfectLevelCount = 0;
        _HUD.gameObject.SetActive(false);
        _HUD.pScore = mCurrScore;
        mHeightControlPoint = EBEel.DoBezierReverse(_PetMarker.transform.position, _PetJumpHeightMarker.transform.position, _PetMarker.transform.position);
        _GameState = EBGameManager.GameState.LOADING_ASSETS;
        PairData.Load(_PairID, OnPairDataReady, null, false, null);
        UtDebug.Log("Event: EelRoast");
    }

    void OnDestroy()
    {
    }

    void Update()
    {
        if (_GameState != mPrevGameState)
        {
            ReleaseFromPreviousState();
            EnterNewState();
            mPrevGameState = _GameState;
        }
        switch (_GameState)
        {
            case EBGameManager.GameState.LOADING_ASSETS:
                GSLoadingUpdate();
                break;
            case EBGameManager.GameState.WAITING_INFO:
                GSWaitingInfoUpdate();
                break;
            case EBGameManager.GameState.LOADING_LEVEL:
                GSLoadingLevelUpdate();
                break;
            case EBGameManager.GameState.COUNT_DOWN:
                GSCountDownUpdate();
                break;
            case EBGameManager.GameState.INGAME:
                GSInGameUpdate();
                break;
            case EBGameManager.GameState.GAME_END:
                GSGameEndUpdate();
                break;
        }
        CommonUpdate();
    }

    void CommonUpdate()
    {
        if (mStopAnimationAfter > 0f)
        {
            if (mDragonJumpAnimIndex >= 0)
            {
                if (mDragonJumpAnimIndex < _DragonJumpAnimNames.Length - 1)
                {
                    if (!_CurrentPet.IsAnimationPlaying(_DragonJumpAnimNames[mDragonJumpAnimIndex]))
                    {
                        mDragonJumpAnimIndex++;
                        _CurrentPet.PlayAnimation(_DragonJumpAnimNames[mDragonJumpAnimIndex], WrapMode.Once, 1f / _DragonJumpAnimTimes[mDragonJumpAnimIndex], 0.3f);
                    }
                }
                else
                {
                    mDragonJumpAnimIndex = -1;
                }
            }
            mStopAnimationAfter -= Time.deltaTime;
            if (mStopAnimationAfter <= 0f)
            {
                mStopAnimationAfter = 0f;
                _CurrentPet.PlayAnimation(_CurrentPet.GetIdleAnimationName(), WrapMode.Loop, 1f, 0.3f);
            }
        }
    }

    void GSLoadingUpdate()
    {
        if (_CurrentActiveEels.Count != 0)
        {
            foreach (EBEel ebeel in _CurrentActiveEels)
            {
                UnityEngine.Object.Destroy(ebeel.gameObject);
            }
            _CurrentActiveEels.Clear();
        }
        if (mPairData != null)
        {
            ResetGameData();
            _GameState = EBGameManager.GameState.WAITING_INFO;
        }
    }

    void GSWaitingInfoUpdate()
    {
        if (!_UiStartInfo.gameObject.activeSelf)
        {
            _UiStartInfo.gameObject.SetActive(true);
            if (SanctuaryManager.pCurPetInstance != null)
            {
                SanctuaryManager.pCurPetInstance.SetMoodParticleIgnore(_DisableMoodParticle);
            }
            _UiStartInfo.SetEelColor(GetEelColorValue(_Levels[0]._CorrectEel), _Levels[0]._MinCurrectEelsForNextLevel);
        }
    }

    void GSLoadingLevelUpdate()
    {
        if (!mIsDidYouKnowAllowed)
        {
            mIsDidYouKnowAllowed = true;
        }
        if (!mIsDidYouKnowHidden)
        {
            mIsDidYouKnowHidden = true;
            _UiNextLevel.HideDidYouKnow();
        }
        if (!mClearCurrentActiveEels)
        {
            mClearCurrentActiveEels = true;
        }
        if (_CurrentActiveEels.Count != 0)
        {
            for (int i = 0; i < _CurrentActiveEels.Count; i++)
            {
                if (_CurrentActiveEels[i] != null)
                {
                    UnityEngine.Object.Destroy(_CurrentActiveEels[i].gameObject);
                }
            }
            _CurrentActiveEels.Clear();
        }
        mWrongEelClicked = false;
        mIsInBonusLevel = false;
        mCurrLevel = new EBGameManager.LevelProps(_Levels[mCurrLevelIndex]);
        mCurrSlippingTravelTime = 0f;
        mCurrentJumpTime = 0f;
        pIsJumping = false;
        mIsDragonSlippingIntoWater = false;
        mWaveMode = false;
        mIsPetKnockedOut = false;
        mCurrentLevelBonusEelsClicked = 0;
        mCurrentLevelElectricEelsClicked = 0;
        mCurrentLevelRightEelsClicked = 0;
        mCurrentLevelWrongEelsClicked = 0;
        ResetDragon();
        for (int j = 0; j < mCurrLevel._EelGroups.Length; j++)
        {
            if (mCurrLevel._EelGroups[j]._EelType == EBGameManager.EBEelType.NORMAL && mCurrLevel._EelGroups[j]._EelColor == mCurrLevel._CorrectEel)
            {
                mCurrLevel._CorrectGroup = mCurrLevel._EelGroups[j];
                break;
            }
        }
        _CurrEelGroups = mCurrLevel._EelGroups;
        _HUD.SetColor(mCurrLevel._CorrectGroup._ColorValue, _EelColorValues[(int)mCurrLevel._CorrectGroup._EelColor]._ColorNameText.GetLocalizedString());
        _GameState = EBGameManager.GameState.COUNT_DOWN;
    }

    void GSCountDownUpdate()
    {
        if (mCurrCountDownTime == _CountDownTime)
        {
            if (_SndCountDown)
            {
                SnChannel.Play(_SndCountDown, "DEFAULT_POOL", true);
            }
            if (_DragonExcitedAnims != null && _DragonExcitedAnims.Length != 0)
            {
                PlayAnimation(_DragonExcitedAnims[UnityEngine.Random.Range(0, _DragonExcitedAnims.Length)], false);
            }
        }
        if (!_HUD.gameObject.activeSelf)
        {
            _HUD.gameObject.SetActive(true);
            if (ChallengeInfo.pActiveChallenge != null)
            {
                mChallengePoints = ChallengeInfo.pActiveChallenge.Points;
                if (!mChallengeAchieved && mChallengePoints > 0)
                {
                    _HUD.UpdateChallengePoints(mChallengePoints);
                }
            }
            if (!mChallengeAchieved && mChallengePoints > 0)
            {
                _HUD.ChallengeItemVisible(true);
            }
            else
            {
                _HUD.ChallengeItemVisible(false);
            }
        }
        _HUD.pLifesCount = mCurrLifes;
        _HUD.pScore = mCurrScore;
        _HUD.pEelsRemaining = mCurrLevel._MinCurrectEelsForNextLevel;
        mCurrCountDownTime -= Time.deltaTime;
        if (mCurrCountDownTime <= 0f)
        {
            if (MissionManager.pInstance != null)
            {
                MissionManager.pInstance.SetTimedTaskUpdate(true, true);
            }
            _GameState++;
        }
    }

    void GSInGameUpdate()
    {
        if (mCommonVariables._Paused)
        {
            return;
        }
        if (!_HUD.gameObject.activeSelf)
        {
            _HUD.gameObject.SetActive(true);
        }
        _HUD.pLifesCount = mCurrLifes;
        _HUD.pScore = mCurrScore;
        if (SanctuaryManager.pCurPetInstance != null)
        {
            float num = 10f;
            Vector3 pos = _MainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _MainCamera.nearClipPlane * num));
            SanctuaryManager.pCurPetInstance.SetLookAt(pos, true);
        }
        InGameDragonMovementUpdate();
        if (!pIsDucking && !pIsJumping)
        {
            InGameEelShootingUpdate();
        }
    }

    void InGameEelShootingUpdate()
    {
        _HUD.pEelsRemaining = ((mCurrLevel._MinCurrectEelsForNextLevel - mCurrLevel._CorrectGroup._BurntCount > 0) ? (mCurrLevel._MinCurrectEelsForNextLevel - mCurrLevel._CorrectGroup._BurntCount) : 0);
        if (_HUD.pEelsRemaining <= 0)
        {
            _Terror.gameObject.SetActive(false);
            _Wave.gameObject.SetActive(false);
        }
        bool flag = false;
        bool flag2 = false;
        while (!flag)
        {
            flag = true;
            int num = 0;
            while (num < mCurrLevel._EelFrequency && num < _CurrentActiveEels.Count)
            {
                if (_CurrentActiveEels[num] == null)
                {
                    _CurrentActiveEels.RemoveAt(num);
                    flag = false;
                    break;
                }
                num++;
            }
        }
        int totalEelsReadyCount;
        int num2;
        if (mIsInBonusLevel)
        {
            if (!_HUD.IsBonusLevelIntroCompleted())
            {
                return;
            }
            totalEelsReadyCount = GetTotalEelsReadyCount(true);
            num2 = mCurrLevel._EelBonusGroup._Count;
            if ((totalEelsReadyCount == 0 || num2 == 0) && _CurrentActiveEels.Count == 0)
            {
                flag2 = true;
                _GameState = EBGameManager.GameState.GAME_END;
            }
        }
        else
        {
            totalEelsReadyCount = GetTotalEelsReadyCount(false);
            num2 = ((mCurrLevel._MinCurrectEelsForNextLevel - mCurrLevel._CorrectGroup._BurntCount > 0) ? (mCurrLevel._MinCurrectEelsForNextLevel - mCurrLevel._CorrectGroup._BurntCount) : 0);
            if (totalEelsReadyCount == 0 || num2 == 0)
            {
                flag2 = true;
                OnElectricEelClicked();
                if (!_Wave.gameObject.activeSelf && !_Terror.gameObject.activeSelf)
                {
                    if (IsElegibleForBonusLevel())
                    {
                        StartBonusLevel();
                    }
                    else
                    {
                        _GameState = EBGameManager.GameState.GAME_END;
                    }
                }
            }
        }
        if (!flag2 && _CurrentActiveEels.Count < mCurrLevel._EelFrequency && num2 > 0)
        {
            int index = UnityEngine.Random.Range(0, totalEelsReadyCount);
            EBGameManager.EBEelGroup eelGroupFromReadyIndex = GetEelGroupFromReadyIndex(index, mIsInBonusLevel);
            EBEel ebeel = InstantiateEel(eelGroupFromReadyIndex);
            ebeel._Active = true;
            _CurrentActiveEels.Add(ebeel);
        }
    }

    void InGameDragonMovementUpdate()
    {
        if (!_Wave.gameObject.activeSelf && !_Terror.gameObject.activeSelf)
        {
            mCurrLevel._RemainingWaveTime -= Time.deltaTime;
            if (mCurrLevel._RemainingWaveTime <= 0f && mCurrLevel._RemainingWaveCount > 0 && !mIsInBonusLevel)
            {
                StartWave();
            }
            mCurrLevel._RemainingTerrorTime -= Time.deltaTime;
            if (mCurrLevel._RemainingTerrorTime <= 0f && mCurrLevel._RemainingTerrorCount > 0 && !mIsInBonusLevel)
            {
                StartTerror();
            }
        }
        if (mWaveMode || mTerrorMode)
        {
            if (pIsJumping)
            {
                mCurrentJumpTime += Time.deltaTime;
                if (mCurrentJumpTime > _WaveJumpTime)
                {
                    mCurrentJumpTime = _WaveJumpTime;
                }
                _CurrentPet.transform.position = EBEel.DoBezier(_PetMarker.position, mHeightControlPoint, _PetMarker.position, mCurrentJumpTime / _WaveJumpTime);
                if (mCurrentJumpTime == _WaveJumpTime)
                {
                    pIsJumping = false;
                    mCurrentJumpTime = 0f;
                }
            }
            else if (pIsDucking)
            {
                if (_ColliderForTerrorCollision != null && _ColliderForTerrorCollision.enabled)
                {
                    _ColliderForTerrorCollision.enabled = false;
                }
                mCurrDuckTime += Time.deltaTime;
                if (mCurrDuckTime > _TerrorDuckTime)
                {
                    mCurrDuckTime = _TerrorDuckTime;
                }
                if (mCurrDuckTime == _TerrorDuckTime)
                {
                    pIsDucking = false;
                    mCurrDuckTime = 0f;
                    _ColliderForTerrorCollision.enabled = true;
                }
            }
            else if (KAInput.GetKeyDown("space") && mWaveMode)
            {
                if (!mIsDragonSlippingIntoWater)
                {
                    if (_SndJumpSound)
                    {
                        SnChannel.Play(_SndJumpSound, "DEFAULT_POOL", true);
                    }
                    PlayJumpAnimation(_WaveJumpTime);
                    pIsJumping = true;
                }
            }
            else if ((KAInput.GetKeyDown(KeyCode.LeftAlt) || KAInput.GetKeyDown(KeyCode.RightAlt)) && mTerrorMode && !mIsDragonSlippingIntoWater)
            {
                PlayDuckAnimation(_TerrorDuckTime);
                pIsDucking = true;
            }
            if (mIsDragonSlippingIntoWater)
            {
                if (pIsJumping)
                {
                    pIsJumping = false;
                }
                mCurrSlippingTravelTime += Time.deltaTime;
                if (mCurrSlippingTravelTime > _DragonSlipTravelTime)
                {
                    mCurrSlippingTravelTime = _DragonSlipTravelTime;
                }
                _CurrentPet.transform.position = EBEel.DoBezier(mDragonHitStartPoint, mDragonHitControlPoint, mDragonHitEndPoint, mCurrSlippingTravelTime / _DragonSlipTravelTime);
                if (mCurrSlippingTravelTime == _DragonSlipTravelTime)
                {
                    mIsDragonSlippingIntoWater = false;
                    mCurrSlippingTravelTime = 0f;
                }
            }
            if (pIsJumping || pIsDucking || mIsDragonSlippingIntoWater || mIsPetKnockedOut)
            {
                mCommonVariables._Touchable = false;
            }
            else
            {
                mCommonVariables._Touchable = true;
            }
            if (!_Wave.gameObject.activeSelf && !_Terror.gameObject.activeSelf && !pIsDucking && !pIsJumping && !mIsDragonSlippingIntoWater)
            {
                if (mIsPetKnockedOut)
                {
                    mIsGameOver = true;
                    _GameState = EBGameManager.GameState.GAME_END;
                }
                mWaveMode = false;
                mTerrorMode = false;
            }
        }
    }

    void GSGameEndUpdate()
    {
        if (mClearCurrentActiveEels)
        {
            _CurrentActiveEels.Clear();
            mClearCurrentActiveEels = false;
        }
        if (!_EndDBUI.GetVisibility() && !_UiNextLevel.gameObject.activeSelf && !mIsStoreOpen && !(mGenericDBUi != null) && !(UiPetEnergyGenericDB.pInstance != null) && !UiChallengeInvite.pInstance.GetVisibility())
        {
            if (mCurrLevel._CorrectGroup._BurntCount >= mCurrLevel._MinCurrectEelsForNextLevel && !mIsGameOver)
            {
                PlayDragonCelebration();
                mTotalLevelsCompleted++;
                int num = mCurrLevelIndex;
                if (num < _Levels.Length - 1)
                {
                    mCurrLevelIndex++;
                }
                EBGameManager.EBEelColorValue eelColorValue = GetEelColorValue(_Levels[mCurrLevelIndex]._CorrectEel);
                float num2 = (float)(mCurrentLevelBonusEelsClicked + mCurrentLevelElectricEelsClicked + mCurrentLevelRightEelsClicked + mCurrentLevelWrongEelsClicked);
                float accuracy = 0f;
                if (num2 > 0f)
                {
                    accuracy = (float)((mCurrentLevelBonusEelsClicked + mCurrentLevelElectricEelsClicked + mCurrentLevelRightEelsClicked) * 100) / num2;
                }
                if (num == _DisplayBonusInfoAterLevel)
                {
                    _UiNextLevel.SetDisplayType(UiEBNextLevel.LevelCompleteType.BonusEel, GetUserMessage(accuracy), mTotalLevelsCompleted, eelColorValue, _Levels[mCurrLevelIndex]._MinCurrectEelsForNextLevel, _HUD.pScore, mCurrLifes, accuracy);
                }
                else if (num == _DisplayElectricInfoAfterLevel)
                {
                    _UiNextLevel.SetDisplayType(UiEBNextLevel.LevelCompleteType.ElectricEel, GetUserMessage(accuracy), mTotalLevelsCompleted, eelColorValue, _Levels[mCurrLevelIndex]._MinCurrectEelsForNextLevel, _HUD.pScore, mCurrLifes, accuracy);
                }
                else if (num == _DisplayWaveInfoAfterLevel)
                {
                    _UiNextLevel.SetDisplayType(UiEBNextLevel.LevelCompleteType.WaveEffect, GetUserMessage(accuracy), mTotalLevelsCompleted, eelColorValue, _Levels[mCurrLevelIndex]._MinCurrectEelsForNextLevel, _HUD.pScore, mCurrLifes, accuracy);
                }
                else if (num == _DisplayTerrorInfoAfterLevel)
                {
                    _UiNextLevel.SetDisplayType(UiEBNextLevel.LevelCompleteType.TerrorEffect, GetUserMessage(accuracy), mTotalLevelsCompleted, eelColorValue, _Levels[mCurrLevelIndex]._MinCurrectEelsForNextLevel, _HUD.pScore, mCurrLifes, accuracy);
                }
                else
                {
                    _UiNextLevel.SetDisplayType(UiEBNextLevel.LevelCompleteType.Normal, GetUserMessage(accuracy), mTotalLevelsCompleted, eelColorValue, _Levels[mCurrLevelIndex]._MinCurrectEelsForNextLevel, _HUD.pScore, mCurrLifes, accuracy);
                }
                if (MissionManager.pInstance != null)
                {
                    MissionManager.pInstance.SetTimedTaskUpdate(false, false);
                }
                _UiNextLevel.gameObject.SetActive(true);
                if (mIsInBonusLevel)
                {
                    if (_SndGamePerfectEndMusic != null)
                    {
                        SnChannel.Play(_SndGamePerfectEndMusic[UnityEngine.Random.Range(0, _SndGamePerfectEndMusic.Length)], "DEFAULT_POOL", true);
                    }
                }
                else if (_SndGameEndMusic)
                {
                    SnChannel.Play(_SndGameEndMusic, "DEFAULT_POOL", true);
                }
            }
            else
            {
                PlayAnimation(_DragonSadAnimName, false);
                EnableGameEndScreen();
                if (mIsInBonusLevel)
                {
                    if (_SndGamePerfectEndMusic != null)
                    {
                        SnChannel.Play(_SndGamePerfectEndMusic[UnityEngine.Random.Range(0, _SndGamePerfectEndMusic.Length)], "DEFAULT_POOL", true);
                    }
                }
                else if (_SndGameEndMusic)
                {
                    SnChannel.Play(_SndGameEndMusic, "DEFAULT_POOL", true);
                }
            }
        }
        if (_UiNextLevel.IsActive() && mIsDidYouKnowHidden && mIsDidYouKnowAllowed)
        {
            if (SanctuaryManager.pCurPetInstance != null)
            {
                float num3 = 10f;
                Vector3 pos = _MainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _MainCamera.nearClipPlane * num3));
                SanctuaryManager.pCurPetInstance.SetLookAt(pos, true);
            }
            if (_CurrentActiveEels.Count == 0)
            {
                EBGameManager.EBEelGroup eelGroupFromReadyIndex = GetEelGroupFromReadyIndex(0, true);
                EBEel ebeel = InstantiateEel(eelGroupFromReadyIndex);
                ebeel._Active = true;
                _CurrentActiveEels.Add(ebeel);
            }
            else if (_CurrentActiveEels.Count == 1 && _CurrentActiveEels[0] == null)
            {
                _CurrentActiveEels.Clear();
            }
            if (_CurrentActiveEels.Count == 0)
            {
                return;
            }
            if (mObClickableEel == null)
            {
                mObClickableEel = _CurrentActiveEels[0].gameObject.GetComponentInChildren<ObClickableEelBlast>();
                return;
            }
            if (mObClickableEel._MouseOver)
            {
                KAUICursorManager.SetCustomCursor("", _ReticleTex, null, 0, 0);
                return;
            }
            KAUICursorManager.SetDefaultCursor("Arrow", true);
        }
    }

    void OnGenericDBButtonClicked(string ResponseStr)
    {
        if (ResponseStr.Contains("Yes"))
        {
            if (_GameState == EBGameManager.GameState.WAITING_INFO)
            {
                _CurrentPet.UpdateMeter(SanctuaryPetMeterType.ENERGY, _EnergyChangeOnStart);
                _GameState = EBGameManager.GameState.LOADING_LEVEL;
            }
            else if (_GameState == EBGameManager.GameState.GAME_END)
            {
                if (_UiNextLevel.gameObject.activeSelf)
                {
                    _GameState = EBGameManager.GameState.LOADING_LEVEL;
                }
                else
                {
                    UtDebug.Log("Shouldnt be here");
                }
            }
        }
        else if (ResponseStr.Contains("No"))
        {
            if (_GameState == EBGameManager.GameState.WAITING_INFO)
            {
                ExitGame();
            }
            else if (_GameState == EBGameManager.GameState.GAME_END)
            {
                if (_UiNextLevel.gameObject.activeSelf)
                {
                    _UiNextLevel.gameObject.SetActive(false);
                    EnableGameEndScreen();
                }
                else
                {
                    ExitGame();
                }
            }
        }
        else if (ResponseStr.Contains("Replay") && _GameState == EBGameManager.GameState.GAME_END)
        {
            OnReplayClicked();
        }
        if (_GameState == EBGameManager.GameState.GAME_END)
        {
            mIsDidYouKnowAllowed = false;
        }
    }

    void StartWave()
    {
        mIsPetKnockedOut = false;
        _Wave.gameObject.SetActive(true);
        _Wave.StartWave(_CurrentPet.collider, mCurrLevel._WaveTravelTime.GetRandomValue());
        mWaveMode = true;
        mCurrLevel._RemainingWaveCount--;
        mCurrLevel._RemainingWaveTime = mCurrLevel._WaveTravelTime.GetRandomValue();
    }

    void StartTerror()
    {
        mIsPetKnockedOut = false;
        _Terror.gameObject.SetActive(true);
        _Terror.Start(_ColliderForTerrorCollision, mCurrLevel._TerrorTravelTime.GetRandomValue(), _CurrentPet.GetHeadPosition());
        mTerrorMode = true;
        mCurrLevel._RemainingTerrorCount--;
        mCurrLevel._RemainingTerrorTime = mCurrLevel._TimeBetweenTerors.GetRandomValue();
    }

    public void OnPairDataReady(bool success, PairData pData, object inUserData)
    {
        if (pData != null)
        {
            mPairData = pData;
            return;
        }
        mPairData = new PairData();
    }

    void OnEelClicked(EBEel eel)
    {
        int num = mCurrScore;
        if (_CurrentPet.animation.IsPlaying(_DragonAttackAnimName))
        {
            _CurrentPet.PlayAnimation(_DragonAttackAnimName, WrapMode.Once, _DragonAttackAnimSpeed, 0f);
            mStopAnimationAfter = _CurrentPet.animation[_DragonAttackAnimName].length / _DragonAttackAnimSpeed;
        }
        else
        {
            _CurrentPet.PlayAnimation(_DragonAttackAnimName, WrapMode.Once, _DragonAttackAnimSpeed, 0.25f);
            mStopAnimationAfter = _CurrentPet.animation[_DragonAttackAnimName].length / _DragonAttackAnimSpeed;
        }
        EBGameManager.FireDataByPetType fireDataByPetType = FindFireDataByPetTypeID(SanctuaryManager.pCurrentPetType);
        if (fireDataByPetType == null)
        {
            UtDebug.Log("No fire data defined for pet type:" + SanctuaryManager.pCurrentPetType);
        }
        else
        {
            WeaponManager componentInChildren = _CurrentPet.GetComponentInChildren<WeaponManager>();
            UnityEngine.Object.Instantiate<GameObject>(fireDataByPetType._DragonMouthFire, componentInChildren.pShootPoint, fireDataByPetType._DragonMouthFire.transform.rotation).transform.parent = componentInChildren.ShootPointTrans;
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(fireDataByPetType._AmmoPrt, componentInChildren.pShootPoint, fireDataByPetType._AmmoPrt.transform.rotation);
            gameObject.transform.parent = _PrtRoot.transform;
            EBAmmo component = gameObject.GetComponent<EBAmmo>();
            component._FollowObj = eel.gameObject;
            component._TimeOfTravel = _ParticleTravelTime;
            if (_SndDragonFire)
            {
                SnChannel.Play(_SndDragonFire, "DEFAULT_POOL", true);
            }
        }
        eel.MarkAsDestroying();
        if (eel._Group._EelType == EBGameManager.EBEelType.BONUS)
        {
            if (!mClearCurrentActiveEels && _GameState == EBGameManager.GameState.GAME_END)
            {
                _UiNextLevel.ShowDidYouKnow(_DidYouKnowText[UnityEngine.Random.Range(0, _DidYouKnowText.Length)].GetLocalizedString());
                KAUICursorManager.SetDefaultCursor("Arrow", true);
                SnChannel.Play(_SndDidYouKnow, "DEFAULT_POOL", true);
                mIsDidYouKnowHidden = false;
                return;
            }
            mTotalBonusEelsClicked++;
            mCurrentLevelBonusEelsClicked++;
            mCurrScore += mCurrLevel._BonusEelScore;
            if (_SndBonusEelHit)
            {
                SnChannel.Play(_SndBonusEelHit, "DEFAULT_POOL", true);
            }
            _CurrentPet.UpdateMeter(SanctuaryPetMeterType.HAPPINESS, mCurrLevel._HappinessOnBonusEel);
        }
        else if (eel._Group._EelType == EBGameManager.EBEelType.ELECTRIC)
        {
            if (_SndElectricEelHit)
            {
                SnChannel.Play(_SndElectricEelHit, "DEFAULT_POOL", true);
            }
            mCurrScore += mCurrLevel._ElectricEelScore;
            mTotalElectricEelsClicked++;
            mCurrentLevelElectricEelsClicked++;
            OnElectricEelClicked();
        }
        else if (eel._Group._EelType == EBGameManager.EBEelType.NORMAL)
        {
            if (_SndEelHit)
            {
                SnChannel.Play(_SndEelHit, "DEFAULT_POOL", true);
            }
            if (eel._Group._EelColor == mCurrLevel._CorrectEel)
            {
                if (_SndPositiveScore)
                {
                    SnChannel.Play(_SndPositiveScore, "DEFAULT_POOL", true);
                }
                mCurrScore += mCurrLevel._EelScore;
                mTotalRightEelsClicked++;
                mCurrentLevelRightEelsClicked++;
            }
            else
            {
                if (_SndNegativeScore)
                {
                    SnChannel.Play(_SndNegativeScore, "DEFAULT_POOL", true);
                }
                mCurrScore -= mCurrLevel._EelScore;
                mTotalWrongEelsClicked++;
                mCurrentLevelWrongEelsClicked++;
                if (mCurrScore < 0)
                {
                    mCurrScore = 0;
                }
                mWrongEelClicked = true;
            }
        }
        if (mCurrScore != num)
        {
            Show3DTargetHitScore(eel.transform.position, mCurrScore - num);
        }
        if (!mChallengeAchieved && mChallengePoints > 0 && mCurrScore > mChallengePoints)
        {
            mChallengeAchieved = true;
            _HUD.FlashChallengeItem(_ChallengeFlashTimeInterval, _ChallengeFlashLoopCount);
            SnChannel.Play(_ChallengeCompleteSFX, "SFX_Pool", true, null);
        }
        _HUD.OnEelClicked();
    }

    void OnEelDestroyed(EBEel eel)
    {
    }

    EBEel InstantiateEel(EBGameManager.EBEelGroup eelGroup)
    {
        if (_eelObject == null)
        {
            UtDebug.LogError("Error: Eel object is null", 10);
        }
        GameObject gameObject = null;
        if (eelGroup._EelType == EBGameManager.EBEelType.NORMAL)
        {
            gameObject = UnityEngine.Object.Instantiate<GameObject>(_eelObject);
        }
        else if (eelGroup._EelType == EBGameManager.EBEelType.BONUS)
        {
            gameObject = UnityEngine.Object.Instantiate<GameObject>(_BonusEelObject);
        }
        else if (eelGroup._EelType == EBGameManager.EBEelType.ELECTRIC)
        {
            gameObject = UnityEngine.Object.Instantiate<GameObject>(_ElectricEelObject);
        }
        gameObject.transform.parent = base.gameObject.transform;
        EBEel component = gameObject.GetComponent<EBEel>();
        if (component == null)
        {
            UtDebug.LogError("Error: Eel object is doesnt have eel script attached", 10);
        }
        component._Active = false;
        component._Group = eelGroup;
        if (_GameState == EBGameManager.GameState.GAME_END)
        {
            component._StartPoint = _DidYouKnowSpawnPoints[0];
            component._EndPoint = _DidYouKnowSpawnPoints[1];
        }
        else
        {
            component._StartPoint = _SpawnPoints[UnityEngine.Random.Range(0, _SpawnPoints.Length)];
            component._EndPoint = _SpawnPoints[UnityEngine.Random.Range(0, _SpawnPoints.Length)];
        }
        if (eelGroup._EelType == EBGameManager.EBEelType.BONUS)
        {
            component._TopPoint = (component._StartPoint.position + component._EndPoint.position) / 2f + component._StartPoint.up * UnityEngine.Random.Range(mCurrLevel._BonusEelHeightMin, mCurrLevel._BonusEelHeightMax);
        }
        else
        {
            component._TopPoint = (component._StartPoint.position + component._EndPoint.position) / 2f + component._StartPoint.up * UnityEngine.Random.Range(mCurrLevel._EelHeightMin, mCurrLevel._EelHeightMax);
        }
        if (eelGroup._EelType == EBGameManager.EBEelType.BONUS)
        {
            component._Speed = UnityEngine.Random.Range(mCurrLevel._BonusEelSpeedMin, mCurrLevel._BonusEelSpeedMax);
            component._DelayOnTop = mCurrLevel._BonusEelDelayOnTop;
        }
        else
        {
            component._Speed = UnityEngine.Random.Range(mCurrLevel._EelSpeedMin, mCurrLevel._EelSpeedMax);
            component._DelayOnTop = mCurrLevel._EelDelayOnTop;
        }
        component._OnClicked = new EBEel.OnEvent(OnEelClicked);
        component._OnDestroyed = new EBEel.OnEvent(OnEelDestroyed);
        component._ParticleRoot = _PrtRoot;
        component._WaterHeightMarker = _WaterHeightMarker;
        component.Initialize(mCommonVariables);
        return component;
    }

    EBGameManager.EBEelGroup GetEelGroupFromReadyIndex(int index, bool IsBonusLevel)
    {
        if (IsBonusLevel)
        {
            return mCurrLevel._EelBonusGroup;
        }
        int num = 0;
        for (int i = 0; i < _CurrEelGroups.Length; i++)
        {
            num += _CurrEelGroups[i]._Count;
            if (index < num)
            {
                return _CurrEelGroups[i];
            }
        }
        return null;
    }

    int GetTotalEelsReadyCount(bool IsBonusLevel)
    {
        int num = 0;
        if (IsBonusLevel)
        {
            num += mCurrLevel._EelBonusGroup._Count;
        }
        else
        {
            for (int i = 0; i < _CurrEelGroups.Length; i++)
            {
                num += _CurrEelGroups[i]._Count;
            }
        }
        return num;
    }

    EBGameManager.EBEelColorValue GetEelColorValue(EBGameManager.EBEelColor eel)
    {
        for (int i = 0; i < _EelColorValues.Length; i++)
        {
            if (eel == _EelColorValues[i]._Color)
            {
                return _EelColorValues[i];
            }
        }
        UtDebug.LogError("Color Values are not set (_EelColorValues). This will lead to Null Refs ", 10);
        return null;
    }

    void OnReplayClicked()
    {
        if (SanctuaryManager.pCurPetInstance != null)
        {
            if (SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.HAPPINESS) < Mathf.Abs(_MinHappinessToReplay))
            {
                UiPetEnergyGenericDB.Show(gameObject, "OnYesLowEnergyMessage", null, false, null);
                if (_SndLowEnergyVO != null)
                {
                    SnChannel.Play(_SndLowEnergyVO, "VO_Pool", 0, true);
                }
                return;
            }
            if (SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.ENERGY) < Mathf.Abs(_MinEnergyToReplay))
            {
                UiPetEnergyGenericDB.Show(gameObject, "OnYesLowEnergyMessage", "OnNoLowEnergyMessage", true, null);
                if (_SndLowEnergyVO != null)
                {
                    SnChannel.Play(_SndLowEnergyVO, "VO_Pool", 0, true);
                }
                return;
            }
            if (!SanctuaryManager.pInstance.pPetMeter.GetVisibility())
            {
                SanctuaryManager.pInstance.pPetMeter.SetVisibility(true);
            }
        }
        _CurrentPet.UpdateMeter(SanctuaryPetMeterType.ENERGY, _MinEnergyToReplay);
        ResetGameData();
        _GameState = EBGameManager.GameState.LOADING_LEVEL;
    }

    void ResetGameData()
    {
        mCommonVariables._Paused = false;
        mCommonVariables._Touchable = true;
        mIsGameOver = false;
        mTotalLevelsCompleted = 0;
        mCurrLevelIndex = 0;
        mPerfectLevelCount = 0;
        mCurrLifes = _MaxLifes;
        mCurrScore = 0;
        mTotalScore = 0;
        mCurrentLevelRightEelsClicked = 0;
        mCurrentLevelWrongEelsClicked = 0;
        mCurrentLevelBonusEelsClicked = 0;
        mCurrentLevelElectricEelsClicked = 0;
        mTotalRightEelsClicked = 0;
        mTotalWrongEelsClicked = 0;
        mTotalBonusEelsClicked = 0;
        mTotalElectricEelsClicked = 0;
        mTotalBonusEelsClicked = 0;
        _HUD.pScore = 0;
        _HUD.ShowLifes(false);
    }

    EBGameManager.FireDataByPetType FindFireDataByPetTypeID(int inPetTypeID)
    {
        foreach (EBGameManager.FireDataByPetType fireDataByPetType2 in _FireDataByPetType)
        {
            if (fireDataByPetType2._TypeID == inPetTypeID)
            {
                return fireDataByPetType2;
            }
        }
        return null;
    }

    void SetChallengeText(string challengeText)
    {
        _EndDBUI.SetRewardMessage(challengeText);
    }

    void EnableGameEndScreen()
    {
        mTotalScore = mCurrScore + mPerfectLevelCount * _PerfectLevelBonus;
        UiChallengeInvite.SetData(_GameID, 1, 0, mTotalScore);
        HighScores.SetCurrentGameSettings(_GameModuleName, _GameID, false, 0, 1);
        HighScores.AddGameData("highscore", mTotalScore.ToString());
        _EndDBUI.SetHighScoreData(mTotalScore, "highscore", false);
        if (SanctuaryManager.pInstance != null && SanctuaryManager.pInstance.pPetMeter.GetVisibility())
        {
            SanctuaryManager.pInstance.pPetMeter.SetVisibility(false);
        }
        int num = mTotalBonusEelsClicked + mTotalElectricEelsClicked + mTotalRightEelsClicked + mTotalWrongEelsClicked;
        int num2 = num;
        RaisedPetAttribute raisedPetAttribute = SanctuaryManager.pCurPetInstance.pData.FindAttrData("eels");
        if (raisedPetAttribute != null)
        {
            num2 = int.Parse(raisedPetAttribute.Value) + num2;
        }
        SanctuaryManager.pCurPetInstance.pData.SetAttrData("eels", num2.ToString(), DataType.INT);
        float num3 = (float)((num != 0) ? ((mTotalBonusEelsClicked + mTotalElectricEelsClicked + mTotalRightEelsClicked) * 100 / num) : 0);
        _EndDBUI.SetVisibility(true);
        _EndDBUI.SetGameSettings(_GameModuleName, gameObject, "any", true);
        _EndDBUI.SetResultData("BonusEelsRoasted", null, mTotalBonusEelsClicked.ToString(), null, null);
        _EndDBUI.SetResultData("LevelCompleted", null, mTotalLevelsCompleted.ToString(), null, null);
        _EndDBUI.SetResultData("PerfectLevel", null, mPerfectLevelCount.ToString(), null, null);
        _EndDBUI.SetResultData("TotalEelsRoasted", null, num.ToString(), null, null);
        _EndDBUI.SetResultData("TotalScore", null, mTotalScore.ToString(), null, null);
        _EndDBUI.SetResultData("Accuracy", null, num3.ToString(), null, null);
        if (SubscriptionInfo.pIsMember)
        {
            _EndDBUI.SetRewardMessage(_MemberResultText.GetLocalizedString());
        }
        else
        {
            _EndDBUI.SetRewardMessage(_NonMemberResultText.GetLocalizedString());
        }
        _CurrentPet.UpdateMeter(SanctuaryPetMeterType.HAPPINESS, _HappinessIncreaseOnEachLevel * (float)mTotalLevelsCompleted);
        if (MissionManager.pInstance != null)
        {
            _CurrentPet.CheckForTaskCompletion(PetActions.EELBLAST);
            MissionManager.pInstance.CheckForTaskCompletion("Game", _GameModuleName);
        }
        KAUICursorManager.SetDefaultCursor("Loading", true);
        string text = _GameModuleName;
        if (SubscriptionInfo.pIsMember)
        {
            text += "Member";
        }
        _EndDBUI.SetAdRewardData(text, mTotalScore);
        WsWebService.ApplyPayout(text, mTotalScore, new WsServiceEventHandler(ServiceEventHandler), null);
        if (_HUD != null)
        {
            _HUD.ChallengeItemVisible(false);
        }
        ChallengeInfo pActiveChallenge = ChallengeInfo.pActiveChallenge;
        if (pActiveChallenge != null)
        {
            ChallengeResultState challengeResultState = ChallengeInfo.CheckForChallengeCompletion(_GameID, 1, 0, mTotalScore, false);
            if (challengeResultState == ChallengeResultState.LOST)
            {
                SetChallengeText(_ChallengeTryAgainText.GetLocalizedString());
            }
            else if (challengeResultState == ChallengeResultState.WON)
            {
                string text2 = _ChallengeCompleteText.GetLocalizedString();
                if (text2.Contains("[Name]"))
                {
                    bool flag = false;
                    if (BuddyList.pIsReady)
                    {
                        Buddy buddy = BuddyList.pInstance.GetBuddy(pActiveChallenge.UserID.ToString());
                        if (buddy != null && !string.IsNullOrEmpty(buddy.DisplayName))
                        {
                            text2 = text2.Replace("[Name]", buddy.DisplayName);
                            SetChallengeText(text2);
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        SetChallengeText("");
                        WsWebService.GetDisplayNameByUserID(pActiveChallenge.UserID.ToString(), new WsServiceEventHandler(ServiceEventHandler), null);
                    }
                }
                else
                {
                    SetChallengeText(text2);
                }
            }
        }
        else
        {
            SetChallengeText("");
        }
        if (_EndDBUI != null)
        {
            _EndDBUI.AllowChallenge(mTotalScore > 0);
        }
        ChallengeInfo.pActiveChallenge = null;
        UiChallengeInvite.SetData(_GameID, 1, 0, mTotalScore);
    }

    void OnReplayGame()
    {
        OnGenericDBButtonClicked("Replay");
    }

    void OnEndDBClose()
    {
        OnGenericDBButtonClicked("No");
    }

    void OnStoreOpened()
    {
        mIsStoreOpen = true;
        _EndDBUI.SetVisibility(false);
    }

    void OnStoreClosed()
    {
        mIsStoreOpen = false;
        _EndDBUI.SetVisibility(true);
    }

    void ReleaseFromPreviousState()
    {
        switch (mPrevGameState)
        {
            case EBGameManager.GameState.LOADING_ASSETS:
            case EBGameManager.GameState.LOADING_LEVEL:
            case EBGameManager.GameState.COUNT_DOWN:
                break;
            case EBGameManager.GameState.WAITING_INFO:
                _UiStartInfo.gameObject.SetActive(false);
                return;
            case EBGameManager.GameState.INGAME:
                _HUD.gameObject.SetActive(false);
                _CurrentPet.StopLookAtObject();
                SnChannel.StopPool("MusicEelSFX_Pool");
                return;
            case EBGameManager.GameState.GAME_END:
                _EndDBUI.SetVisibility(false);
                _UiNextLevel.gameObject.SetActive(false);
                _CurrentPet.PlayAnimation(_CurrentPet.GetIdleAnimationName(), WrapMode.Loop, 1f, 0.3f);
                break;
            default:
                return;
        }
    }

    void EnterNewState()
    {
        switch (_GameState)
        {
            case EBGameManager.GameState.LOADING_ASSETS:
            case EBGameManager.GameState.WAITING_INFO:
            case EBGameManager.GameState.LOADING_LEVEL:
            case EBGameManager.GameState.GAME_END:
                break;
            case EBGameManager.GameState.COUNT_DOWN:
                _UiCountDown.enabled = false;
                mCurrCountDownTime = _CountDownTime;
                return;
            case EBGameManager.GameState.INGAME:
                {
                    int num = mCurrLevelIndex / _GameMusicRepeatInterval;
                    if (_SndGameMusic != null)
                    {
                        num %= _SndGameMusic.Length;
                        if (_SndGameMusic[num])
                        {
                            SnChannel.AcquireChannel("MusicEelSFX_Pool", true).pVolume = _GameMusicVolume;
                            SnChannel.Play(_SndGameMusic[num], "MusicEelSFX_Pool", true);
                        }
                    }
                    break;
                }
            default:
                return;
        }
    }

    void OnElectricEelClicked()
    {
        for (int i = 0; i < _CurrentActiveEels.Count; i++)
        {
            if ((_CurrentActiveEels[i]._Group._EelType == EBGameManager.EBEelType.NORMAL || _CurrentActiveEels[i]._Group._EelType == EBGameManager.EBEelType.BONUS || _CurrentActiveEels[i]._Group._EelType == EBGameManager.EBEelType.ELECTRIC) && !_CurrentActiveEels[i].pIsDestroying)
            {
                _CurrentActiveEels[i].MarkAsDestroying();
                _CurrentActiveEels[i].MarkAsDestroyedForce(_PrtEelsElectrified);
                mCurrScore += mCurrLevel._ScoreForOtherEelsOnBombed;
                Show3DTargetHitScore(_CurrentActiveEels[i].transform.position, mCurrLevel._ScoreForOtherEelsOnBombed);
            }
        }
    }

    void ConstructLevelsArray()
    {
        EBGameManager.LevelProps[] levels = _Levels;
        _Levels = new EBGameManager.LevelProps[levels.Length + _MaxLevelsToGenerate];
        for (int i = 0; i < levels.Length; i++)
        {
            _Levels[i] = new EBGameManager.LevelProps(levels[i]);
        }
        int num = 0;
        for (int j = levels.Length; j < _Levels.Length; j++)
        {
            EBGameManager.LevelProps props = _DeltaLevel[num];
            num++;
            if (num >= _DeltaLevel.Length)
            {
                num = 0;
            }
            _Levels[j] = new EBGameManager.LevelProps(_Levels[j - 1]);
            _Levels[j].AddProps(props, _MaxLevel);
        }
        int num2 = Enum.GetNames(typeof(EBGameManager.EBEelColor)).Length;
        List<EBGameManager.EBEelColor> list = new List<EBGameManager.EBEelColor>();
        for (int k = 0; k < _Levels.Length; k++)
        {
            bool flag = false;
            list.Clear();
            for (int l = 0; l < num2; l++)
            {
                list.Add((EBGameManager.EBEelColor)l);
            }
            UtUtilities.Shuffle<EBGameManager.EBEelColor>(list);
            _Levels[k]._RemainingWaveCount = (int)_Levels[k]._WaveCount.GetRandomValue();
            _Levels[k]._RemainingWaveTime = _Levels[k]._TimeBetweenWaves.GetRandomValue();
            _Levels[k]._RemainingTerrorCount = (int)_Levels[k]._TerrorCount.GetRandomValue();
            _Levels[k]._RemainingTerrorTime = _Levels[k]._TimeBetweenTerors.GetRandomValue();
            if (list.Count < _Levels[k]._EelGroups.Length)
            {
                UtDebug.LogError("insufficient colors. Shouldnt come here or will lead to argument out of index exception", 10);
            }
            for (int m = 0; m < _Levels[k]._EelGroups.Length; m++)
            {
                _Levels[k]._EelGroups[m]._EelColor = list[m];
                EBGameManager.EBEelColorValue eelColorValue = GetEelColorValue(_Levels[k]._EelGroups[m]._EelColor);
                _Levels[k]._EelGroups[m]._ColorValue = eelColorValue._ColorValue;
                _Levels[k]._EelGroups[m]._MaxCount = _Levels[k]._EelGroups[m]._Count;
                if (_Levels[k]._EelGroups[m]._CorrectEelGroup)
                {
                    if (flag)
                    {
                        UtDebug.LogError("Multiple Correct EelGroup is assigned to level:" + k, 10);
                    }
                    _Levels[k]._CorrectEel = _Levels[k]._EelGroups[m]._EelColor;
                    flag = true;
                }
            }
            if (!flag)
            {
                UtDebug.LogError("Correct EelGroup is not assigned to level:" + k, 10);
            }
        }
    }

    void Show3DTargetHitScore(Vector3 inPosition, int inScore)
    {
        inPosition.z = _PrtRoot.transform.position.z;
        TargetHit3DScore.Show3DHitScore(_ScoreGO, inPosition, inScore, false, EffectType.INVALID);
    }

    void GetTopScoreEventHandler(GameDataSummary gdata, object inUserData)
    {
        if (gdata != null && gdata.GameDataList != null)
        {
            foreach (GameData gameData in gdata.GameDataList)
            {
                string str = gameData.UserName;
                if (Nicknames.pInstance != null)
                {
                    string nickname = Nicknames.pInstance.GetNickname(gameData.UserID.ToString());
                    if (!string.IsNullOrEmpty(nickname))
                    {
                        str = nickname;
                    }
                }
                UtDebug.Log("HighScore Names:" + str);
            }
        }
    }

    void OnHighScoresDone()
    {
        UtDebug.Log("OnHighScoresDone");
        if (HighScores.pInstance.pHighScoresInterface != null)
        {
            UnityEngine.Object.Destroy(HighScores.pInstance.pHighScoresInterface);
        }
    }

    string GetUserMessage(float Accuracy)
    {
        if (_UserMessages == null || _UserMessages.Length == 0)
        {
            UtDebug.LogError("User messages are not set", 10);
            return string.Empty;
        }
        for (int i = 0; i < _UserMessages.Length; i++)
        {
            if (_UserMessages[i]._MessageText != null && _UserMessages[i]._MessageText.Length != 0 && _UserMessages[i]._Accuracy.IsInRange(Accuracy))
            {
                return _UserMessages[i]._MessageText[UnityEngine.Random.Range(0, _UserMessages[i]._MessageText.Length)].GetLocalizedString();
            }
        }
        UtDebug.LogError("User messages are not set", 10);
        return string.Empty;
    }

    void StartBonusLevel()
    {
        if (mIsInBonusLevel)
        {
            return;
        }
        SnChannel.StopPool("MusicEelSFX_Pool");
        if (_SndGameBonusLevelStartSFX != null)
        {
            SnChannel.Play(_SndGameBonusLevelStartSFX, "DEFAULT_POOL", true);
        }
        if (_SndGameBonusMusic)
        {
            SnChannel.Play(_SndGameBonusMusic, "MusicEelSFX_Pool", true);
        }
        mPerfectLevelCount++;
        mIsInBonusLevel = true;
        if (_DragonExcitedAnims != null && _DragonExcitedAnims.Length != 0)
        {
            PlayAnimation(_DragonExcitedAnims[UnityEngine.Random.Range(0, _DragonExcitedAnims.Length)], false);
        }
        _HUD.StartBonusLevelIntro();
    }

    bool IsElegibleForBonusLevel()
    {
        return !mIsInBonusLevel && !mWrongEelClicked;
    }

    void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
    {
        if (inType != WsServiceType.APPLY_PAYOUT)
        {
            if (inType == WsServiceType.GET_DISPLAYNAME_BY_USER_ID && inEvent == WsServiceEvent.COMPLETE && inObject != null && !string.IsNullOrEmpty((string)inObject))
            {
                string text = _ChallengeCompleteText.GetLocalizedString();
                text = text.Replace("[Name]", (string)inObject);
                SetChallengeText(text);
            }
            return;
        }
        if (inEvent == WsServiceEvent.COMPLETE)
        {
            AchievementReward[] array = null;
            KAUICursorManager.SetDefaultCursor("Arrow", true);
            if (inObject != null)
            {
                array = (AchievementReward[])inObject;
                if (array != null)
                {
                    foreach (AchievementReward achievementReward in array)
                    {
                        int? pointTypeID = achievementReward.PointTypeID;
                        if (pointTypeID != null)
                        {
                            int valueOrDefault = pointTypeID.GetValueOrDefault();
                            if (valueOrDefault != 2)
                            {
                                if (valueOrDefault == 8)
                                {
                                    SanctuaryManager.pInstance.AddXP(achievementReward.Amount.Value);
                                }
                            }
                            else
                            {
                                Money.AddToGameCurrency(achievementReward.Amount.Value);
                            }
                        }
                    }
                }
            }
            _EndDBUI.SetRewardDisplay(array);
            return;
        }
        if (inEvent != WsServiceEvent.ERROR)
        {
            return;
        }
        KAUICursorManager.SetCursor("Arrow", true);
        UtDebug.Log("reward data is null!!!");
    }

    void ShowQuitMessage()
    {
        if (mGenericDBUi != null)
        {
            UnityEngine.Object.Destroy(mGenericDBUi.gameObject);
        }
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSmSocial", true));
        mGenericDBUi = (KAUIGenericDB)gameObject.GetComponent("KAUIGenericDB");
        mGenericDBUi.SetExclusive();
        mGenericDBUi._MessageObject = base.gameObject;
        mGenericDBUi.SetText(_EelBlastQuitMessage.GetLocalizedString(), false);
        mGenericDBUi.SetButtonVisibility(true, true, false, false);
        mGenericDBUi._YesMessage = "ExitGame";
        mGenericDBUi._NoMessage = "OkMessage";
        if (MissionManager.pInstance != null)
        {
            MissionManager.pInstance.SetTimedTaskUpdate(false, false);
        }
        mCommonVariables._Paused = true;
        _HUD.SetInteractive(false);
    }

    void PlayAnimation(string AnimName, bool loop = false)
    {
        if (AnimName == string.Empty)
        {
            return;
        }
        _CurrentPet.PlayAnimation(AnimName, loop ? WrapMode.Loop : WrapMode.Once, 1f, 0.3f);
        if (_CurrentPet.animation[AnimName] != null && !loop)
        {
            mStopAnimationAfter = _CurrentPet.animation[AnimName].length;
        }
        if (loop)
        {
            mStopAnimationAfter = 0f;
        }
    }

    void StopAnimation()
    {
        _CurrentPet.PlayAnimation(_CurrentPet.GetIdleAnimationName(), WrapMode.Loop, 1f, 0.3f);
        mStopAnimationAfter = 0f;
    }

    void PlayJumpAnimation(float time)
    {
        if (_SndJumpSound)
        {
            SnChannel.Play(_SndJumpSound, "DEFAULT_POOL", true);
        }
        _CurrentPet.PlayAnimation(_DragonJumpAnimNames[0], WrapMode.Once, 1f / _DragonJumpAnimTimes[0], 0.3f);
        mDragonJumpAnimIndex = 0;
        mStopAnimationAfter = time;
    }

    void PlayDuckAnimation(float time)
    {
        if (_CurrentPet.animation[_DragonDuckAnimName] != null)
        {
            _CurrentPet.PlayAnimation(_DragonDuckAnimName, WrapMode.Once, _CurrentPet.animation[_DragonDuckAnimName].length / time, 0.3f);
            mStopAnimationAfter = time;
        }
    }

    void PlayDragonCelebration()
    {
        if (!(_CurrentPet.animation[_CelebrationInfo._AnimName] != null))
        {
            UtDebug.LogError("Animation not found for dragon. Anim:" + _CelebrationInfo._AnimName, 10);
            return;
        }
        PlayAnimation(_CelebrationInfo._AnimName, false);
        WeaponManager componentInChildren = _CurrentPet.GetComponentInChildren<WeaponManager>();
        if (componentInChildren != null && _CelebrationInfo._CurrDragonInfo != null && _CelebrationInfo._CurrDragonInfo._Object != null)
        {
            mDragonFlameParticle = UnityEngine.Object.Instantiate<GameObject>(_CelebrationInfo._CurrDragonInfo._Object);
            if (_CelebrationInfo._CurrDragonInfo._CelebrationSfx != null)
            {
                SnChannel.Play(_CelebrationInfo._CurrDragonInfo._CelebrationSfx, "DEFAULT_POOL", true);
            }
            mDragonFlameParticle.transform.parent = componentInChildren.ShootPointTrans;
            mDragonFlameParticle.transform.localPosition = Vector3.zero;
            mDragonFlameParticle.transform.localRotation = Quaternion.identity;
            return;
        }
        UtDebug.Log("The head bone was not found " + _CurrentPet.GetHeadBoneName());
    }

    void OkMessage()
    {
        UnityEngine.Object.Destroy(mGenericDBUi.gameObject);
        if (MissionManager.pInstance != null)
        {
            MissionManager.pInstance.SetTimedTaskUpdate(true, true);
        }
        mCommonVariables._Paused = false;
        _HUD.SetInteractive(true);
    }

    void OnYesLowEnergyMessage()
    {
        OnReplayGame();
    }

    void OnNoLowEnergyMessage()
    {
        mCommonVariables._Paused = false;
        _HUD.SetInteractive(true);
        _EndDBUI.SetVisibility(true);
    }

    void OnWaveTouched(Vector3 PointOfContact)
    {
        if (mIsJumping)
        {
            return;
        }
        if (_PrtWaveHit != null)
        {
            UnityEngine.Object.Instantiate<GameObject>(_PrtWaveHit, PointOfContact, _PrtWaveHit.transform.rotation).transform.parent = _PrtRoot.transform;
        }
        if (_SndWaveHit != null)
        {
            SnChannel.Play(_SndWaveHit, "DEFAULT_POOL", true);
        }
        mCurrLifes--;
        if (mCurrLifes <= 0)
        {
            mDragonHitStartPoint = _CurrentPet.transform.position;
            mDragonHitControlPoint = EBEel.DoBezierReverse(mDragonHitStartPoint, _PetMiddleMarkerOnWave.transform.position, _PetFinalMarkerOnWave.transform.position);
            mDragonHitEndPoint = _PetFinalMarkerOnWave.transform.position;
            mIsDragonSlippingIntoWater = true;
            mIsPetKnockedOut = true;
            StopAnimation();
            PlayAnimation(_DragonSwimAnim, true);
            return;
        }
        PlayAnimation(_WaveHitAnim, false);
    }

    void OnTerrorTouched(Vector3 PointOfContact)
    {
        if (mIsDucking)
        {
            return;
        }
        if (_PrtTerrorHit != null)
        {
            UnityEngine.Object.Instantiate<GameObject>(_PrtTerrorHit, PointOfContact, _PrtTerrorHit.transform.rotation).transform.parent = _PrtRoot.transform;
        }
        if (_SndTerrorHit != null)
        {
            SnChannel.Play(_SndTerrorHit, "DEFAULT_POOL", true);
        }
        mCurrLifes--;
        if (mCurrLifes <= 0)
        {
            mDragonHitStartPoint = _CurrentPet.transform.position;
            mDragonHitControlPoint = EBEel.DoBezierReverse(mDragonHitStartPoint, _MiddleMarkerOnTerrorHit.transform.position, _FinalPetMarkerOnTerrorHit.transform.position);
            mDragonHitEndPoint = _FinalPetMarkerOnTerrorHit.transform.position;
            mIsDragonSlippingIntoWater = true;
            mIsPetKnockedOut = true;
            StopAnimation();
            PlayAnimation(_DragonSwimAnim, true);
            return;
        }
        PlayAnimation(_TerrorHitAnim, false);
    }

    void ResetDragon()
    {
        if (_PetMarker != null)
        {
            _CurrentPet.transform.position = _PetMarker.position;
            _CurrentPet.transform.rotation = _PetMarker.rotation;
            _CurrentPet.transform.forward = _PetMarker.forward;
        }
        StopAnimation();
    }

    public EBGameManager()
    {
    }

    public UiEBStartInfo _UiStartInfo;

    public UiCountDown _UiCountDown;

    public UiEBHUD _HUD;

    public UiEBNextLevel _UiNextLevel;

    public UiDragonsEndDB _EndDBUI;

    public int _MaxLifes = 3;

    public Camera _MainCamera;

    public Transform _PetMarker;

    public GameObject _eelObject;

    public GameObject _BonusEelObject;

    public GameObject _ElectricEelObject;

    public GameObject _PrtRoot;

    public GameObject _WaterHeightMarker;

    public GameObject _ScoreGO;

    public GameObject _PrtEelsElectrified;

    public EBGameManager.FireDataByPetType[] _FireDataByPetType;

    public Transform[] _SpawnPoints;

    public Transform[] _DidYouKnowSpawnPoints;

    public EBGameManager.EBEelColorValue[] _EelColorValues;

    public EBGameManager.LevelProps[] _Levels;

    public EBGameManager.LevelProps[] _DeltaLevel;

    public EBGameManager.LevelProps _MaxLevel;

    public int _MaxLevelsToGenerate = 25;

    public float _CountDownTime = 2f;

    public int _PairID;

    public Color _WaterShderWhileGrowth = Color.blue;

    public float _WaterShaderColorChangeTime = 1f;

    public Vector2 _PetMeterAnchorOffset = new Vector2(-0.06f, 0.09f);

    public float _HappinessIncreaseOnEachLevel = 0.01f;

    public float _EnergyChangeOnStart = 0.01f;

    public float _MinEnergyToReplay = 0.1f;

    public float _MinHappinessToReplay = 0.1f;

    public LocaleString _EelBlastQuitMessage = new LocaleString("Do You Want to Quit");

    public int _PerfectLevelBonus = 50;

    public EBGameManager.UserMessages[] _UserMessages;

    public string[] _DragonExcitedAnims;

    public string _DragonSwimAnim;

    public string _DragonAttackAnimName = "Attack02";

    public string _GameModuleName = "DOEelBlast";

    public int _GameID = 57;

    public int _GoldMultiplierForMembers = 2;

    public float _ParticleTravelTime = 0.1f;

    public int _DisplayBonusInfoAterLevel = 1;

    public int _DisplayElectricInfoAfterLevel = 2;

    public int _DisplayWaveInfoAfterLevel = 2;

    public int _DisplayTerrorInfoAfterLevel = 2;

    public string _DragonSadAnimName = "IdleSad";

    public LocaleString _NonMemberResultText = new LocaleString("Members earn twice as many coins!");

    public LocaleString _MemberResultText = new LocaleString("");

    public int _GameMusicRepeatInterval = 5;

    public float _GameMusicVolume = 1f;

    public AudioClip _SndAmbientMusic;

    public AudioClip[] _SndGameMusic;

    public AudioClip _SndGameBonusMusic;

    public AudioClip _SndGameEndMusic;

    public AudioClip[] _SndGamePerfectEndMusic;

    public AudioClip _SndGameBonusLevelStartSFX;

    public AudioClip _SndDidYouKnow;

    public AudioClip _SndJumpSound;

    public AudioClip _SndCountDown;

    public AudioClip _SndEelHit;

    public AudioClip _SndBonusEelHit;

    public AudioClip _SndElectricEelHit;

    public AudioClip _SndDragonFire;

    public AudioClip _SndPositiveScore;

    public AudioClip _SndNegativeScore;

    public AudioClip _SndLowEnergyVO;

    public EBGameManager.DragonCelebrationInfo _CelebrationInfo;

    public float _DragonAttackAnimSpeed = 2f;

    public bool _DisableMoodParticle = true;

    [HideInInspector]
    public SanctuaryPet _CurrentPet;

    [HideInInspector]
    public List<EBEel> _CurrentActiveEels;

    [HideInInspector]
    public EBGameManager.EBEelGroup[] _CurrEelGroups;

    [HideInInspector]
    public EBGameManager.GameState _GameState;

    EBGameManager.GameState mPrevGameState;

    PairData mPairData;

    EBGameManager.EBCommonVariables mCommonVariables = new EBGameManager.EBCommonVariables();

    EBGameManager.LevelProps mCurrLevel;

    float mStopAnimationAfter;

    bool mIsInBonusLevel;

    int mCurrLifes;

    bool mIsGameOver;

    float mCurrCountDownTime;

    bool mIsStoreOpen;

    bool mInitialized;

    protected Camera mOldCam;

    protected bool mOldHover;

    protected bool mOldFly;

    protected bool mOldEnablePetAnim;

    protected string mOldIdleAnimationName;

    KAUIGenericDB mGenericDBUi;

    GameObject mDragonFlameParticle;

    int mCurrentLevelRightEelsClicked;

    int mCurrentLevelWrongEelsClicked;

    int mCurrentLevelBonusEelsClicked;

    int mCurrentLevelElectricEelsClicked;

    int mTotalRightEelsClicked;

    int mTotalWrongEelsClicked;

    int mTotalBonusEelsClicked;

    int mTotalElectricEelsClicked;

    int mCurrScore;

    int mTotalScore;

    bool mWrongEelClicked;

    int mTotalLevelsCompleted;

    int mCurrLevelIndex;

    int mPerfectLevelCount;

    public EBWave _Wave;

    public Transform _PetJumpHeightMarker;

    public Transform _PetMiddleMarkerOnWave;

    public Transform _PetFinalMarkerOnWave;

    public float _WaveJumpTime = 3f;

    public float _DragonSlipTravelTime = 2f;

    public GameObject _PrtWaveHit;

    public string _WaveHitAnim;

    public AudioClip _SndWaveHit;

    Vector3 mHeightControlPoint = Vector3.zero;

    Vector3 mDragonHitControlPoint = Vector3.zero;

    Vector3 mDragonHitEndPoint = Vector3.zero;

    Vector3 mDragonHitStartPoint = Vector3.zero;

    float mCurrSlippingTravelTime;

    float mCurrentJumpTime;

    bool mIsJumping;

    bool mWaveMode;

    bool mIsDragonSlippingIntoWater;

    bool mIsPetKnockedOut;

    public string _DragonJumpAnimName;

    public string[] _DragonJumpAnimNames;

    public float[] _DragonJumpAnimTimes;

    int mDragonJumpAnimIndex = -1;

    public EBTerror _Terror;

    public Collider _ColliderForTerrorCollision;

    public Transform _MiddleMarkerOnTerrorHit;

    public Transform _FinalPetMarkerOnTerrorHit;

    public float _TerrorDuckTime = 3f;

    public GameObject _PrtTerrorHit;

    public string _TerrorHitAnim;

    public AudioClip _SndTerrorHit;

    float mCurrDuckTime;

    bool mIsDucking;

    bool mTerrorMode;

    public string _DragonDuckAnimName;

    public LocaleString[] _DidYouKnowText;

    bool mIsDidYouKnowAllowed = true;

    bool mIsDidYouKnowHidden = true;

    bool mClearCurrentActiveEels = true;

    ObClickableEelBlast mObClickableEel;

    public Texture2D _ReticleTex;

    public LocaleString _ChallengeCompleteText = new LocaleString("Amazing! you beat [Name]'s challenge");

    public LocaleString _ChallengeTryAgainText = new LocaleString("Nice Try! Why dont you try again.");

    public float _ChallengeFlashTimeInterval;

    public int _ChallengeFlashLoopCount;

    public AudioClip _ChallengeCompleteSFX;

    int mChallengePoints;

    bool mChallengeAchieved;

    [Serializable]
    public class LevelProps
    {
        public LevelProps(EBGameManager.LevelProps props)
        {
            _EelScore = props._EelScore;
            _BonusEelScore = props._BonusEelScore;
            _EelHeightMin = props._EelHeightMin;
            _EelHeightMax = props._EelHeightMax;
            _BonusEelHeightMin = props._BonusEelHeightMin;
            _BonusEelHeightMax = props._BonusEelHeightMax;
            _EelDelayOnTop = props._EelDelayOnTop;
            _EelSpeedMin = props._EelSpeedMin;
            _EelSpeedMax = props._EelSpeedMax;
            _BonusEelDelayOnTop = props._BonusEelDelayOnTop;
            _BonusEelSpeedMin = props._BonusEelSpeedMin;
            _BonusEelSpeedMax = props._BonusEelSpeedMax;
            _EelFrequency = props._EelFrequency;
            _ElectricEelScore = props._ElectricEelScore;
            _MinCurrectEelsForNextLevel = props._MinCurrectEelsForNextLevel;
            _CorrectEel = props._CorrectEel;
            _ScoreForOtherEelsOnBombed = props._ScoreForOtherEelsOnBombed;
            _WaveCount = new MinMax(props._WaveCount.Min, props._WaveCount.Max);
            _TimeBetweenWaves = new MinMax(props._TimeBetweenWaves.Min, props._TimeBetweenWaves.Max);
            _WaveTravelTime = new MinMax(props._WaveTravelTime.Min, props._WaveTravelTime.Max);
            _RemainingWaveCount = props._RemainingWaveCount;
            _RemainingWaveTime = props._RemainingWaveTime;
            _TerrorCount = new MinMax(props._TerrorCount.Min, props._TerrorCount.Max);
            _TimeBetweenTerors = new MinMax(props._TimeBetweenTerors.Min, props._TimeBetweenTerors.Max);
            _TerrorTravelTime = new MinMax(props._TerrorTravelTime.Min, props._TerrorTravelTime.Max);
            _RemainingTerrorCount = props._RemainingTerrorCount;
            _RemainingTerrorTime = props._RemainingTerrorTime;
            _HappinessOnBonusEel = props._HappinessOnBonusEel;
            _EelGroups = new EBGameManager.EBEelGroup[props._EelGroups.Length];
            for (int i = 0; i < _EelGroups.Length; i++)
            {
                _EelGroups[i] = new EBGameManager.EBEelGroup();
                _EelGroups[i]._EelColor = props._EelGroups[i]._EelColor;
                _EelGroups[i]._EelType = props._EelGroups[i]._EelType;
                _EelGroups[i]._Count = props._EelGroups[i]._Count;
                _EelGroups[i]._BurntCount = props._EelGroups[i]._BurntCount;
                _EelGroups[i]._CorrectEelGroup = props._EelGroups[i]._CorrectEelGroup;
                _EelGroups[i]._ColorValue = props._EelGroups[i]._ColorValue;
                _EelGroups[i]._ColorName = props._EelGroups[i]._ColorName;
                _EelGroups[i]._MaxCount = props._EelGroups[i]._MaxCount;
            }
            _EelBonusGroup = new EBGameManager.EBEelGroup();
            _EelBonusGroup._EelColor = props._EelBonusGroup._EelColor;
            _EelBonusGroup._EelType = props._EelBonusGroup._EelType;
            _EelBonusGroup._Count = props._EelBonusGroup._Count;
            _EelBonusGroup._BurntCount = props._EelBonusGroup._BurntCount;
            _EelBonusGroup._CorrectEelGroup = props._EelBonusGroup._CorrectEelGroup;
            _EelBonusGroup._ColorValue = props._EelBonusGroup._ColorValue;
            _EelBonusGroup._ColorName = props._EelBonusGroup._ColorName;
            _EelBonusGroup._MaxCount = props._EelBonusGroup._MaxCount;
        }

        public void AddProps(LevelProps props, LevelProps MaxProps)
        {
            _EelScore = Mathf.Min(_EelScore + props._EelScore, MaxProps._EelScore);
            _BonusEelScore = Mathf.Min(_BonusEelScore + props._BonusEelScore, MaxProps._BonusEelScore);
            _EelHeightMin = Mathf.Min(_EelHeightMin + props._EelHeightMin, MaxProps._EelHeightMin);
            _EelHeightMax = Mathf.Min(_EelHeightMax + props._EelHeightMax, MaxProps._EelHeightMax);
            _BonusEelHeightMin = Mathf.Min(_BonusEelHeightMin + props._BonusEelHeightMin, MaxProps._BonusEelHeightMin);
            _BonusEelHeightMax = Mathf.Min(_BonusEelHeightMax + props._BonusEelHeightMax, MaxProps._BonusEelHeightMax);
            _EelDelayOnTop = Mathf.Min(_EelDelayOnTop + props._EelDelayOnTop, MaxProps._EelDelayOnTop);
            _EelSpeedMin = Mathf.Min(_EelSpeedMin + props._EelSpeedMin, MaxProps._EelSpeedMin);
            _EelSpeedMax = Mathf.Min(_EelSpeedMax + props._EelSpeedMax, MaxProps._EelSpeedMax);
            _BonusEelDelayOnTop = Mathf.Min(_BonusEelDelayOnTop + props._BonusEelDelayOnTop, MaxProps._BonusEelDelayOnTop);
            _BonusEelSpeedMin = Mathf.Min(_BonusEelSpeedMin + props._BonusEelSpeedMin, MaxProps._BonusEelSpeedMin);
            _BonusEelSpeedMax = Mathf.Min(_BonusEelSpeedMax + props._BonusEelSpeedMax, MaxProps._BonusEelSpeedMax);
            _EelFrequency = Mathf.Min(_EelFrequency + props._EelFrequency, MaxProps._EelFrequency);
            _ElectricEelScore = Mathf.Min(_ElectricEelScore + props._ElectricEelScore, MaxProps._ElectricEelScore);
            _MinCurrectEelsForNextLevel = Mathf.Min(_MinCurrectEelsForNextLevel + props._MinCurrectEelsForNextLevel, MaxProps._MinCurrectEelsForNextLevel);
            _ScoreForOtherEelsOnBombed = Mathf.Min(_ScoreForOtherEelsOnBombed + props._ScoreForOtherEelsOnBombed, MaxProps._ScoreForOtherEelsOnBombed);
            _WaveCount.Min = Mathf.Min(_WaveCount.Min + props._WaveCount.Min, MaxProps._WaveCount.Min);
            _WaveCount.Max = Mathf.Min(_WaveCount.Max + props._WaveCount.Max, MaxProps._WaveCount.Max);
            _TimeBetweenWaves.Min = Mathf.Max(_TimeBetweenWaves.Min + props._TimeBetweenWaves.Min, MaxProps._TimeBetweenWaves.Min);
            _TimeBetweenWaves.Max = Mathf.Max(_TimeBetweenWaves.Max + props._TimeBetweenWaves.Max, MaxProps._TimeBetweenWaves.Max);
            _WaveTravelTime.Min = Mathf.Max(_WaveTravelTime.Min + props._WaveTravelTime.Min, MaxProps._WaveTravelTime.Min);
            _WaveTravelTime.Max = Mathf.Max(_WaveTravelTime.Max + props._WaveTravelTime.Max, MaxProps._WaveTravelTime.Max);
            _TerrorCount.Min = Mathf.Min(_TerrorCount.Min + props._TerrorCount.Min, MaxProps._TerrorCount.Min);
            _TerrorCount.Max = Mathf.Min(_TerrorCount.Max + props._TerrorCount.Max, MaxProps._TerrorCount.Max);
            _TimeBetweenTerors.Min = Mathf.Max(_TimeBetweenTerors.Min + props._TimeBetweenTerors.Min, MaxProps._TimeBetweenTerors.Min);
            _TimeBetweenTerors.Max = Mathf.Max(_TimeBetweenTerors.Max + props._TimeBetweenTerors.Max, MaxProps._TimeBetweenTerors.Max);
            _TerrorTravelTime.Min = Mathf.Max(_TerrorTravelTime.Min + props._TerrorTravelTime.Min, MaxProps._TerrorTravelTime.Min);
            _TerrorTravelTime.Max = Mathf.Max(_TerrorTravelTime.Max + props._TerrorTravelTime.Max, MaxProps._TerrorTravelTime.Max);
            _HappinessOnBonusEel = Mathf.Min(_HappinessOnBonusEel + props._HappinessOnBonusEel, MaxProps._HappinessOnBonusEel);
        }

        public EBEelGroup[] _EelGroups;

        public EBEelGroup _EelBonusGroup;

        [NonSerialized]
        public EBEelGroup _CorrectGroup;

        [NonSerialized]
        public EBEelColor _CorrectEel;

        public int _MinCurrectEelsForNextLevel;

        public MinMax _WaveCount;

        public MinMax _TimeBetweenWaves;

        public MinMax _WaveTravelTime;

        [NonSerialized]
        public int _RemainingWaveCount;

        [NonSerialized]
        public float _RemainingWaveTime;

        public MinMax _TerrorCount;

        public MinMax _TimeBetweenTerors;

        public MinMax _TerrorTravelTime;

        [NonSerialized]
        public int _RemainingTerrorCount;

        [NonSerialized]
        public float _RemainingTerrorTime;

        public int _EelScore;

        public int _BonusEelScore;

        public int _ElectricEelScore = -30;

        public int _ScoreForOtherEelsOnBombed = 10;

        public float _HappinessOnBonusEel = 1f;

        public float _EelHeightMin;

        public float _EelHeightMax;

        public float _BonusEelHeightMin;

        public float _BonusEelHeightMax;

        public float _EelDelayOnTop;

        public float _EelSpeedMin;

        public float _EelSpeedMax;

        public float _BonusEelDelayOnTop;

        public float _BonusEelSpeedMin;

        public float _BonusEelSpeedMax;

        public int _EelFrequency;
    }

    public enum GameState
    {
        LOADING_ASSETS,
        WAITING_INFO,
        LOADING_LEVEL,
        COUNT_DOWN,
        INGAME,
        GAME_END
    }

    [Serializable]
    public enum EBEelColor
    {
        COLOR_1,
        COLOR_2,
        COLOR_3,
        COLOR_4,
        COLOR_5
    }

    [Serializable]
    public enum EBEelType
    {
        NORMAL,
        BONUS,
        ELECTRIC
    }

    [Serializable]
    public class EBEelColorValue
    {
        public EBEelColorValue()
        {
        }

        public EBGameManager.EBEelColor _Color;

        public Color _ColorValue;

        public LocaleString _ColorNameText;
    }

    [Serializable]
    public class EBEelGroup
    {
        public EBEelGroup()
        {
        }

        public EBGameManager.EBEelType _EelType;

        public int _Count;

        public bool _CorrectEelGroup;

        [HideInInspector]
        public EBGameManager.EBEelColor _EelColor;

        [HideInInspector]
        public int _BurntCount;

        [HideInInspector]
        public int _InAirCount;

        [HideInInspector]
        public int _MaxCount;

        [HideInInspector]
        public Color _ColorValue;

        [HideInInspector]
        public string _ColorName;
    }

    [Serializable]
    public class EBDragonSpecificInfo
    {
        public EBDragonSpecificInfo()
        {
        }

        public string _DragonsName;

        public int _DragonID;

        public GameObject _Object;

        public AudioClip _CelebrationSfx;
    }

    [Serializable]
    public class UserMessages
    {
        public UserMessages()
        {
        }

        public MinMax _Accuracy;

        public LocaleString[] _MessageText;
    }

    [Serializable]
    public class DragonCelebrationInfo
    {
        public DragonCelebrationInfo()
        {
        }

        public string _AnimName;

        public EBGameManager.EBDragonSpecificInfo[] _DragonCelebrationParticles;

        [NonSerialized]
        public EBGameManager.EBDragonSpecificInfo _CurrDragonInfo;
    }

    [Serializable]
    public class FireDataByPetType
    {
        public FireDataByPetType()
        {
        }

        public string _DragonsName;

        public int _TypeID;

        public GameObject _AmmoPrt;

        public GameObject _DragonMouthFire;
    }

    public class EBCommonVariables
    {
        public EBCommonVariables()
        {
        }

        public bool _Touchable;

        public bool _Paused;
    }

    public delegate void UpdateFunction();
}
