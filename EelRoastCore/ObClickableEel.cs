using UnityEngine;

public class ObClickableEel : ObClickable
{
    public override void ProcessMouseUp()
    {
        if (this._MessageObject)
        {
            this._MessageObject.SendMessage("OnClick", base.gameObject, SendMessageOptions.DontRequireReceiver);
        }
        if (this._AvatarWalkTo && AvAvatar.pObject != null)
        {
            AvAvatar.pObject.SendMessage("OnClick", base.gameObject, SendMessageOptions.DontRequireReceiver);
        }
        if (this._ClickSound != null && this._ClickSound._AudioClip != null)
        {
            this._ClickSound.Play();
        }
        if (!this.WithinRange())
        {
            return;
        }
        if (this._StopVOPoolOnClick)
        {
            SnChannel.StopPool("VO_Pool");
        }
        base.SendMessage("OnActivate", null, SendMessageOptions.DontRequireReceiver);
    }

    public ObClickableEel()
    {
    }
}
