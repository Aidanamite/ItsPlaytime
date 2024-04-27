using UnityEngine;

public class EBTerror : MonoBehaviour
{
    public void Start()
    {
    }

    public void Start(Collider colliderToCollide, float TimeOfTravel, Vector3 DragonHeadPosition)
    {
        this.mIsStarted = true;
        this.mIsTouchSignalSent = false;
        this.mTimeOfTravel = TimeOfTravel;
        this.mOtherCollider = colliderToCollide;
        base.transform.position = this._SpawnMarker.transform.position;
        base.transform.up = Vector3.up;
        base.transform.forward = this._StartMarker.position - this._SpawnMarker.position;
        Vector3 mid = DragonHeadPosition;
        mid.y += this._TerrorHeightOffset;
        this.mCurrWaitTime = 0f;
        this.mCurrTimeOfTravel = 0f;
        this.mIsFirstTimeTerrorFire = true;
        this.mMiddleControlPoint = EBEel.DoBezierReverse(this._StartMarker.position, mid, this._EndMarker.position);
        this.mSpawnMiddleControlPoint = EBEel.DoBezierReverse(this._SpawnMarker.position, this._SpawnMiddleMarker.position, this._StartMarker.position);
        if (this._WarningSFX)
        {
            SnChannel.Play(this._WarningSFX, "DEFAULT_POOL", true);
        }
        if (this.mFireParticle != null)
        {
            UnityEngine.Object.Destroy(this.mFireParticle);
        }
        base.gameObject.SetActive(true);
        this.PlayFlying();
    }

    public void Stop()
    {
        base.gameObject.SetActive(false);
        base.transform.position = this._StartMarker.position;
        base.transform.rotation = this._StartMarker.rotation;
        this.mIsStarted = false;
        this.mIsTouchSignalSent = true;
        if (this.mFireParticle != null)
        {
            UnityEngine.Object.Destroy(this.mFireParticle);
        }
    }

    private void Update()
    {
        if (!this.mIsStarted || this._CommonVariables._Paused)
        {
            return;
        }
        this.UpdateDragonRotation();
        if (this.mCurrWaitTime < this._TerrorWaitTime)
        {
            this.mCurrWaitTime += Time.deltaTime;
            base.transform.position = EBEel.DoBezier(this._SpawnMarker.position, this.mSpawnMiddleControlPoint, this._StartMarker.position, this.mCurrWaitTime / this._TerrorWaitTime);
            return;
        }
        if (this.mIsFirstTimeTerrorFire)
        {
            this.mIsFirstTimeTerrorFire = false;
            base.transform.forward = this._EndMarker.position - this._StartMarker.position;
            base.transform.up = Vector3.up;
            this.PlayFire();
        }
        this.mCurrTimeOfTravel += Time.deltaTime;
        base.transform.position = EBEel.DoBezier(this._StartMarker.position, this.mMiddleControlPoint, this._EndMarker.position, this.mCurrTimeOfTravel / this.mTimeOfTravel);
        if (this.mCurrTimeOfTravel > this.mTimeOfTravel)
        {
            this.Stop();
        }
    }

    private void UpdateDragonRotation()
    {
        if (this.mPreviousPosition == this._Terror.transform.position)
        {
            return;
        }
        base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(base.transform.position - this.mPreviousPosition), Time.deltaTime * this._DragonRotationUpdationSpeed);
        this.mPreviousPosition = base.transform.position;
    }

    private void OnCollisionEnter(Collision inCollision)
    {
        if (this.mCurrWaitTime < this._TerrorWaitTime)
        {
            return;
        }
        Vector3 a = Vector3.zero;
        int num = 0;
        if (!this.mIsTouchSignalSent && this.mOtherCollider == inCollision.collider && this._MessageObject != null)
        {
            foreach (ContactPoint contactPoint in inCollision.contacts)
            {
                a += contactPoint.point;
                num++;
            }
            this._MessageObject.SendMessage("OnTerrorTouched", a / (float)num);
            this.mIsTouchSignalSent = true;
        }
    }

    private void OnTriggerEnter(Collider OtherCollider)
    {
        if (this.mCurrWaitTime < this._TerrorWaitTime)
        {
            return;
        }
        if (!this.mIsTouchSignalSent && this.mOtherCollider == OtherCollider && this._MessageObject != null)
        {
            this._MessageObject.SendMessage("OnTerrorTouched", base.gameObject.GetComponent<Collider>().ClosestPointOnBounds(OtherCollider.gameObject.transform.position));
            this.mIsTouchSignalSent = true;
        }
    }

    private void PlayFire()
    {
        Animation component = this._Terror.GetComponent<Animation>();
        if (component[this._FireAnimName] != null)
        {
            component.CrossFade(this._FireAnimName, 0.3f);
            component[this._FireAnimName].wrapMode = WrapMode.Loop;
            if (this._FireParticleMarker != null && this._FirePrt != null)
            {
                if (this._TerrorFireSFX)
                {
                    SnChannel.Play(this._TerrorFireSFX, "DEFAULT_POOL", true);
                }
                this.mFireParticle = UnityEngine.Object.Instantiate<GameObject>((GameObject)this._FirePrt);
                this.mFireParticle.transform.parent = this._FireParticleMarker;
                this.mFireParticle.transform.localPosition = Vector3.zero;
                this.mFireParticle.transform.localRotation = Quaternion.identity;
                return;
            }
        }
        else
        {
            UtDebug.LogError("Animation not found for dragon. Anim:" + this._FireAnimName, 10);
        }
    }

    private void PlayFlying()
    {
        Animation component = this._Terror.GetComponent<Animation>();
        if (component[this._FlyingAnimName] != null)
        {
            component.CrossFade(this._FlyingAnimName, 0.3f);
            component[this._FlyingAnimName].wrapMode = WrapMode.Loop;
        }
    }

    public EBTerror()
    {
    }

    public Transform _SpawnMarker;

    public Transform _SpawnMiddleMarker;

    public Transform _StartMarker;

    public Transform _EndMarker;

    public GameObject _MessageObject;

    public GameObject _Terror;

    public string _FlyingAnimName;

    public string _FireAnimName;

    public UnityEngine.Object _FirePrt;

    public Transform _FireParticleMarker;

    private Collider mOtherCollider;

    public float _TerrorWaitTime = 1f;

    public AudioClip _WarningSFX;

    private float mCurrWaitTime;

    public AudioClip _TerrorFireSFX;

    public float _TerrorHeightOffset;

    private float mTimeOfTravel;

    private float mCurrTimeOfTravel;

    private bool mIsStarted;

    private bool mIsFirstTimeTerrorFire = true;

    private Vector3 mSpawnMiddleControlPoint;

    private Vector3 mMiddleControlPoint;

    private bool mIsTouchSignalSent;

    public EBGameManager.EBCommonVariables _CommonVariables;

    public float _DragonRotationUpdationSpeed = 0.3f;

    private Vector3 mPreviousPosition = Vector3.zero;

    private GameObject mFireParticle;
}
