using UnityEngine;

public class EBWave : MonoBehaviour
{
    private void Start()
    {
    }

    public void StartWave(Collider colliderToCollide, float TimeOfTravel)
    {
        this.mStarted = true;
        this.mTouchSignalSend = false;
        this.mTimeOfTravel = TimeOfTravel;
        this.mPet = colliderToCollide;
        base.gameObject.SetActive(true);
        base.transform.position = this._StartMarker.position;
        this.mCurrWaitTime = 0f;
        this.mCurrTime = 0f;
        this.mWaveStartFirstTime = true;
        if (this._WaveWarningSFX)
        {
            SnChannel.Play(this._WaveWarningSFX, "DEFAULT_POOL", true);
        }
    }

    public void StopWave()
    {
        base.gameObject.SetActive(false);
        base.transform.position = this._StartMarker.position;
        this.mStarted = false;
    }

    private void Update()
    {
        if (!this.mStarted || this._CommonVariables._Paused)
        {
            return;
        }
        if (this.mCurrWaitTime < this._WaitTime)
        {
            this.mCurrWaitTime += Time.deltaTime;
            return;
        }
        if (this.mWaveStartFirstTime)
        {
            this.mWaveStartFirstTime = false;
            if (this._WaveSFX)
            {
                SnChannel.Play(this._WaveSFX, "DEFAULT_POOL", true);
            }
        }
        this.mCurrTime += Time.deltaTime;
        base.transform.position = Vector3.Lerp(this._StartMarker.position, this._EndMarker.position, this.mCurrTime / this.mTimeOfTravel);
        if (this.mCurrTime > this.mTimeOfTravel)
        {
            this.StopWave();
        }
    }

    private void OnCollisionEnter(Collision inCollision)
    {
        Vector3 a = Vector3.zero;
        int num = 0;
        if (!this.mTouchSignalSend && this.mPet == inCollision.collider && this._MessageObject != null)
        {
            foreach (ContactPoint contactPoint in inCollision.contacts)
            {
                a += contactPoint.point;
                num++;
            }
            this._MessageObject.SendMessage("OnWaveTouched", a / (float)num);
            this.mTouchSignalSend = true;
        }
    }

    private void OnTriggerEnter(Collider OtherCollider)
    {
        if (!this.mTouchSignalSend && this.mPet == OtherCollider && this._MessageObject != null)
        {
            this._MessageObject.SendMessage("OnWaveTouched", base.gameObject.GetComponent<Collider>().ClosestPointOnBounds(OtherCollider.gameObject.transform.position));
            this.mTouchSignalSend = true;
        }
    }

    public EBWave()
    {
    }

    public Transform _StartMarker;

    public Transform _EndMarker;

    public GameObject _MessageObject;

    public AudioClip _WaveWarningSFX;

    public AudioClip _WaveSFX;

    public float _WaitTime = 1f;

    private float mTimeOfTravel;

    private float mCurrTime;

    private bool mStarted;

    private Collider mPet;

    private float mCurrWaitTime;

    private bool mWaveStartFirstTime = true;

    private bool mTouchSignalSend;

    public EBGameManager.EBCommonVariables _CommonVariables;
}
