using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

namespace GalaxyGame
{
    public class Economy
    {
        public int BaseIncome { get; set; }
        public int TurnsBetweenPayments { get; set; }
        public double PropertyTaxRate { get; set; }
        public int PirateRansom { get; set; }
        public int TowingCost { get; set; }
        public int ShipyardRepairCost { get; set; }
        public Dictionary<StructureType, int> BuildingCosts { get; set; }
        public Dictionary<StructureType, int> UpgradeCosts { get; set; }

        public Economy()
        {
            BaseIncome = 200;
            TurnsBetweenPayments = 5;
            PropertyTaxRate = 0.1; // 10%
            PirateRansom = 500;
            TowingCost = 300;
            ShipyardRepairCost = 2000;

            BuildingCosts = new Dictionary<StructureType, int>
        {
            { StructureType.Spaceport, 1000 },
            { StructureType.Outpost, 800 },
            { StructureType.Mine, 1200 },
            { StructureType.FoodFarm, 800 },
            { StructureType.GalacticShipyard, 8000 },
            { StructureType.AsteroidMine, 1500 }
        };

            UpgradeCosts = new Dictionary<StructureType, int>
        {
            { StructureType.Outpost, 1000 }, // Cost to upgrade from Outpost to Habitat
            { StructureType.Habitat, 2000 }, // Cost to upgrade from Habitat to Colony
            { StructureType.Colony, 3000 },  // Cost to upgrade from Colony to GalacticHotel
            { StructureType.GalacticHotel, 5000 }, // Cost to upgrade from GalacticHotel to PlanetaryHotelNetwork
            { StructureType.Mine, 1500 },    // Cost to upgrade Mine (per level)
            { StructureType.FoodFarm, 1000 }, // Cost to upgrade FoodFarm (per level)
            { StructureType.AsteroidMine, 2000 } // Cost to upgrade AsteroidMine (per level)
        };
        }

        public int CalculatePlayerIncome(Player player)
        {
            int income = BaseIncome;

            foreach (var planet in player.OwnedPlanets)
            {
                foreach (var structure in planet.Structures)
                {
                    income += structure.IncomePerTurn;
                }
            }

            return income;
        }

        public int CalculatePropertyTax(Player player)
        {
            int totalPropertyValue = 0;

            foreach (var planet in player.OwnedPlanets)
            {
                if (planet.HasSpaceport)
                {
                    totalPropertyValue += BuildingCosts[StructureType.Spaceport];
                }

                foreach (var structure in planet.Structures)
                {
                    totalPropertyValue += structure.Value;
                }
            }

            return (int)(totalPropertyValue * PropertyTaxRate);
        }

        public bool CanAfford(Player player, int cost)
        {
            return player.Credits >= cost;
        }

        public void ChargeFee(Player player, int amount)
        {
            if (!CanAfford(player, amount))
            {
                throw new InvalidOperationException("Player cannot afford this fee.");
            }

            player.Credits -= amount;
        }

        public void AddCredits(Player player, int amount)
        {
            player.Credits += amount;
        }
    }
}