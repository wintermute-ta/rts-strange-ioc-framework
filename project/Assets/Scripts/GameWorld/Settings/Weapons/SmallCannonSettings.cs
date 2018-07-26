namespace GameWorld
{
    namespace Settings
    {
        public class SmallCannonSettings : WeaponSettings
        {
            protected override void OnDeserialize()
            {
                // Defaut settings
                AttackRange = 3;
                RateOfFire = 1;
                Damage = 1.0f;

                // Make deserialization.
                base.OnDeserialize();
            }
        }
    }
}
