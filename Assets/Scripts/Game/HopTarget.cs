using UnityEngine;
using System.Collections;

public class HopTarget : MonoBehaviour
{
    protected HopColor hopColor;
    private SpriteRenderer sprite;

    public HopColor HopColor { set { hopColor = value; } get { return hopColor; } }

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    public virtual void SetColorAppearance()
    {
        switch(hopColor)
        {
            case HopColor.RED:
                sprite.color = GameManager.instance.redAppearance;
                break;
            case HopColor.BLUE:
                sprite.color = GameManager.instance.blueAppearance;
                break;
            case HopColor.YELLOW:
                sprite.color = GameManager.instance.yellowAppearance;
                break;
        }
    }
}
