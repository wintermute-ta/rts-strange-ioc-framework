using strange.extensions.pool.api;
using strange.framework.api;
using strange.framework.impl;
using System;

public abstract class PoolBinder<TKey, TValue> : Binder, IPoolBinder<TKey, TValue> where TValue : class
{
    public IPool GetPool<T>() where T : TKey
    {
        return GetPool(typeof(T));
    }

    public virtual TValue GetInstance<T>() where T : TKey
    {
        IPool pool = GetPool(typeof(T));
        if (pool != null)
        {
            return pool.GetInstance() as TValue;
        }
        return null;
    }

    public virtual void ReturnInstance(TValue instance)
    {
        IPool pool = GetPool(instance.GetType());
        if (pool != null)
        {
            pool.ReturnInstance(instance);
        }
    }

    protected virtual IPool GetPool(Type key)
    {
        IBinding binding = GetBinding(key);
        if (binding != null)
        {
            return binding.value as IPool;
        }
        return null;
    }
}
