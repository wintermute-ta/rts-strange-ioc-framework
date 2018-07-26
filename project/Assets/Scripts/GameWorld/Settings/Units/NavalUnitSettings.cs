namespace GameWorld
{
    namespace Settings
    {
        public class NavalUnitSettings : UnitSettings, INavalUnitSettings
        {
            public float Speed
            {
                get
                {
                    return speed;
                }
                set
                {
                    if (value != speed)
                    {
                        speed = value;
                        OnSettingsChanged();
                    }
                }
            }

            private float speed;

            protected override void OnDeserialize()
            {
                // Defaut settings
                HideSpawnCell = false;

                // Make deserialization.
                base.OnDeserialize();
            }
        }
    }
}
