using Core;
using GameWorld.Units;
using strange.extensions.mediation.impl;

namespace Views
{
    namespace Units
    {
        public class BaseUnitMediator<T> : Mediator where T : BaseUnitView
        {
            [Inject]
            public T View { get; private set; }
            [Inject]
            public IGameManager GameManager { get; private set; }
            [Inject]
            public IUIManager UIManager { get; private set; }
            [Inject]
            public Signals.Camera.Rotate RotateSignal { get; private set; }
            [Inject]
            public Signals.Camera.Zoom ZoomSignal { get; private set; }
            [Inject]
            public Signals.Units.HitDamage HitDamageSignal { get; private set; }
            [Inject]
            public Signals.Units.ChangeTarget ChangeTargetSignal { get; private set; }

            public override void OnRegister()
            {
                HitDamageSignal.AddListener(OnUnitHitDamage);
                ChangeTargetSignal.AddListener(OnUnitChangeTarget);
                View.AttachHealhBar(UIManager.CreateUI<SliderModel>(View.HealthBarCanvas.transform, SliderHandler.SliderType.BASE_SLIDER.ToString()));
                ZoomSignal.AddListener(View.HealthBarCanvas.UpdateCanvas);
                RotateSignal.AddListener(View.HealthBarCanvas.UpdateCanvas);
            }

            public override void OnRemove()
            {
                View.RemoveHealthBar();
                HitDamageSignal.RemoveListener(OnUnitHitDamage);
                ChangeTargetSignal.RemoveListener(OnUnitChangeTarget);
                ZoomSignal.RemoveListener(View.HealthBarCanvas.UpdateCanvas);
                RotateSignal.RemoveListener(View.HealthBarCanvas.UpdateCanvas);
            }

            protected bool IsViewUnit(IUnit unit)
            {
                return unit.ID == View.Unit.ID;
            }

            protected virtual void OnUnitHitDamage(IUnit unit, float damage)
            {
                if (IsViewUnit(unit))
                {
                    View.UpdateHP(unit.HealthPoint);
                }
            }
            protected virtual void OnUnitChangeTarget(IUnit unit, IUnit target) { }
        }
    }
}
