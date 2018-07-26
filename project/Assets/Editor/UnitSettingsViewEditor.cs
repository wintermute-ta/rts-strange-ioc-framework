using GameWorld.Settings;
using UnityEditor;
using UnityEngine;
using Views.Settings;

namespace EditorExtensions
{
    [CustomEditor(typeof(UnitSettingsView))]
    public class UnitSettingsViewEditor : Editor
    {
        private bool toggleShip = true;
        private bool toggleGroundCannon = true;
        private bool toggleFort = true;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            UnitSettingsView view = target as UnitSettingsView;
            toggleShip = AddNavalUnitSettings(toggleShip, "Ship", view.ShipSettings);
            toggleGroundCannon = AddUnitSettings(toggleGroundCannon, "Ground Cannon", view.GroundCannonSettings);
            toggleFort = AddUnitSettings(toggleFort, "Fort", view.FortSettings);
        }

        private bool AddUnitSettings(bool toggle, string caption, IGroundUnitSettings settings)
        {
            toggle = EditorGUILayout.Foldout(toggle, string.Format("{0} Settings", caption));
            if (toggle)
            {
                settings.HealthPoint = EditorGUILayout.IntField("Health Points", Mathf.RoundToInt(settings.HealthPoint));
                settings.HideSpawnCell = EditorGUILayout.Toggle("Hide Spawn Cell", settings.HideSpawnCell);
            }
            return toggle;
        }

        private bool AddNavalUnitSettings(bool toggle, string caption, INavalUnitSettings settings)
        {
            toggle = EditorGUILayout.Foldout(toggle, string.Format("{0} Settings", caption));
            if (toggle)
            {
                settings.Speed = EditorGUILayout.IntField("Speed", Mathf.RoundToInt(settings.Speed));
                settings.HealthPoint = EditorGUILayout.IntField("Health Points", Mathf.RoundToInt(settings.HealthPoint));
            }
            return toggle;
        }
    }
}
