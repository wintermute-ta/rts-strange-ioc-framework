namespace GameWorld
{
    namespace Settings
    {
        public class WeaponSettings : BaseSettings, IWeaponSettings
        {
            public int AttackRange
            {
                get
                {
                    return attackRange;
                }
                set
                {
                    if (value != attackRange)
                    {
                        attackRange = value;
                        OnSettingsChanged();
                    }
                }
            }

            public float Damage
            {
                get
                {
                    return damage;
                }
                set
                {
                    if (value != damage)
                    {
                        damage = value;
                        OnSettingsChanged();
                    }
                }
            }

            public int RateOfFire
            {
                get
                {
                    return rateOfFire;
                }
                set
                {
                    if (value != rateOfFire)
                    {
                        rateOfFire = value;
                        OnSettingsChanged();
                    }
                }
            }

            private int rateOfFire;
            private float damage;
            private int attackRange;
        }
    }
}
