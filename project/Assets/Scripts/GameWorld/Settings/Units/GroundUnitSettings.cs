namespace GameWorld
{
    namespace Settings
    {
        public class GroundUnitSettings : UnitSettings, IGroundUnitSettings
        {
            protected override void OnDeserialize()
            {
                // Defaut settings
                HideSpawnCell = true;

                // Make deserialization.
                base.OnDeserialize();
            }
        }
    }
}
