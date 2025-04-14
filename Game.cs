using System;
using System.Collections.Generic;

public class Game
{
    private List<Player> players;  // Lista graczy
    private Galaxy galaxy;
    private Economy economy;
    private Events events;
    private int currentTurn;
    private int currentPlayerIndex;
    private readonly Random random = new Random();
    
    public Game()
    {
        // Try to load existing game
        var gameState = JsonManager.LoadGame();
        
        if (gameState != null)
        {
            players = gameState.Players;
            galaxy = gameState.Galaxy;
            economy = gameState.Economy;
            currentTurn = gameState.CurrentTurn;
            currentPlayerIndex = gameState.CurrentPlayerIndex;
            
            // Fix references that may be broken after deserialization
            JsonManager.FixReferences(gameState);
            
            events = new Events(economy, galaxy);
        }
        else
        {
            // Tworzenie nowej gry
            SetupNewGame();
        }
    }

    private void SetupNewGame()
    {
        Console.WriteLine("Witaj w Galactic Business Simulator!");
        Console.Write("Podaj liczbę graczy (1-4): ");
        int playerCount;
        while (!int.TryParse(Console.ReadLine(), out playerCount) || playerCount < 1 || playerCount > 4)
        {
            Console.Write("Nieprawidłowa wartość. Podaj liczbę graczy (1-4): ");
        }
        
        players = new List<Player>();
        for (int i = 0; i < playerCount; i++)
        {
            Console.Write($"Podaj nazwę gracza {i + 1}: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"Gracz {i + 1}";
            }
            
            players.Add(new Player(name, 5000));
        }
        
        galaxy = new Galaxy(40);
        galaxy.InitializeGalaxy();
        economy = new Economy();
        events = new Events(economy, galaxy);
        currentTurn = 1;
        currentPlayerIndex = 0;
    }

    public void StartGame()
    {
        bool gameRunning = true;
        
        Console.WriteLine("Witaj w Galactic Business Simulator!");
        Console.WriteLine("Jesteś biznesmenem galaktycznym podróżującym przez galaktykę.");
        Console.WriteLine("Buduj porty kosmiczne, kopalnie i więcej, by rozwijać swoje imperium biznesowe!");
        
        while (gameRunning)
        {
            Player currentPlayer = players[currentPlayerIndex];
            
            DisplayGameState();
            
            if (currentPlayer.LostTurns > 0)
            {
                Console.WriteLine($"{currentPlayer.Name} ma {currentPlayer.LostTurns} straconych tur.");
                currentPlayer.LostTurns--;
                NextPlayer();
                continue;
            }
            
            string action = GetPlayerAction();
            ProcessPlayerAction(action);
            
            // Sprawdź, czy należy wypłacić dochód
            if (currentTurn % economy.TurnsBetweenPayments == 0)
            {
                foreach (var player in players)
                {
                    int income = economy.CalculatePlayerIncome(player);
                    economy.AddCredits(player, income);
                    Console.WriteLine($"{player.Name} otrzymał {income} kredytów dochodu w tej turze!");
                }
            }
            
            // Zapisz grę po każdej turze
            JsonManager.SaveGame(players, galaxy, economy, currentTurn, currentPlayerIndex);
            NextPlayer();
        }
    }

