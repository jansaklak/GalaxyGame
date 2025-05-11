using Newtonsoft.Json;

namespace GalaxyGame
{

    public enum CardType
    {
        PirateAttack,
        PirateDefense,
        GalacticTicket,
        ImperialTax,
        GalacticLottery,
        EngineFailure,
        ShipyardMalfunction
    }

    public class ChangeCard
    {
        public CardType Type { get; set; }
        public string Description { get; set; }
        public Action Effect { get; set; }

        public ChangeCard(CardType type, string description)
        {
            Type = type;
            Description = description;
        }

        public static ChangeCard CreatePirateAttackCard()
        {
            return new ChangeCard(CardType.PirateAttack, "You have been attacked by pirates! Lose 2 turns or pay a ransom.");
        }

        public static ChangeCard CreatePirateDefenseCard()
        {
            return new ChangeCard(CardType.PirateDefense, "This card protects you from pirate attacks.");
        }

        public static ChangeCard CreateGalacticTicketCard()
        {
            return new ChangeCard(CardType.GalacticTicket, "This ticket allows you to travel to any galactic railway station.");
        }

        public static ChangeCard CreateImperialTaxCard()
        {
            return new ChangeCard(CardType.ImperialTax, "The Emperor has imposed a one-time property tax!");
        }

        public static ChangeCard CreateGalacticLotteryCard(int winAmount)
        {
            return new ChangeCard(CardType.GalacticLottery, $"You won {winAmount} galactic credits in the lottery!");
        }

        public static ChangeCard CreateEngineFailureCard()
        {
            return new ChangeCard(CardType.EngineFailure, "Engine failure! Lose a turn and pay for towing.");
        }

        public static ChangeCard CreateShipyardMalfunctionCard()
        {
            return new ChangeCard(CardType.ShipyardMalfunction, "Malfunction at your galactic shipyard! Pay for repairs or lose the shipyard.");
        }
    }
}