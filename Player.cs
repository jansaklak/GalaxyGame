using System;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace GalaxyGame
{
    public class Player
    {
        public string Name { get; set; }
        public int Credits { get; set; }
        public int Position { get; set; }
        public List<Planet> OwnedPlanets { get; set; } = new List<Planet>();
        public List<ChangeCard> Cards { get; set; } = new List<ChangeCard>();
        public int SkippedTurnsCounter { get; set; }
        public bool IsSkippingTurn { get; set; }
        public int LostTurns { get; set; }

        public Player(string name, int startingCredits)
        {
            Name = name;
            Credits = startingCredits;
            Position = 0;
            SkippedTurnsCounter = 0;
            IsSkippingTurn = false;
            LostTurns = 0;
        }

        public bool CanSkipTurn()
        {
            return SkippedTurnsCounter < 2;
        }

        public void SkipTurn()
        {
            if (CanSkipTurn())
            {
                IsSkippingTurn = true;
                SkippedTurnsCounter++;
            }
            else
            {
                throw new InvalidOperationException("Cannot skip more than 2 turns in a row.");
            }
        }

        public void TakeTurn()
        {
            IsSkippingTurn = false;
            SkippedTurnsCounter = 0;
        }

        public bool HasCard(CardType type)
        {
            return Cards.Exists(card => card.Type == type);
        }

        public void UseCard(CardType type)
        {
            var cardIndex = Cards.FindIndex(card => card.Type == type);
            if (cardIndex != -1)
            {
                Cards.RemoveAt(cardIndex);
            }
            else
            {
                throw new InvalidOperationException($"Player does not have a {type} card.");
            }
        }

        public bool OwnsAllPlanetsInSystem(SolarSystem system)
        {
            foreach (var planet in system.Planets)
            {
                if (!OwnedPlanets.Contains(planet))
                {
                    return false;
                }
            }
            return true;
        }

        public int CountOwnedStructures(StructureType type)
        {
            int count = 0;
            foreach (var planet in OwnedPlanets)
            {
                count += planet.Structures.Count(s => s.Type == type);
            }
            return count;
        }

        public int CountGalacticShipyards()
        {
            return CountOwnedStructures(StructureType.GalacticShipyard);
        }
    }
}