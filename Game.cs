using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
namespace GalaxyGame
{
    public class Game
    {
        private List<Player> players;
        private Galaxy galaxy;
        private Economy economy;
        private Events events;
        private int currentTurn;
        private int currentPlayerIndex;
        private readonly Random random = new Random();

        public IReadOnlyList<Player> Players => players.AsReadOnly();
        public Player CurrentPlayer => players[currentPlayerIndex];
        public int CurrentTurn => currentTurn;
        public Galaxy Galaxy => galaxy;
        public Economy Economy => economy;

        public event Action<string> GameMessageLogged;
        public event Action GameStateChanged;

        public Game(int playerCount = 1, List<string> playerNames = null)
        {
            try
            {
                GameMessageLogged?.Invoke("Initializing game...");

                players = new List<Player>();
                galaxy = new Galaxy(40);
                economy = new Economy();
                currentTurn = 1;
                currentPlayerIndex = 0;

                var gameState = JsonManager.LoadGame();
                if (gameState?.Players?.Count > 0)
                {
                    GameMessageLogged?.Invoke("Loading saved game...");
                    players = gameState.Players;
                    galaxy = gameState.Galaxy ?? new Galaxy(40);
                    economy = gameState.Economy ?? new Economy();
                    currentTurn = gameState.CurrentTurn;
                    currentPlayerIndex = gameState.CurrentPlayerIndex;
                    JsonManager.FixReferences(gameState);
                    GameMessageLogged?.Invoke("Saved game loaded successfully.");
                }
                else
                {
                    GameMessageLogged?.Invoke("No valid saved game found. Setting up new game...");
                    SetupNewGame(playerCount, playerNames);
                }

                events = new Events(economy, galaxy);
                GameMessageLogged?.Invoke("Game initialization completed.");
            }
            catch (Exception ex)
            {
                GameMessageLogged?.Invoke($"Error during initialization: {ex.Message}");
                players = new List<Player>();
                galaxy = new Galaxy(40);
                economy = new Economy();
                currentTurn = 1;
                currentPlayerIndex = 0;
                SetupNewGame(playerCount, playerNames);
                events = new Events(economy, galaxy);
            }
        }

        private void SetupNewGame(int playerCount, List<string> playerNames)
        {
            playerCount = Math.Clamp(playerCount, 1, 4);
            players.Clear();
            for (int i = 0; i < playerCount; i++)
            {
                string name = playerNames != null && i < playerNames.Count && !string.IsNullOrWhiteSpace(playerNames[i])
                    ? playerNames[i]
                    : $"Player {i + 1}";
                players.Add(new Player(name, 5000));
            }

            galaxy.InitializeGalaxy();
            economy = new Economy();
            events = new Events(economy, galaxy);
            currentTurn = 1;
            currentPlayerIndex = 0;
            GameStateChanged?.Invoke();
        }

        public void StartGame()
        {
            GameMessageLogged?.Invoke("Welcome to Galactic Business Simulator!");
            GameMessageLogged?.Invoke("You are a galactic businessman traveling through the galaxy.");
            GameMessageLogged?.Invoke("Build spaceports, mines, and more to grow your business empire!");
            GameStateChanged?.Invoke();
        }

        public void MovePlayer()
        {
            Player currentPlayer = CurrentPlayer;
            if (currentPlayer.LostTurns > 0)
            {
                GameMessageLogged?.Invoke($"{currentPlayer.Name} has {currentPlayer.LostTurns} lost turns remaining.");
                currentPlayer.LostTurns--;
                NextPlayer();
                return;
            }

            int diceRoll = random.Next(1, 7);
            GameMessageLogged?.Invoke($"Rolled dice: {diceRoll}.");
            currentPlayer.Position = (currentPlayer.Position + diceRoll) % galaxy.TotalPositions;
            GameMessageLogged?.Invoke($"Moved to position {currentPlayer.Position}.");
            ProcessLocation(currentPlayer.Position);
            CheckIncomePayment();
            SaveGame();
            GameStateChanged?.Invoke();
        }

