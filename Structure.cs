using Newtonsoft.Json;
namespace GalaxyGame
{
    public enum StructureType
    {
        Spaceport,
        Outpost,
        Habitat,
        Colony,
        GalacticHotel,
        PlanetaryHotelNetwork,
        Mine,
        FoodFarm,
        GalacticShipyard,
        AsteroidMine
    }

    public class Structure
    {
        public StructureType Type { get; private set; }
        public int Level { get; private set; }
        public int Value { get; private set; }
        public int IncomePerTurn { get; private set; }

        [JsonIgnore]
        public Planet OwnerPlanet { get; set; }

        public Structure(StructureType type)
        {
            Type = type;
            Level = 1;
            UpdateValues();
        }

        public bool CanUpgrade()
        {
            return GetMaxLevel() > Level;
        }

        public void Upgrade()
        {
            if (!CanUpgrade())
            {
                throw new System.InvalidOperationException($"Cannot upgrade {Type} beyond level {Level}.");
            }

            Level++;
            UpdateValues();
        }

        private int GetMaxLevel()
        {
            switch (Type)
            {
                case StructureType.Outpost:
                    return 4; // Can be upgraded to Habitat, Colony, GalacticHotel, PlanetaryHotelNetwork
                case StructureType.Mine:
                    return 3; // Can be upgraded to level 3
                case StructureType.FoodFarm:
                    return 6; // Can be upgraded to level 6
                case StructureType.AsteroidMine:
                    return 5; // Can be upgraded to level 5
                default:
                    return 1; // Not upgradeable
            }
        }

        private void UpdateValues()
        {
            // These values are examples and can be adjusted
            switch (Type)
            {
                case StructureType.Spaceport:
                    Value = 1000;
                    IncomePerTurn = 50;
                    break;
                case StructureType.Outpost:
                    Value = 800 * Level;
                    IncomePerTurn = 30 * Level;
                    break;
                case StructureType.Habitat:
                    Value = 1500;
                    IncomePerTurn = 100;
                    break;
                case StructureType.Colony:
                    Value = 3000;
                    IncomePerTurn = 200;
                    break;
                case StructureType.GalacticHotel:
                    Value = 5000;
                    IncomePerTurn = 400;
                    break;
                case StructureType.PlanetaryHotelNetwork:
                    Value = 10000;
                    IncomePerTurn = 1000;
                    break;
                case StructureType.Mine:
                    Value = 1200 * Level;
                    IncomePerTurn = 80 * Level;
                    break;
                case StructureType.FoodFarm:
                    Value = 800 * Level;
                    IncomePerTurn = 60 * Level;
                    break;
                case StructureType.GalacticShipyard:
                    Value = 8000;
                    IncomePerTurn = 800;
                    break;
                case StructureType.AsteroidMine:
                    Value = 1500 * Level;
                    IncomePerTurn = 120 * Level;
                    break;
            }
        }
    }
}