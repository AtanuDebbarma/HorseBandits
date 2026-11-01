using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace HorseBandits
{
    /// <summary>
    /// Custom troop items are merchandise so vanilla battle loot can select them,
    /// but culture Culture.horse_bandits keeps them out of normal town markets.
    /// Any copy that enters a loot roster is swapped for the matching vanilla item,
    /// iterating exact EquipmentElement rows so item modifiers are handled safely.
    /// </summary>
    internal sealed class HorseBanditsLootBehavior : CampaignBehaviorBase
    {
        // Bandit Javelins (x4 / x8) are intentionally NOT swapped: they drop and trade as themselves.

        private const string CustomLargeBagSimpleJavelinId = "horse_bandits_large_bag_simple_javelin";
        private const string VanillaSimpleJavelinId = "western_javelin_1_t2";

        private const string CustomStackedPiercingId = "horse_bandits_stacked_piercing_arrows";
        private const string VanillaPiercingArrowsId = "piercing_arrows";
        // Custom quiver holds 40; loot yields two vanilla piercing quivers (23 each).
        private const int PiercingArrowsPerCustomStack = 2;

        public override void RegisterEvents()
        {
            CampaignEvents.OnCollectLootsItemsEvent.AddNonSerializedListener(this, OnCollectLootItems);
            CampaignEvents.OnLootDistributedToPartyEvent.AddNonSerializedListener(this, OnLootDistributedToParty);
            CampaignEvents.ItemsLooted.AddNonSerializedListener(this, OnItemsLooted);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private static void OnCollectLootItems(PartyBase party, ItemRoster itemRoster)
        {
            ReplaceCustomLootItems(itemRoster);
        }

        private static void OnLootDistributedToParty(PartyBase winner, PartyBase defeated, ItemRoster lootedItems)
        {
            ReplaceCustomLootItems(lootedItems);
        }

        private static void OnItemsLooted(MobileParty mobileParty, ItemRoster items)
        {
            ReplaceCustomLootItems(items);
        }

        internal static void ReplaceCustomLootItems(ItemRoster roster)
        {
            if (roster == null || roster.Count == 0)
            {
                return;
            }

            ReplaceOneForOne(roster, CustomLargeBagSimpleJavelinId, VanillaSimpleJavelinId);
            ReplaceStackedPiercing(roster);
        }

        private static void ReplaceOneForOne(ItemRoster roster, string customId, string vanillaId)
        {
            ItemObject custom = MBObjectManager.Instance.GetObject<ItemObject>(customId);
            ItemObject vanilla = MBObjectManager.Instance.GetObject<ItemObject>(vanillaId);
            if (custom == null || vanilla == null)
            {
                return;
            }

            int count = RemoveAllExactMatches(roster, custom);
            if (count > 0)
            {
                roster.AddToCounts(vanilla, count);
            }
        }

        private static void ReplaceStackedPiercing(ItemRoster roster)
        {
            ItemObject custom = MBObjectManager.Instance.GetObject<ItemObject>(CustomStackedPiercingId);
            ItemObject piercing = MBObjectManager.Instance.GetObject<ItemObject>(VanillaPiercingArrowsId);
            if (custom == null || piercing == null)
            {
                return;
            }

            int count = RemoveAllExactMatches(roster, custom);
            if (count > 0)
            {
                roster.AddToCounts(piercing, count * PiercingArrowsPerCustomStack);
            }
        }

        /// <summary>
        /// Removes every roster row whose item matches <paramref name="custom"/>,
        /// including modified EquipmentElements. GetItemNumber/AddToCounts(ItemObject)
        /// only touch the unmodified element and would leave variants behind.
        /// </summary>
        private static int RemoveAllExactMatches(ItemRoster roster, ItemObject custom)
        {
            List<ItemRosterElement> matches = new List<ItemRosterElement>();
            int total = 0;
            for (int i = 0; i < roster.Count; i++)
            {
                ItemRosterElement element = roster.GetElementCopyAtIndex(i);
                if (element.EquipmentElement.Item == custom && element.Amount > 0)
                {
                    matches.Add(element);
                    total += element.Amount;
                }
            }

            for (int i = 0; i < matches.Count; i++)
            {
                ItemRosterElement element = matches[i];
                roster.AddToCounts(element.EquipmentElement, -element.Amount);
            }

            return total;
        }
    }
}
