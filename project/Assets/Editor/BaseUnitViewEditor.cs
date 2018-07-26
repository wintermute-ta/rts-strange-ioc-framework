using GameWorld.Units;
using UnityEditor;
using UnityEngine;
using Views.Units;

namespace EditorExtensions
{
    public class BaseUnitViewEditor : Editor
    {
        private bool toggleUnit = true;
        private bool toggleWeapon = true;
        private bool toggleSettings = true;
        private bool toggleWeaponTargets = false;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            BaseUnitView view = target as BaseUnitView;
            if (view.Unit != null)
            {
                toggleUnit = EditorGUILayout.Foldout(toggleUnit, "Unit");
                if (toggleUnit)
                {
                    MakeLabel("ID", view.Unit.ID.ToString());
                    MakeLabel("Fraction", view.Unit.FractionId.ToString());
                    MakeLabel("Type", view.Unit.Type.ToString());
                    MakeLabel("Hex Coordinates", view.Unit.Coordinates.ToString());
                    view.Unit.HealthPoint = EditorGUILayout.IntField("Health Points", Mathf.RoundToInt(view.Unit.HealthPoint));
                    toggleSettings = EditorGUILayout.Foldout(toggleSettings, "Settings");
                    if (toggleSettings)
                    {
                        MakeLabel("Health Points", view.Unit.Settings.HealthPoint.ToString());
                        MakeLabel("Hide Spawn Cell", view.Unit.Settings.HideSpawnCell.ToString());
                    }
                    if (view.Unit.Weapon != null)
                    {
                        toggleWeapon = EditorGUILayout.Foldout(toggleWeapon, "Weapon");
                        if (toggleWeapon)
                        {
                            MakeLabel("Unit ID", view.Unit.Weapon.UnitID.ToString());
                            MakeLabel("Type", view.Unit.Weapon.GetType().Name);
                            MakeLabel("Attack Range", view.Unit.Weapon.Settings.AttackRange.ToString());
                            MakeLabel("Damage", view.Unit.Weapon.Settings.Damage.ToString());
                            MakeLabel("Rate of Fire", view.Unit.Weapon.Settings.RateOfFire.ToString());
                            MakeLabel("Current Target", view.Unit.Weapon.CurrentTarget != null ? LockedTargetToString(view.Unit.Weapon.CurrentTarget) : string.Empty);

                            toggleWeaponTargets = EditorGUILayout.Foldout(toggleWeaponTargets, string.Format("Locked Targets({0})", view.Unit.Weapon.LockedTargets.Count));
                            if (toggleWeaponTargets)
                            {
                                for (int i = 0; i < view.Unit.Weapon.LockedTargets.Count; i++)
                                {
                                    MakeLabel("Target", LockedTargetToString(view.Unit.Weapon.LockedTargets[i]));
                                }
                            }
                        }
                    }
                }
            }
        }

        private string LockedTargetToString(IUnit unit)
        {
            return string.Format("ID: {0}, Fraction: {1}, Type: {2}, Hex: {3}, HP: {4}", unit.ID, unit.FractionId, unit.Type, unit.Coordinates, unit.HealthPoint);
        }

        protected void MakeLabel(string caption, string text)
        {
            MakeLabel(new GUIContent(caption), text);
        }

        protected void MakeLabel(GUIContent caption, string text)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(caption);
            EditorGUILayout.LabelField(text);
            EditorGUILayout.EndHorizontal();
        }
    }
}
