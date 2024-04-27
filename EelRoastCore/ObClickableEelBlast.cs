public class ObClickableEelBlast : ObClickable
{
    public override void OnMouseEnter()
    {
        base.OnMouseEnter();
        this._MouseOver = true;
    }

    public override void OnMouseExit()
    {
        base.OnMouseExit();
        this._MouseOver = false;
    }

    public ObClickableEelBlast()
    {
    }

    public bool _MouseOver;
}
