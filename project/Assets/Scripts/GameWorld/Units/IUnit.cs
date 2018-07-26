using GameWorld.HexMap;
using GameWorld.Settings;
using GameWorld.Weapons;
using Signals.Units;

namespace GameWorld
{
    namespace Units
    {
        public interface IUnit
        {
            /// <summary>
            /// ID for View
            /// </summary>
            int ID { get; set; }
            /// <summary>
            /// Id for for fraction relations
            /// </summary>
            int FractionId { get; set; }
            /// <summary>
            /// Type of unit
            /// </summary>
            UnitType Type { get; }
            /// <summary>
            /// Position object
            /// </summary>
            HexCoordinates Coordinates { get; set; }
            /// <summary>
            /// Attack range is measured in count cells
            /// </summary>
            int AttackRange { get; }
            /// <summary>
            /// HP object
            /// </summary>
            float HealthPoint { get; set; }
            bool InDestruction { get; }
            IWeapon Weapon { get; }
            IUnitSettings Settings { get; }

            Destroy OnDestroy { get; }

            void Init(int id, int fraction, HexCoordinates coordinates);
            void HitDamage(float damage);
            void DestroyUnit(); // Mark unit for delayed destruction
            void UpdateAI(float deltaTime);
            bool IsEnemy(IUnit unit);
            bool IsEnemyOf(int fractionId);
        }
    }
}
