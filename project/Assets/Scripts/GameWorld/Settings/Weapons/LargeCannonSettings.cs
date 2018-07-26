namespace GameWorld
{
    namespace Settings
    {
        public class LargeCannonSettings : WeaponSettings
        {
            protected override void OnDeserialize()
            {
                // Defaut settings
                AttackRange = 3;
                RateOfFire = 1;
                Damage = 10.0f;

                // Make deserialization.
                base.OnDeserialize();
            }
        }
    }
}
