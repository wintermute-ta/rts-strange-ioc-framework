using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICoroutineCreator
{
    Coroutine StartCoroutine(IEnumerator routine);
    Coroutine StartCoroutine(string methodName);
    Coroutine StartCoroutine(string methodName, object value);
    void StopAllCoroutines();
    void StopCoroutine(string methodName);
    void StopCoroutine(IEnumerator routine);
    void StopCoroutine(Coroutine routine);
    Coroutine DelayedAction(Action action);
    Coroutine DelayedAction(Action action, YieldInstruction delay);
}
