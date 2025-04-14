using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class Galaxy
{
    public List<SolarSystem> SolarSystems { get; set; } = new List<SolarSystem>();
    public List<int> AnomalyPositions { get; set; } = new List<int>();
    public List<int> PiratePositions { get; set; } = new List<int>();
    public List<int> RailwayStations { get; set; } = new List<int>();
    public int TotalPositions { get; set; }

    public Galaxy(int totalPositions)
    {
        TotalPositions = totalPositions;
    }

    public void InitializeGalaxy()
    {
        Random random = new Random();
        
        // Create solar systems
        int numSystems = TotalPositions / 10; // Roughly 10% of positions are solar systems
        for (int i = 0; i < numSystems; i++)
        {
            int position = random.Next(TotalPositions);
            while (IsSolarSystemAt(position))
            {
                position = random.Next(TotalPositions);
            }
            
            SolarSystem system = new SolarSystem($"System-{i+1}", position);
            
            // Add 1-4 planets to each system
            int numPlanets = random.Next(1, 5);
            for (int j = 0; j < numPlanets; j++)
            {
                system.AddPlanet($"Planet-{i+1}-{j+1}");
            }
            
            SolarSystems.Add(system);
        }
        
        // Create anomalies (5% of positions)
        int numAnomalies = TotalPositions / 20;
        for (int i = 0; i < numAnomalies; i++)
        {
            int position = random.Next(TotalPositions);
            while (IsSolarSystemAt(position) || IsAnomalyAt(position))
            {
                position = random.Next(TotalPositions);
            }
            AnomalyPositions.Add(position);
        }
        
        // Create pirate territories (5% of positions)
        int numPirates = TotalPositions / 20;
        for (int i = 0; i < numPirates; i++)
        {
            int position = random.Next(TotalPositions);
            while (IsSolarSystemAt(position) || IsAnomalyAt(position) || IsPirateAt(position))
            {
                position = random.Next(TotalPositions);
            }
            PiratePositions.Add(position);
        }
        
        // Create railway stations (8% of positions)
        int numStations = TotalPositions / 12;
        for (int i = 0; i < numStations; i++)
        {
            int position = random.Next(TotalPositions);
            while (IsSolarSystemAt(position) || IsAnomalyAt(position) || IsPirateAt(position) || IsRailwayStationAt(position))
            {
                position = random.Next(TotalPositions);
            }
            RailwayStations.Add(position);
        }
    }

    public bool IsSolarSystemAt(int position)
    {
        return SolarSystems.Any(s => s.Position == position);
    }

    public bool IsAnomalyAt(int position)
    {
        return AnomalyPositions.Contains(position);
    }

    public bool IsPirateAt(int position)
    {
        return PiratePositions.Contains(position);
    }

    public bool IsRailwayStationAt(int position)
    {
        return RailwayStations.Contains(position);
    }

    public SolarSystem GetSolarSystemAt(int position)
    {
        return SolarSystems.FirstOrDefault(s => s.Position == position);
    }

    public List<int> GetRailwayStations()
    {
        return new List<int>(RailwayStations);
    }
}