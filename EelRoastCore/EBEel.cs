using System;
using System.Collections;
using UnityEngine;

public class EBEel : KAMonoBase
{
    public Transform _RenderPath;
    public Renderer _Renderer;
    public GameObject _PrtBlast;
    public float _DestroyDelay = 0.3f;
    public GameObject[] _WaterSplashEffects;
    public string _LaunchAnimName = "Launch";
    public string _IdleAnimName = "Idle";
    public Transform _Eel;
    public float _LauchCrossFadeTime = 0.3f;
    public AudioClip _SndWaterSplash;
    public Transform _StartPoint;
    public Transform _EndPoint;
    public Vector3 _TopPoint;
    public bool _Active;
    public EBGameManager.EBEelGroup _Group;
    public float _Speed;
    public float _DelayOnTop;
    public OnEvent _OnClicked;
    public OnEvent _OnDestroyed;
    public GameObject _ParticleRoot;
    public GameObject _WaterHeightMarker;
    float mLamdaPosition;
    bool mIsDestroying;
    Vector3 mControlPoint = Vector3.zero;
    float mDelayTime;
    bool mPlayedPartForGoingAbove;
    bool mPlayedPartForGoingBelow;
    bool mIdleAnimationPlayed;
    EBGameManager.EBCommonVariables mCommonVariables;
    Quaternion mRotFrom;
    Quaternion mRotTo;

    public bool pIsDestroying => mIsDestroying;

    public void Initialize(EBGameManager.EBCommonVariables touchable)
    {
        mCommonVariables = touchable;
        _Group._InAirCount++;
        _Group._Count--;
        if (_Group._EelType == EBGameManager.EBEelType.NORMAL && _Renderer)
            _Renderer.material.color = _Group._ColorValue;
        transform.position = _StartPoint.position;
        transform.rotation = _StartPoint.rotation;
        transform.LookAt(_TopPoint);
        mRotFrom.eulerAngles = transform.rotation.eulerAngles;
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        eulerAngles.x = -transform.rotation.eulerAngles.x;
        mRotTo.eulerAngles = eulerAngles;
        mControlPoint = DoBezierReverse(_StartPoint.position, _TopPoint, _EndPoint.position);
    }

    public void SetColor(Color color)
    {
        if (_Renderer)
            _Renderer.material.color = color;
    }

