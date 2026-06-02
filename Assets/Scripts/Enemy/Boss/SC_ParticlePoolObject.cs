using UnityEngine;

public class SC_ParticlePoolObject : MonoBehaviour, SC_IPoolObject
{
    private SC_ObjectPool ownerPool;
    private ParticleSystem particle;

    public void SetPool(SC_ObjectPool pool)
    {
        ownerPool = pool;
    }

    public void OnGetFromPool()
    {
        if (particle == null)
        {
            particle = GetComponentInChildren<ParticleSystem>();
        }

        if (particle != null)
        {
            particle.Clear(true);
            particle.Play(true);
        }
    }

    private void Update()
    {
        if (particle == null) return;

        if (!particle.IsAlive(true))
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        if (particle != null)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (ownerPool != null)
        {
            ownerPool.ReturnObject(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
