namespace GameWorld
{
    namespace Settings
    {
        public class MediumCannonSettings : WeaponSettings
        {
            protected override void OnDeserialize()
            {
                // Defaut settings
                AttackRange = 3;
                RateOfFire = 1;
                Damage = 5.0f;

                // Make deserialization.
                base.OnDeserialize();
            }
        }
    }
}