    void Update()
    {
        if (mCommonVariables._Paused)
        {
            return;
        }
        if (_Active != _RenderPath.gameObject.activeSelf)
        {
            if (_Active && _StartPoint != null)
            {
                transform.position = _StartPoint.position;
                transform.rotation = _StartPoint.rotation;
            }
            _RenderPath.gameObject.SetActive(_Active);
        }
        if (_Active)
        {
            Animation component = _Eel.GetComponent<Animation>();
            if (!mPlayedPartForGoingAbove && MultiplyVectors(transform.position, Vector3.up) > MultiplyVectors(_WaterHeightMarker.transform.position, Vector3.up))
            {
                if (_WaterSplashEffects != null)
                {
                    GameObject gameObject = _WaterSplashEffects[UnityEngine.Random.Range(0, _WaterSplashEffects.Length)];
                    Instantiate(gameObject, transform.position, gameObject.transform.rotation).transform.parent = _ParticleRoot.transform;
                    if (_SndWaterSplash)
                        SnChannel.Play(_SndWaterSplash, "DEFAULT_POOL", true);
                }
                component.CrossFade(_LaunchAnimName, 0.3f);
                component[_LaunchAnimName].wrapMode = WrapMode.Once;
                mPlayedPartForGoingAbove = true;
            }
            if (mPlayedPartForGoingAbove && !mPlayedPartForGoingBelow)
            {
                if (!mIdleAnimationPlayed && component.IsPlaying(_LaunchAnimName) && component[_LaunchAnimName].time + _LauchCrossFadeTime > component[_LaunchAnimName].length)
                {
                    mIdleAnimationPlayed = true;
                    component.CrossFade(_IdleAnimName, _LauchCrossFadeTime);
                    component[_IdleAnimName].wrapMode = WrapMode.Loop;
                }
                if (MultiplyVectors(transform.position, Vector3.up) < MultiplyVectors(_WaterHeightMarker.transform.position, Vector3.up))
                {
                    if (_WaterSplashEffects != null)
                    {
                        GameObject gameObject2 = _WaterSplashEffects[UnityEngine.Random.Range(0, _WaterSplashEffects.Length)];
                        Instantiate(gameObject2, transform.position, gameObject2.transform.rotation).transform.parent = _ParticleRoot.transform;
                        if (_SndWaterSplash)
                            SnChannel.Play(_SndWaterSplash, "DEFAULT_POOL", true);
                    }
                    mPlayedPartForGoingBelow = true;
                }
            }
            if (mLamdaPosition < 1f)
            {
                transform.rotation = Quaternion.Lerp(mRotFrom, mRotTo, mLamdaPosition);
                transform.position = DoBezier(_StartPoint.position, mControlPoint, _EndPoint.position, mLamdaPosition);
                if (mLamdaPosition > 0.5f && mDelayTime < _DelayOnTop)
                    mDelayTime += Time.deltaTime;
                else
                    mLamdaPosition += _Speed * Time.deltaTime;
                if (mLamdaPosition >= 1f)
                {
                    mLamdaPosition = 1f;
                    StartCoroutine(DestroySlowly(_DestroyDelay));
                }
            }
        }
    }

    public void MarkAsDestroying()
    {
        if (mIsDestroying)
        {
            UtDebug.LogError("The Eel is already destroying. Shouldnt be here");
            return;
        }
        mIsDestroying = true;
        _Group._BurntCount++;
        SetColor(Color.black);
    }

    public void MarkAsDestroyedForce(GameObject externalParticlesToFire)
    {
        if (!mIsDestroying)
            return;
        SetColor(Color.black);
        if (_PrtBlast != null)
        {
            if (externalParticlesToFire != null)
            {
                GameObject gameObject = Instantiate(externalParticlesToFire, _PrtBlast.transform.position, externalParticlesToFire.transform.rotation);
                if (gameObject)
                    gameObject.transform.parent = _PrtBlast.transform.parent;
            }
            else
                _PrtBlast.SetActive(true);
        }
        StartCoroutine(DestroySlowly(_DestroyDelay));
    }

    void MarkAsDestroyed()
    {
        if (!mIsDestroying)
            return;
        SetColor(Color.black);
        _PrtBlast.SetActive(true);
        StartCoroutine(DestroySlowly(_DestroyDelay));
    }

    void OnClick(GameObject colliderObj)
    {
        if (_Active && !mIsDestroying && mCommonVariables._Touchable && !mCommonVariables._Paused)
            _OnClicked(this);
    }

    IEnumerator DestroySlowly(float time)
    {
        yield return new WaitForSeconds(time);
        _OnDestroyed(this);
        _Group._InAirCount--;
        if (gameObject != null)
            Destroy(gameObject);
        yield break;
    }

    public static Vector3 DoBezier(Vector3 Start, Vector3 Control, Vector3 End, float lamda)
    {
        if (lamda <= 0f)
            return Start;
        if (lamda >= 1f)
            return End;
        return (1f - lamda) * (1f - lamda) * Start + 2f * lamda * (1f - lamda) * Control + lamda * lamda * End;
    }

    public static Vector3 DoBezierReverse(Vector3 Start, Vector3 Mid, Vector3 End) => (Mid - 0.25f * Start - 0.25f * End) / 0.5f;

    float MultiplyVectors(Vector3 a, Vector3 b) => a.x * b.x + a.y * b.y + a.z * b.z;

    public delegate void OnEvent(EBEel eel);
}
