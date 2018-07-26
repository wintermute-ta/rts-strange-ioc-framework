namespace GameWorld
{
    namespace Settings
    {
        public class ShipSettings : NavalUnitSettings
        {
            protected override void OnDeserialize()
            {
                // Defaut settings
                HealthPoint = 200.0f;
                Speed = 1.0f;

                // Make deserialization.
                base.OnDeserialize();
            }
        }
    }
}
