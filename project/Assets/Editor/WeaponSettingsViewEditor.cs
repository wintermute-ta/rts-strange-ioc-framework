using GameWorld.Settings;
using UnityEditor;
using UnityEngine;
using Views.Settings;

namespace EditorExtensions
{
    [CustomEditor(typeof(WeaponSettingsView))]
    public class WeaponSettingsViewEditor : Editor
    {
        private bool toggleSmallCannon = true;
        private bool toggleMediumCannon = true;
        private bool toggleLargeCannon = true;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            WeaponSettingsView view = target as WeaponSettingsView;
            toggleSmallCannon = AddWeaponSettings(toggleSmallCannon, "Small Cannon", view.SmallCannonSettings);
            toggleMediumCannon = AddWeaponSettings(toggleMediumCannon, "Medium Cannon", view.MediumCannonSettings);
            toggleLargeCannon = AddWeaponSettings(toggleLargeCannon, "Large Cannon", view.LargeCannonSettings);
        }

        private bool AddWeaponSettings(bool toggle, string caption, IWeaponSettings settings)
        {
            toggle = EditorGUILayout.Foldout(toggle, string.Format("{0} Settings", caption));
            if (toggle)
            {
                settings.AttackRange = EditorGUILayout.IntField("Attack Range", Mathf.RoundToInt(settings.AttackRange));
                settings.Damage = EditorGUILayout.FloatField("Damage", settings.Damage);
                settings.RateOfFire = EditorGUILayout.IntField("Rate Of Fire", Mathf.RoundToInt(settings.RateOfFire));
            }
            return toggle;
        }
    }
}
