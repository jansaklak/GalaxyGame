using System.IO;
using Newtonsoft.Json;
using System.Linq;

public class JsonManager
{
    private const string SaveFileName = "GalacticBusinessSimulator.json";
    
    public class GameState
    {
        public List<Player> Players { get; set; }
        public Galaxy Galaxy { get; set; }
        public Economy Economy { get; set; }
        public int CurrentTurn { get; set; }
        public int CurrentPlayerIndex { get; set; }
    }

    public static void SaveGame(List<Player> players, Galaxy galaxy, Economy economy, int currentTurn, int currentPlayerIndex)
    {
        var gameState = new GameState
        {
            Players = players,
            Galaxy = galaxy,
            Economy = economy,
            CurrentTurn = currentTurn,
            CurrentPlayerIndex = currentPlayerIndex
        };

        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };

        string json = JsonConvert.SerializeObject(gameState, settings);
        File.WriteAllText(SaveFileName, json);
    }

    public static GameState LoadGame()
    {
        if (!File.Exists(SaveFileName))
        {
            return null;
        }

        string json = File.ReadAllText(SaveFileName);
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        return JsonConvert.DeserializeObject<GameState>(json, settings);
    }

    // Helper method to fix references after loading from JSON
    public static void FixReferences(GameState gameState)
    {
        // Fix references for all players
        foreach (var player in gameState.Players)
        {
            player.OwnedPlanets.Clear();  // Ensure no broken references exist
        }

        // For each solar system and planet
        foreach (var system in gameState.Galaxy.SolarSystems)
        {
            foreach (var planet in system.Planets)
            {
                // Set reference to the parent system
                planet.ParentSystem = system;

                // Fix reference to owner if planet is owned by any player
                if (planet?.Owner != null)
                {
                    string ownerName = planet.Owner.Name;
                    planet.Owner = gameState.Players.FirstOrDefault(p => p.Name == ownerName);

                    if (planet.Owner != null)
                    {
                        planet.Owner.OwnedPlanets.Add(planet);  // Add planet to the owner's list
                    }
                }
            }
        }
    }
}
