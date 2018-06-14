using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnitView<T> where T : IUnit
{
    T Unit { get; }

    void Init(T unit, Vector3 position, Quaternion rotation);
}
