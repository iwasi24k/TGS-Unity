public interface SC_IPoolObject
{
    void SetPool(SC_ObjectPool pool);
    void OnGetFromPool();
    void ReturnToPool();
}
