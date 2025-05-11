using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Numerics;
using System.Xml;
namespace GalaxyGame
{
    public class JsonManager
    {
        private const string SaveFileName = "GalacticBusinessSimulator.json";

        public class GameState
        {
            public List<Player> Players { get; set; } = new List<Player>();
            public Galaxy Galaxy { get; set; }
            public Economy Economy { get; set; }
            public int CurrentTurn { get; set; }
            public int CurrentPlayerIndex { get; set; }
        }

        public static void SaveGame(List<Player> players, Galaxy galaxy, Economy economy, int currentTurn, int currentPlayerIndex)
        {
            try
            {
                var gameState = new GameState
                {
                    Players = players ?? new List<Player>(),
                    Galaxy = galaxy,
                    Economy = economy,
                    CurrentTurn = currentTurn,
                    CurrentPlayerIndex = currentPlayerIndex
                };

                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                };

                string json = JsonConvert.SerializeObject(gameState, settings);
                File.WriteAllText(SaveFileName, json);
                Console.WriteLine("Gra zosta�a zapisana.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B��d podczas zapisywania gry: {ex.Message}");
            }
        }

        public static GameState LoadGame()
        {
            if (!File.Exists(SaveFileName))
            {
                Console.WriteLine("Nie znaleziono pliku zapisu gry.");
                return null;
            }

            try
            {
                string json = File.ReadAllText(SaveFileName);
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Error = HandleDeserializationError
                };

                var gameState = JsonConvert.DeserializeObject<GameState>(json, settings);

                // Zabezpieczenie przed null
                if (gameState == null)
                {
                    Console.WriteLine("Plik zapisu gry jest uszkodzony lub pusty.");
                    return null;
                }

                // Inicjalizuj kolekcje je�li s� null
                gameState.Players ??= new List<Player>();

                return gameState;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B��d podczas �adowania gry: {ex.Message}");
                return null;
            }
        }

        // Obs�uga b��d�w deserializacji
        private static void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            Console.WriteLine($"B��d deserializacji: {e.ErrorContext.Error.Message}");
            e.ErrorContext.Handled = true; // Oznaczamy b��d jako obs�u�ony
        }

        // Helper method to fix references after loading from JSON
        public static void FixReferences(GameState gameState)
        {
            try
            {
                if (gameState == null)
                {
                    Console.WriteLine("Nie mo�na naprawi� referencji - gameState jest null.");
                    return;
                }

                if (gameState.Players == null)
                {
                    Console.WriteLine("Lista graczy jest null. Inicjalizuj� now� list�.");
                    gameState.Players = new List<Player>();
                    return;
                }

                if (gameState.Galaxy == null)
                {
                    Console.WriteLine("Galaxy jest null. Nie mo�na naprawi� referencji.");
                    return;
                }

                if (gameState.Galaxy.SolarSystems == null)
                {
                    Console.WriteLine("SolarSystems jest null. Inicjalizuj� now� list� system�w s�onecznych.");
                    gameState.Galaxy.SolarSystems = new List<SolarSystem>();
                    return;
                }

                // Fix references for all players
                foreach (var player in gameState.Players)
                {
                    if (player == null) continue;

                    if (player.OwnedPlanets == null)
                    {
                        player.OwnedPlanets = new List<Planet>();
                    }
                    else
                    {
                        player.OwnedPlanets.Clear();  // Ensure no broken references exist
                    }

                    if (player.Cards == null)
                    {
                        player.Cards = new List<ChangeCard>();
                    }
                }

                // For each solar system and planet
                foreach (var system in gameState.Galaxy.SolarSystems)
                {
                    if (system == null) continue;

                    if (system.Planets == null)
                    {
                        system.Planets = new List<Planet>();
                        continue;
                    }

                    foreach (var planet in system.Planets)
                    {
                        if (planet == null) continue;

                        // Set reference to the parent system
                        planet.ParentSystem = system;

                        // Initialization of any null collections
                        if (planet.Structures == null)
                        {
                            planet.Structures = new List<Structure>();
                        }

                        // Fix reference to owner if planet is owned by any player
                        if (planet.Owner != null)
                        {
                            string ownerName = planet.Owner.Name;
                            var owner = gameState.Players.FirstOrDefault(p => p != null && p.Name == ownerName);

                            if (owner != null)
                            {
                                planet.Owner = owner;
                                owner.OwnedPlanets.Add(planet);  // Add planet to the owner's list
                            }
                            else
                            {
                                // Owner reference is invalid, clear it
                                planet.Owner = null;
                            }
                        }

                        // Fix references for structures
                        // Fix references for structures
                        foreach (var structure in planet.Structures)
                        {
                            if (structure == null) continue;
                            structure.OwnerPlanet = planet;
                        }


                    }
                }

                // Initialize other collections in Galaxy if they're null
                if (gameState.Galaxy.AnomalyPositions == null)
                {
                    gameState.Galaxy.AnomalyPositions = new List<int>();
                }

                if (gameState.Galaxy.PiratePositions == null)
                {
                    gameState.Galaxy.PiratePositions = new List<int>();
                }

                if (gameState.Galaxy.RailwayStations == null)
                {
                    gameState.Galaxy.RailwayStations = new List<int>();
                }

                Console.WriteLine("Referencje zosta�y pomy�lnie naprawione.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B��d podczas naprawiania referencji: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
    }
}