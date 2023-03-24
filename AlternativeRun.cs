using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlternativeRun
{
    [BepInPlugin("Dexy-AlternativeRun", "AlternativeRun", "0.0.1")]
    public class AlternativeRun : BaseUnityPlugin
    {

        internal static new ManualLogSource Logger { get; set; }
        internal static int totalPrice = 0;
        internal static int finalPrice = 0;

        private void Awake()
        {

            // Get the BepInEx Logger //
            AlternativeRun.Logger = base.Logger;

            // Log the Start of the Plugin //
            Logger.LogInfo("Plugin AlternativeRun Added!");

            // Create the Hook //
            Utils.Hooks.AddHooks(typeof(Hero), nameof(Hero.SetInitalCards), SetInitalCardsHook);
            Utils.Hooks.AddHooks(typeof(AtOManager), nameof(AtOManager.InitGame), AddDustHook);
            Utils.Hooks.AddHooks(typeof(CardCraftManager), nameof(CardCraftManager.GetCardAvailability), UpStockHook);

        }

        public void SetInitalCardsHook(Action<Hero, HeroData> orig, Hero self, HeroData heroData)
        {

            // Gets all Carts //
            SubClassData heroSubClass = heroData.HeroSubClass;
            if (heroSubClass == null) goto end;
            HeroCards[] cards = heroSubClass.Cards;
            if (cards == null) goto end;

            // Create the List //
            List<HeroCards> newList = new List<HeroCards>();

            // Itinerate all Cards //
            for (int i = 0; i < cards.Length; i++)
            {
                // Get the Card Data //
                HeroCards heroCard = cards[i];
                if (heroCard == null) continue;
                CardData cardData = heroCard.Card;

                // Remove the Card //
                if (cardData != null && cardData.cardClass == Enums.CardClass.Special)
                {
                    newList.Add(heroCard);
                }
                else if (cardData != null)
                {
                    int price = Globals.Instance.GetCraftCost(cardData.id);
                    AlternativeRun.totalPrice += price * cards[i].unitsInDeck;
                    Logger.LogInfo("Removed Card from initial Deck: " + cardData.cardName + " x " + cards[i].unitsInDeck + " and retrieve it's price: " + price + " x " + cards[i].unitsInDeck);
                }
            }

            // Log the total Price //
            Logger.LogInfo("Total removed Cards price: " + AlternativeRun.totalPrice);

            // Get the Price for 4 Heros (Instead of 16) //
            AlternativeRun.finalPrice = (int)Math.Ceiling((float)AlternativeRun.totalPrice / 16f * 4f);

            // Log the final Price //
            Logger.LogInfo("Final price for 4 Heros: " + AlternativeRun.finalPrice);

            // Save the List //
            heroSubClass.Cards = newList.ToArray();

            // Call the original Function //
            end:
            orig(self, heroData);

        }        

        public void AddDustHook(Action<AtOManager> orig, AtOManager self)
        {
            // Call the Original Function //
            orig(self);
            // Give the price amount of removed Cards to the Player //
            self.GivePlayer(1, AlternativeRun.finalPrice, "", "", false, false);
            Logger.LogInfo("The Player received: " + AlternativeRun.finalPrice);
        }

        public int[] UpStockHook(Func<CardCraftManager, string, string, int[]> orig, CardCraftManager self, string cardId, string shopId)
        {
            CardData cardData = Globals.Instance.GetCardData(cardId, false);
            int num;
            CardData cardData2;
            if (cardData.CardUpgraded != Enums.CardUpgraded.No && cardData.UpgradedFrom != "")
            {
                num = AtOManager.Instance.HowManyCrafted(self.heroIndex, cardData.UpgradedFrom.ToLower());
                cardData2 = Globals.Instance.GetCardData(cardData.UpgradedFrom.ToLower(), true);
            }
            else
            {
                num = AtOManager.Instance.HowManyCrafted(self.heroIndex, cardId);
                cardData2 = cardData;
            }
            int num2;
            if (cardData2.CardClass == Enums.CardClass.Item)
            {
                if (cardData2.CardType == Enums.CardType.Pet)
                {
                    if (!PlayerManager.Instance.IsCardUnlocked(cardData2.Id))
                    {
                        num2 = 0;
                    }
                    else
                    {
                        num2 = 4;
                    }
                }
                else
                {
                    num2 = 4;
                }
                if (AtOManager.Instance.ItemBoughtOnThisShop(shopId, cardId) || (cardData2.CardType == Enums.CardType.Pet && AtOManager.Instance.TeamHaveItem(cardId, 5, true)))
                {
                    num = 1;
                }
            }
            else if (cardData2.CardRarity == Enums.CardRarity.Common)
            {
                num2 = 4;
                if (PlayerManager.Instance.PlayerHaveSupply("townUpgrade_1_1"))
                {
                    num2 = 5;
                }
            }
            else if (cardData2.CardRarity == Enums.CardRarity.Uncommon)
            {
                num2 = 4;
                if (PlayerManager.Instance.PlayerHaveSupply("townUpgrade_1_3") && AtOManager.Instance.GetNgPlus() < 5)
                {
                    num2 = 5;
                }
            }
            else
            {
                num2 = 4;
            }
            if (num > num2)
            {
                num = num2;
            }
            return new int[]
            {
            num,
            num2
            };
        }

    }

}