    private void NextPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        
        // Jeśli wróciliśmy do pierwszego gracza, zwiększ numer tury
        if (currentPlayerIndex == 0)
        {
            currentTurn++;
        }
    }
    
    private void DisplayGameState()
    {
        Player currentPlayer = players[currentPlayerIndex];
        
        Console.WriteLine($"\n==== Tura {currentTurn} ====");
        Console.WriteLine($"Gracz: {currentPlayer.Name} (Gracz {currentPlayerIndex + 1} z {players.Count})");
        Console.WriteLine($"Kredyty: {currentPlayer.Credits}");
        Console.WriteLine($"Aktualna pozycja: {currentPlayer.Position}");
        Console.WriteLine($"Posiadane planety: {currentPlayer.OwnedPlanets.Count}");
        
        // Display cards
        if (currentPlayer.Cards.Count > 0)
        {
            Console.WriteLine("Twoje karty:");
            foreach (var card in currentPlayer.Cards)
            {
                Console.WriteLine($"- {card.Type}: {card.Description}");
            }
        }
        
        // Display current location info
        DisplayLocationInfo(currentPlayer.Position);
    }
    
    private void DisplayLocationInfo(int position)
    {
        Console.WriteLine($"\nInformacje o lokalizacji (Pozycja {position}):");
        
        if (galaxy.IsSolarSystemAt(position))
        {
            var system = galaxy.GetSolarSystemAt(position);
            Console.WriteLine($"Jesteś w systemie {system.Name}.");
            Console.WriteLine($"Planety w tym systemie:");
            
            foreach (var planet in system.Planets)
            {
                string ownerInfo = planet.Owner == null ? "Bez właściciela" : $"Własność: {planet.Owner.Name}";
                Console.WriteLine($"- {planet.Name}: {ownerInfo}");
                
                if (planet.HasSpaceport)
                {
                    Console.WriteLine("  Ma port kosmiczny");
                }
                
                if (planet.Structures.Count > 0)
                {
                    Console.WriteLine("  Budowle:");
                    foreach (var structure in planet.Structures)
                    {
                        Console.WriteLine($"  - {structure.Type} (Poziom {structure.Level})");
                    }
                }
            }
        }
        else if (galaxy.IsAnomalyAt(position))
        {
            Console.WriteLine("Jesteś w regionie Anomalii. Możesz dobrać kartę!");
        }
        else if (galaxy.IsPirateAt(position))
        {
            Console.WriteLine("Piraci! Przygotuj się do walki lub zapłać okup!");
        }
        else if (galaxy.IsRailwayStationAt(position))
        {
            Console.WriteLine("Jesteś na stacji kolejowej galaktycznej. Możesz użyć biletu do podróży.");
        }
        else
        {
            Console.WriteLine("Jesteś w pustej przestrzeni.");
        }
    }
    
    private string GetPlayerAction()
    {
        Console.WriteLine("\nDostępne akcje:");
        Console.WriteLine("1. Rzuć kością (Ruch)");
        Console.WriteLine("2. Pomiń turę");
        Console.WriteLine("3. Zbuduj/Ulepsz strukturę");
        Console.WriteLine("4. Użyj karty");
        Console.WriteLine("5. Zobacz mapę galaktyki");
        Console.WriteLine("6. Zakończ grę");
        
        Console.Write("Wprowadź swój wybór (1-6): ");
        return Console.ReadLine();
    }
    
    private void ProcessPlayerAction(string action)
    {
        switch (action)
        {
            case "1":
                MovePlayer();
                break;
            case "2":
                SkipTurn();
                break;
            case "3":
                BuildOrUpgradeStructure();
                break;
            case "4":
                UseCard();
                break;
            case "5":
                ViewGalaxyMap();
                break;
            case "6":
                QuitGame();
                break;
            default:
                Console.WriteLine("Nieprawidłowa akcja. Spróbuj ponownie.");
                break;
        }
    }
    
    private void MovePlayer()
    {
        Player currentPlayer = players[currentPlayerIndex];
        int diceRoll = random.Next(1, 7);
        Console.WriteLine($"Rzuciłeś kością: {diceRoll}.");
        
        // Move player
        currentPlayer.Position = (currentPlayer.Position + diceRoll) % galaxy.TotalPositions;
        Console.WriteLine($"Przemieszczono cię na pozycję {currentPlayer.Position}.");
        
        // Check location and process effects
        ProcessLocation(currentPlayer.Position);
    }
    
    private void ProcessLocation(int position)
    {
        Player currentPlayer = players[currentPlayerIndex];
        
        if (galaxy.IsAnomalyAt(position))
        {
            Console.WriteLine("Wpadłeś w anomalię! Dobrałeś kartę...");
            var card = events.DrawAnomalyCard(currentPlayer);
            Console.WriteLine($"Dobrałeś kartę: {card.Type} - {card.Description}");
            events.ProcessCard(card, currentPlayer);
        }
        else if (galaxy.IsPirateAt(position))
        {
            Console.WriteLine("Piraci atakują!");
            events.ProcessPirateAttack(currentPlayer);
        }
        else if (galaxy.IsSolarSystemAt(position))
        {
            var system = galaxy.GetSolarSystemAt(position);
            Console.WriteLine($"Dotarłeś do systemu {system.Name}.");
        }
        else if (galaxy.IsRailwayStationAt(position))
        {
            Console.WriteLine("Dotarłeś do stacji kolejowej.");
        }
    }
    
    private void SkipTurn()
    {
        Player currentPlayer = players[currentPlayerIndex];
        
        if (currentPlayer.CanSkipTurn())
        {
            currentPlayer.SkipTurn();
            Console.WriteLine("Pominięto turę.");
        }
        else
        {
            Console.WriteLine("Nie możesz pominąć więcej niż 2 tur pod rząd.");
        }
    }
    
    private void BuildOrUpgradeStructure()
    {
        Player currentPlayer = players[currentPlayerIndex];
        
        if (!galaxy.IsSolarSystemAt(currentPlayer.Position))
        {
            Console.WriteLine("Musisz być w systemie słonecznym, aby budować lub ulepszać struktury.");
            return;
        }
        
        var system = galaxy.GetSolarSystemAt(currentPlayer.Position);
        
        Console.WriteLine("Wybierz planetę:");
        for (int i = 0; i < system.Planets.Count; i++)
        {
            var planet = system.Planets[i];
            string ownerInfo = planet.Owner == null ? "Bez właściciela" : $"Własność: {planet.Owner.Name}";
            Console.WriteLine($"{i + 1}. {planet.Name} - {ownerInfo}");
        }
        
        Console.Write("Wprowadź numer planety lub 0, aby anulować: ");
        if (!int.TryParse(Console.ReadLine(), out int planetIndex) || planetIndex < 1 || planetIndex > system.Planets.Count)
        {
            Console.WriteLine("Nieprawidłowy wybór.");
            return;
        }
        
        var selectedPlanet = system.Planets[planetIndex - 1];
        
        // Check if player owns the planet or can build a spaceport
        if (selectedPlanet.Owner != null && selectedPlanet.Owner != currentPlayer)
        {
            Console.WriteLine("Nie posiadasz tej planety, nie możesz budować tutaj.");
            return;
        }
        
        if (selectedPlanet.Owner == null)
        {
            // Can only build a spaceport on unowned planets
            Console.WriteLine($"Zbudować port kosmiczny na {selectedPlanet.Name} za {economy.BuildingCosts[StructureType.Spaceport]} kredytów? (Y/N)");
            string response = Console.ReadLine().ToUpper();
            
            if (response == "Y")
            {
                if (economy.CanAfford(currentPlayer, economy.BuildingCosts[StructureType.Spaceport]))
                {
                    economy.ChargeFee(currentPlayer, economy.BuildingCosts[StructureType.Spaceport]);
                    selectedPlanet.BuildSpaceport(currentPlayer);
                    Console.WriteLine($"Zbudowałeś port kosmiczny na {selectedPlanet.Name} i przejąłeś kontrolę!");
                }
                else
                {
                    Console.WriteLine("Nie możesz pozwolić sobie na zbudowanie portu kosmicznego.");
                }
            }
            
            return;
        }
        
        // Player owns the planet, show build/upgrade options
        Console.WriteLine("Co chcesz zrobić?");
        Console.WriteLine("1. Zbudować nową strukturę");
        Console.WriteLine("2. Ulepszyć istniejącą strukturę");
        Console.Write("Wprowadź swój wybór (1-2) lub 0, aby anulować: ");
        
        if (!int.TryParse(Console.ReadLine(), out int actionChoice) || actionChoice < 1 || actionChoice > 2)
        {
            return;
        }
        
        if (actionChoice == 1)
        {
            BuildNewStructure(selectedPlanet);
        }
        else
        {
            UpgradeStructure(selectedPlanet);
        }
    }

    // Pozostała logika dotycząca budowy i ulepszania struktur
    // Oraz inne metody, takie jak `UseCard`, `ViewGalaxyMap` i `QuitGame`
    // pozostają niezmienione.

    private void QuitGame()
    {
        Console.WriteLine("Zapisuję grę i wychodzę...");
        JsonManager.SaveGame(players, galaxy, economy, currentTurn, currentPlayerIndex);
        Environment.Exit(0);
    }

    // Program entry point
    public static void Main(string[] args)
    {
        Game game = new Game();
        game.StartGame();
    }
}
