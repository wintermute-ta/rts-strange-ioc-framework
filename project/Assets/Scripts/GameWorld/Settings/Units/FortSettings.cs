namespace GameWorld
{
    namespace Settings
    {
        public class FortSettings : GroundUnitSettings
        {
            protected override void OnDeserialize()
            {
                // Defaut settings
                HealthPoint = 1000.0f;

                // Make deserialization.
                base.OnDeserialize();
            }
        }
    }
}