        public void SkipTurn()
        {
            Player currentPlayer = CurrentPlayer;
            if (currentPlayer.CanSkipTurn())
            {
                currentPlayer.SkipTurn();
                GameMessageLogged?.Invoke("Turn skipped.");
            }
            else
            {
                GameMessageLogged?.Invoke("Cannot skip more than 2 turns in a row.");
            }
            CheckIncomePayment();
            SaveGame();
            NextPlayer();
            GameStateChanged?.Invoke();
        }

        public void ProcessCard(ChangeCard card)
        {
            Player currentPlayer = CurrentPlayer;
            if (!currentPlayer.Cards.Contains(card))
            {
                GameMessageLogged?.Invoke("Invalid card selected.");
                return;
            }

            switch (card.Type)
            {
                case CardType.GalacticTicket:
                    if (galaxy.IsRailwayStationAt(currentPlayer.Position))
                    {
                        events.ProcessGalacticTicket(currentPlayer);
                        currentPlayer.Cards.Remove(card);
                    }
                    else
                    {
                        GameMessageLogged?.Invoke("Galactic Ticket can only be used at a Railway Station.");
                    }
                    break;
                case CardType.PirateDefense:
                    GameMessageLogged?.Invoke("Pirate Defense card is used automatically during pirate attacks.");
                    break;
                default:
                    GameMessageLogged?.Invoke("This card cannot be used manually.");
                    break;
            }
            GameStateChanged?.Invoke();
        }

        public List<(Planet Planet, string DisplayText)> GetBuildablePlanets()
        {
            Player currentPlayer = CurrentPlayer;
            if (!galaxy.IsSolarSystemAt(currentPlayer.Position))
            {
                GameMessageLogged?.Invoke("Must be in a solar system to build or upgrade structures.");
                return new List<(Planet, string)>();
            }

            var system = galaxy.GetSolarSystemAt(currentPlayer.Position);
            return system.Planets.Select(p =>
            {
                string ownerInfo = p.Owner == null ? "No owner" : $"Owned by: {p.Owner.Name}";
                return (p, $"{p.Name} - {ownerInfo}");
            }).ToList();
        }

        public void BuildOrUpgradeStructure(Planet selectedPlanet, bool buildNew, StructureType? structureType = null, Structure selectedStructure = null)
        {
            Player currentPlayer = CurrentPlayer;
            if (selectedPlanet == null || selectedPlanet.ParentSystem != galaxy.GetSolarSystemAt(currentPlayer.Position))
            {
                GameMessageLogged?.Invoke("Invalid planet selected.");
                return;
            }

            if (selectedPlanet.Owner != null && selectedPlanet.Owner != currentPlayer)
            {
                GameMessageLogged?.Invoke("You do not own this planet.");
                return;
            }

            if (selectedPlanet.Owner == null)
            {
                int cost = economy.BuildingCosts[StructureType.Spaceport];
                if (economy.CanAfford(currentPlayer, cost))
                {
                    economy.ChargeFee(currentPlayer, cost);
                    selectedPlanet.BuildSpaceport(currentPlayer);
                    GameMessageLogged?.Invoke($"Built spaceport on {selectedPlanet.Name} and took ownership!");
                }
                else
                {
                    GameMessageLogged?.Invoke($"Cannot afford spaceport (cost: {cost} credits).");
                }
            }
            else if (buildNew && structureType.HasValue)
            {
                if (selectedPlanet.CanBuildStructure(structureType.Value, currentPlayer))
                {
                    int cost = economy.BuildingCosts[structureType.Value];
                    if (economy.CanAfford(currentPlayer, cost))
                    {
                        economy.ChargeFee(currentPlayer, cost);
                        selectedPlanet.BuildStructure(structureType.Value, currentPlayer);
                        GameMessageLogged?.Invoke($"Built {structureType.Value} on {selectedPlanet.Name} for {cost} credits.");
                    }
                    else
                    {
                        GameMessageLogged?.Invoke($"Cannot afford {structureType.Value} (cost: {cost} credits).");
                    }
                }
                else
                {
                    GameMessageLogged?.Invoke($"Cannot build {structureType.Value} on this planet.");
                }
            }
            else if (!buildNew && selectedStructure != null && selectedStructure.CanUpgrade())
            {
                int cost = GetUpgradeCost(selectedStructure);
                if (economy.CanAfford(currentPlayer, cost))
                {
                    economy.ChargeFee(currentPlayer, cost);
                    selectedPlanet.UpgradeStructure(selectedStructure, currentPlayer);
                    GameMessageLogged?.Invoke($"Upgraded {selectedStructure.Type} to Level {selectedStructure.Level}!");
                }
                else
                {
                    GameMessageLogged?.Invoke($"Cannot afford upgrade (cost: {cost} credits).");
                }
            }

            SaveGame();
            GameStateChanged?.Invoke();
        }

