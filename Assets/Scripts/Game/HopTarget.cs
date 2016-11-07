using UnityEngine;
using System.Collections;

public class HopTarget : MonoBehaviour
{
    protected HopColor hopColor;
    public SpriteRenderer sprite;
    public ParticleSystem explodeParticle;

    public HopColor CurHopColor { set { hopColor = value; } get { return hopColor; } }

    void Awake()
    {
        hopColor = HopColor.RED;
        explodeParticle.transform.SetParent(null);
    }

    public void Explode()
    {
        explodeParticle.transform.localScale = Vector3.one;
        explodeParticle.startColor = sprite.color;
        explodeParticle.Play();
        CacheManager.Store("Basic_HopTarget", this.gameObject);
    }
}
