namespace GameWorld
{
    namespace Settings
    {
        public interface IWeaponSettings
        {
            int AttackRange { get; set; }
            int RateOfFire { get; set; }
            float Damage { get; set; }
        }
    }
}
