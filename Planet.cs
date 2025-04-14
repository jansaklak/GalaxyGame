using System.Collections.Generic;
using Newtonsoft.Json;

public class Planet
{
    public string Name { get; set; }
    public Player Owner { get; set; }
    public SolarSystem ParentSystem { get; set; }
    public List<Structure> Structures { get; set; } = new List<Structure>();
    public bool HasSpaceport { get; set; }

    public Planet(string name, SolarSystem parentSystem)
    {
        Name = name;
        ParentSystem = parentSystem;
        HasSpaceport = false;
    }

    public bool CanBuildStructure(StructureType type, Player player)
    {
        // Player must own the planet
        if (Owner != player)
        {
            return false;
        }

        // Must have a spaceport before building anything else
        if (!HasSpaceport && type != StructureType.Spaceport)
        {
            return false;
        }

        // Check for duplicate structures if not upgradeable
        if (!IsUpgradeableStructure(type) && Structures.Exists(s => s.Type == type))
        {
            return false;
        }

        // Special case for Galactic Shipyard
        if (type == StructureType.GalacticShipyard)
        {
            return player.OwnsAllPlanetsInSystem(ParentSystem) && 
                   !ParentSystem.HasGalacticShipyard();
        }

        // Special case for Planetary Hotel Network
        if (type == StructureType.PlanetaryHotelNetwork)
        {
            return player.OwnsAllPlanetsInSystem(ParentSystem) &&
                   Structures.Exists(s => s.Type == StructureType.GalacticHotel);
        }

        return true;
    }

    public bool CanUpgradeStructure(Structure structure, Player player)
    {
        if (Owner != player)
        {
            return false;
        }

        // Check if the structure is at its maximum level
        return structure.CanUpgrade();
    }

    private bool IsUpgradeableStructure(StructureType type)
    {
        return type == StructureType.Outpost ||
               type == StructureType.Mine ||
               type == StructureType.FoodFarm ||
               type == StructureType.AsteroidMine;
    }

    public void BuildSpaceport(Player player)
    {
        Owner = player;
        HasSpaceport = true;
        player.OwnedPlanets.Add(this);
    }

    public Structure BuildStructure(StructureType type, Player player)
    {
        if (!CanBuildStructure(type, player))
        {
            throw new System.InvalidOperationException($"Cannot build {type} on this planet.");
        }

        var structure = new Structure(type);
        Structures.Add(structure);
        return structure;
    }

    public void UpgradeStructure(Structure structure, Player player)
    {
        if (!CanUpgradeStructure(structure, player))
        {
            throw new System.InvalidOperationException("Cannot upgrade this structure.");
        }

        structure.Upgrade();
    }
}