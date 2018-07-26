using strange.extensions.mediation.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Signals.Units.Weapon;
using GameWorld.Units;

namespace Views
{
    namespace Shots
    {
        public class BaseShotMediator<T> : Mediator where T : BaseShotView
        {
            [Inject]
            public T View { get; private set; }
            [Inject]
            public ShotReachTarget ShotReachTargetSignal { get; private set; }
            [Inject]
            public Signals.Units.NavalUnit.ViewPositionChanged ViewPositionChangedSignal { get; private set; }

            public override void OnRegister()
            {
                View.OnReachTarget.AddListener(OnReachTarget);
                ViewPositionChangedSignal.AddListener(OnTargetViewPositionChanged);
            }

            //OnRemove() is like a destructor/OnDestroy. Use it to clean up.
            public override void OnRemove()
            {
                View.OnReachTarget.RemoveListener(OnReachTarget);
                ViewPositionChangedSignal.RemoveListener(OnTargetViewPositionChanged);
            }

            private void OnTargetViewPositionChanged(IUnit unit, Vector3 position)
            {
                if (unit.ID == View.TargetUnitID)
                {
                    View.AdjustTargetPosition(position);
                }
            }

            private void OnReachTarget()
            {
                ShotReachTargetSignal.Dispatch(View.GetInstanceID());
            }
        }
    }
}
