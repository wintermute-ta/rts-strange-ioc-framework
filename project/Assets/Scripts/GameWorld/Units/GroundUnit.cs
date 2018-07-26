namespace GameWorld
{
    namespace Units
    {
        public class GroundUnit : Unit, IGroundUnit
        {
            static UnitType[] _targetTypes = new UnitType[] { UnitType.Ship };

            public GroundUnit(UnitType type) : base(type, _targetTypes)
            {
            }
        }
    }
}
