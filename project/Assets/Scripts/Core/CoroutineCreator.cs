using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class CoroutineCreator : MonoBehaviour, ICoroutineCreator
    {
        private static ICoroutineCreator _instance;

        [Obsolete("Static CoroutineCreator has been deprecated. Use injections instead CoroutineCreator.Instance.")]
        public static ICoroutineCreator Instance
        {
            get { return _instance ?? (_instance = GlobalContext.Get().GetInstance<ICoroutineCreator>()); }
        }

        public Coroutine DelayedAction(Action action)
        {
            return StartCoroutine(CoDelayedAction(action, null));
        }

        public Coroutine DelayedAction(Action action, YieldInstruction delay)
        {
            return StartCoroutine(CoDelayedAction(action, delay));
        }

        private IEnumerator CoDelayedAction(Action action, YieldInstruction delay)
        {
            yield return delay;
            if (action != null)
            {
                action.Invoke();
            }
        }
    }
}
