using UnityEngine;

public class SC_LaunchVisualMissile : MonoBehaviour, SC_IPoolObject
{
    [SerializeField] private float speed = 20.0f;
    [SerializeField] private float lifeTime = 1.0f;

    private float timer;
    private SC_ObjectPool ownerPool;

    private ParticleSystem[] fireParticles;

    public void SetPool(SC_ObjectPool pool)
    {
        ownerPool = pool;
    }

    public void OnGetFromPool()
    {
        timer = 0f;

        if (fireParticles != null)
        {
            foreach (ParticleSystem ps in fireParticles)
            {
                ps.Clear();
                ps.Play();
            }
        }
    }

    private void Awake()
    {
        fireParticles = GetComponentsInChildren<ParticleSystem>(true);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        transform.position += transform.up * speed * Time.deltaTime;

        if (timer >= lifeTime)
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        if (fireParticles != null)
        {
            foreach (ParticleSystem ps in fireParticles)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
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
