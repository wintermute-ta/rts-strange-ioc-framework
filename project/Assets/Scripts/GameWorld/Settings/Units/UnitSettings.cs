namespace GameWorld
{
    namespace Settings
    {
        public class UnitSettings : BaseSettings, IUnitSettings
        {
            public float HealthPoint
            {
                get
                {
                    return healthPoint;
                }
                set
                {
                    if (value != healthPoint)
                    {
                        healthPoint = value;
                        OnSettingsChanged();
                    }
                }
            }

            public bool HideSpawnCell
            {
                get
                {
                    return hideSpawnCell;
                }
                set
                {
                    if (value != hideSpawnCell)
                    {
                        hideSpawnCell = value;
                        OnSettingsChanged();
                    }
                }
            }

            private float healthPoint;
            private bool hideSpawnCell;
        }
    }
}
