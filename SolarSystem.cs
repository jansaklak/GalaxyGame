using System.Collections.Generic;
using Newtonsoft.Json;
namespace GalaxyGame
{
    public class SolarSystem
    {
        public string Name { get; set; }
        public List<Planet> Planets { get; set; } = new List<Planet>();
        public int Position { get; set; }

        public SolarSystem(string name, int position)
        {
            Name = name;
            Position = position;
        }

        public void AddPlanet(string name)
        {
            Planets.Add(new Planet(name, this));
        }

        public bool HasGalacticShipyard()
        {
            foreach (var planet in Planets)
            {
                if (planet.Structures.Exists(s => s.Type == StructureType.GalacticShipyard))
                {
                    return true;
                }
            }
            return false;
        }
    }
}