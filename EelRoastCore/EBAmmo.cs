using UnityEngine;

public class EBAmmo : MonoBehaviour
{
    public GameObject _FollowObj;
    public float _TimeOfTravel = 0.2f;
    bool mDone;
    float mCurrTime;
    public void Update()
    {
        if (!mDone)
        {
            mCurrTime += Time.deltaTime;
            if (mCurrTime > _TimeOfTravel)
                mCurrTime = _TimeOfTravel;
            if (_FollowObj)
                transform.position = Vector3.Lerp(transform.position, _FollowObj.transform.position, mCurrTime / _TimeOfTravel);
            else
                mCurrTime = _TimeOfTravel;
            if (mCurrTime == _TimeOfTravel)
            {
                if (_FollowObj)
                    _FollowObj.SendMessage("MarkAsDestroyed");
                mDone = true;
                Destroy(gameObject);
                return;
            }
        }
        else
            Destroy(gameObject);
    }
}
