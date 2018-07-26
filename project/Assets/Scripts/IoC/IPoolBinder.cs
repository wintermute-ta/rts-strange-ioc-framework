using strange.extensions.pool.api;

public interface IPoolBinder<TKey, TValue> where TValue : class
{
    IPool GetPool<T>() where T : TKey;
    TValue GetInstance<T>() where T : TKey;
    void ReturnInstance(TValue instance);
}
