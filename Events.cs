using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Events
{
    private readonly Random random = new Random();
    private readonly List<ChangeCard> anomalyCards = new List<ChangeCard>();
    private readonly List<ChangeCard> shipyardAnomalyCards = new List<ChangeCard>();
    private readonly Economy economy;
    private readonly Galaxy galaxy;

    public Events(Economy economy, Galaxy galaxy)
    {
        this.economy = economy;
        this.galaxy = galaxy;
        InitializeAnomalyCards();
    }

    private void InitializeAnomalyCards()
    {
        // Standard anomaly cards
        anomalyCards.Add(ChangeCard.CreatePirateAttackCard());
        anomalyCards.Add(ChangeCard.CreatePirateDefenseCard());
        anomalyCards.Add(ChangeCard.CreateGalacticTicketCard());
        anomalyCards.Add(ChangeCard.CreateImperialTaxCard());
        anomalyCards.Add(ChangeCard.CreateGalacticLotteryCard(1000));
        anomalyCards.Add(ChangeCard.CreateEngineFailureCard());

        // Shipyard anomaly cards
        shipyardAnomalyCards.Add(ChangeCard.CreateShipyardMalfunctionCard());
    }

    public ChangeCard DrawAnomalyCard(Player player)
    {
        List<ChangeCard> availableCards = new List<ChangeCard>(anomalyCards);

        // Add shipyard malfunction card if player has at least one shipyard
        if (player.CountGalacticShipyards() > 0)
        {
            availableCards.AddRange(shipyardAnomalyCards);
        }

        int cardIndex = random.Next(availableCards.Count);
        return availableCards[cardIndex];
    }

    public void ProcessPirateAttack(Player player)
    {
        if (player.HasCard(CardType.PirateDefense))
        {
            Console.WriteLine("You used a Pirate Defense card to protect yourself!");
            player.UseCard(CardType.PirateDefense);
        }
        else if (economy.CanAfford(player, economy.PirateRansom))
        {
            Console.WriteLine($"You paid {economy.PirateRansom} credits as ransom to the pirates.");
            economy.ChargeFee(player, economy.PirateRansom);
        }
        else
        {
            Console.WriteLine("You lost 2 turns to the pirate attack!");
            player.LostTurns += 2;
        }
    }

    public void ProcessImperialTax(Player player)
    {
        int taxAmount = economy.CalculatePropertyTax(player);
        Console.WriteLine($"The Emperor has imposed a one-time property tax of {taxAmount} credits!");
        
        if (economy.CanAfford(player, taxAmount))
        {
            economy.ChargeFee(player, taxAmount);
        }
        else
        {
            Console.WriteLine("You cannot afford to pay the tax! You must sell some of your properties.");
            // Logic for forcing player to sell properties would go here
        }
    }

    public void ProcessGalacticLottery(Player player, int winAmount)
    {
        Console.WriteLine($"Congratulations! You won {winAmount} credits in the Galactic Lottery!");
        economy.AddCredits(player, winAmount);
    }

    public void ProcessEngineFailure(Player player)
    {
        Console.WriteLine($"Your ship's engine has failed. You lose a turn and must pay {economy.TowingCost} credits for towing.");
        player.LostTurns += 1;
        
        if (economy.CanAfford(player, economy.TowingCost))
        {
            economy.ChargeFee(player, economy.TowingCost);
        }
        else
        {
            Console.WriteLine("You cannot afford the towing cost. You must sell some of your properties.");
            // Logic for forcing player to sell properties would go here
        }
    }

    public void ProcessShipyardMalfunction(Player player)
    {
        Console.WriteLine($"There's been a malfunction at one of your shipyards! Repair cost: {economy.ShipyardRepairCost} credits.");
        
        if (economy.CanAfford(player, economy.ShipyardRepairCost))
        {
            economy.ChargeFee(player, economy.ShipyardRepairCost);
        }
        else
        {
            Console.WriteLine("You cannot afford the repair costs. You lose a shipyard!");
            
            // Find a planet with a shipyard and remove it
            foreach (var planet in player.OwnedPlanets)
            {
                var shipyard = planet.Structures.Find(s => s.Type == StructureType.GalacticShipyard);
                if (shipyard != null)
                {
                    planet.Structures.Remove(shipyard);
                    Console.WriteLine($"You lost the shipyard on {planet.Name}.");
                    break;
                }
            }
        }
    }

    public void ProcessGalacticTicket(Player player)
    {
        // Show all railway stations and let player choose one
        List<int> railwayStations = galaxy.GetRailwayStations();
        
        Console.WriteLine("You can use your Galactic Ticket to travel to any of these stations:");
        for (int i = 0; i < railwayStations.Count; i++)
        {
            Console.WriteLine($"{i + 1}. Position {railwayStations[i]}");
        }
        
        // Logic for player selection would go here
        // For now, just using the first station as an example
        if (railwayStations.Count > 0)
        {
            player.Position = railwayStations[0];
            player.UseCard(CardType.GalacticTicket);
        }
    }

    public void ProcessCard(ChangeCard card, Player player)
    {
        switch (card.Type)
        {
            case CardType.PirateAttack:
                ProcessPirateAttack(player);
                break;
            case CardType.PirateDefense:
            case CardType.GalacticTicket:
                player.Cards.Add(card);
                Console.WriteLine($"You received a {card.Type} card!");
                break;
            case CardType.ImperialTax:
                ProcessImperialTax(player);
                break;
            case CardType.GalacticLottery:
                ProcessGalacticLottery(player, 1000); // Example amount
                break;
            case CardType.EngineFailure:
                ProcessEngineFailure(player);
                break;
            case CardType.ShipyardMalfunction:
                ProcessShipyardMalfunction(player);
                break;
        }
    }
}