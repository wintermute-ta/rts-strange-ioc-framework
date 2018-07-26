namespace GameWorld
{
    namespace Settings
    {
        public class GroundCannonSettings : GroundUnitSettings
        {
            protected override void OnDeserialize()
            {
                // Defaut settings
                HealthPoint = 50.0f;

                // Make deserialization.
                base.OnDeserialize();
            }
        }
    }
}