        public List<(StructureType Type, int Cost)> GetAvailableStructures(Planet planet)
        {
            Player currentPlayer = CurrentPlayer;
            var structures = new List<(StructureType Type, int Cost)>
            {
                (StructureType.Outpost, economy.BuildingCosts[StructureType.Outpost]),
                (StructureType.Mine, economy.BuildingCosts[StructureType.Mine]),
                (StructureType.FoodFarm, economy.BuildingCosts[StructureType.FoodFarm])
            };

            if (currentPlayer.OwnsAllPlanetsInSystem(planet.ParentSystem))
            {
                structures.Add((StructureType.GalacticShipyard, economy.BuildingCosts[StructureType.GalacticShipyard]));
                structures.Add((StructureType.AsteroidMine, economy.BuildingCosts[StructureType.AsteroidMine]));
            }

            return structures.Where(s => planet.CanBuildStructure(s.Type, currentPlayer)).ToList();
        }

        public List<Structure> GetUpgradableStructures(Planet planet)
        {
            return planet.Structures.Where(s => s.CanUpgrade()).ToList();
        }

        private void ProcessLocation(int position)
        {
            Player currentPlayer = CurrentPlayer;
            if (galaxy.IsAnomalyAt(position))
            {
                GameMessageLogged?.Invoke("Encountered an anomaly! Drawing a card...");
                var card = events.DrawAnomalyCard(currentPlayer);
                GameMessageLogged?.Invoke($"Drew card: {card.Type} - {card.Description}");
                events.ProcessCard(card, currentPlayer);
            }
            else if (galaxy.IsPirateAt(position))
            {
                GameMessageLogged?.Invoke("Pirates attack!");
                events.ProcessPirateAttack(currentPlayer);
            }
            else if (galaxy.IsSolarSystemAt(position))
            {
                var system = galaxy.GetSolarSystemAt(position);
                GameMessageLogged?.Invoke($"Arrived at system {system.Name}.");
            }
            else if (galaxy.IsRailwayStationAt(position))
            {
                GameMessageLogged?.Invoke("Arrived at a railway station.");
            }
            GameStateChanged?.Invoke();
        }

        private void CheckIncomePayment()
        {
            if (currentTurn % economy.TurnsBetweenPayments == 0)
            {
                foreach (var player in players)
                {
                    int income = economy.CalculatePlayerIncome(player);
                    economy.AddCredits(player, income);
                    GameMessageLogged?.Invoke($"{player.Name} received {income} credits of income!");
                }
            }
        }

        private void NextPlayer()
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            if (currentPlayerIndex == 0)
            {
                currentTurn++;
            }
            GameStateChanged?.Invoke();
        }

        private void SaveGame()
        {
            JsonManager.SaveGame(players, galaxy, economy, currentTurn, currentPlayerIndex);
        }

        private int GetUpgradeCost(Structure structure)
        {
            return economy.UpgradeCosts.TryGetValue(structure.Type, out int cost) ? cost : 1000;
        }

        public void QuitGame()
        {
            GameMessageLogged?.Invoke("Saving game and exiting...");
            SaveGame();
            Environment.Exit(0);
        }
    }
}