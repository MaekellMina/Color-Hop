using UnityEngine;
using System.Collections;

public class HopTarget : MonoBehaviour
{
    protected HopColor hopColor;
    public SpriteRenderer sprite;

    public HopColor CurHopColor { set { hopColor = value; } get { return hopColor; } }

    void Awake()
    {
        hopColor = HopColor.RED;
    }

    public void Explode()
    {
        CacheManager.Store("Basic_HopTarget", this.gameObject);
    }
}
