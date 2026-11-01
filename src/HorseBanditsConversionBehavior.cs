using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace HorseBandits
{
    /// <summary>
    /// Converts a share of existing vanilla bandit parties in Empire / Vlandia / Sturgia
    /// regions so part of their roster becomes Horse Bandits troops. Does not create
    /// settlements, change party clans/cultures, or mutate the map.
    /// Which clans are eligible, and how much of a roster is replaced, comes from MCM.
    /// The defaults match the previous hardcoded behavior, including Corsairs left off.
    /// </summary>
    internal sealed class HorseBanditsConversionBehavior : CampaignBehaviorBase
    {
        private const float PartyConversionChance = 0.35f;

        private static readonly string[] TargetTownCultureIds = new string[]
        {
            "empire",
            "vlandia",
            "sturgia"
        };

        // Only base Bandit troops. Soldier upgrades (Huntsman/Packmaster/Horseman/Elite)
        // require Veteran's Respect to unlock and must not spawn via conversion.
        private static readonly string[] ConversionTroopIds = new string[]
        {
            "mounted_pillager",
            "mounted_ransacker"
        };

        private static readonly string[] BossPartyExtraTroopIds = new string[]
        {
            "horse_bandits_chief",
            "horse_bandits_boss"
        };

        // Strip these from already-converted parties (older saves injected them).
        private static readonly string[] VeteransRespectGatedTroopIds = new string[]
        {
            "horse_bandits_mounted_huntsman",
            "horse_bandits_mounted_packmaster",
            "horse_bandits_imperial_bandit_horseman",
            "horse_bandits_imperial_elite_bandit_horseman",
            "horse_bandits_vlandian_brigand_champion",
            "horse_bandits_vlandian_brigand_banner_knight"
        };

        // Successful conversions — never convert again.
        private List<string> _convertedPartyIds = new List<string>();
        // Intentional random skips — do not re-roll daily.
        private List<string> _skippedPartyIds = new List<string>();
        // Passed the 35% roll but conversion failed technically — retry without re-rolling.
        private List<string> _pendingConversionPartyIds = new List<string>();

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, OnMobilePartyDestroyed);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("HorseBanditsConvertedBanditPartyIds", ref _convertedPartyIds);
            dataStore.SyncData("HorseBanditsSkippedBanditPartyIds", ref _skippedPartyIds);
            dataStore.SyncData("HorseBanditsPendingConversionBanditPartyIds", ref _pendingConversionPartyIds);
            if (_convertedPartyIds == null)
            {
                _convertedPartyIds = new List<string>();
            }
            if (_skippedPartyIds == null)
            {
                _skippedPartyIds = new List<string>();
            }
            if (_pendingConversionPartyIds == null)
            {
                _pendingConversionPartyIds = new List<string>();
            }
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            HorseBanditsSettings.ApplyHighwaymanBranch();
            PruneMissingParties();
            SanitizeConvertedRosters();
        }

        private void OnDailyTick()
        {
            PruneMissingParties();
            SanitizeConvertedRosters();
            ProcessEligibleParties();
        }

        private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyer)
        {
            if (mobileParty == null || string.IsNullOrEmpty(mobileParty.StringId))
            {
                return;
            }

            if (_convertedPartyIds != null)
            {
                _convertedPartyIds.Remove(mobileParty.StringId);
            }
            if (_skippedPartyIds != null)
            {
                _skippedPartyIds.Remove(mobileParty.StringId);
            }
            if (_pendingConversionPartyIds != null)
            {
                _pendingConversionPartyIds.Remove(mobileParty.StringId);
            }
        }

        private void PruneMissingParties()
        {
            _convertedPartyIds = PruneList(_convertedPartyIds);
            _skippedPartyIds = PruneList(_skippedPartyIds);
            _pendingConversionPartyIds = PruneList(_pendingConversionPartyIds);
        }

        private static List<string> PruneList(List<string> source)
        {
            if (source == null || source.Count == 0)
            {
                return new List<string>();
            }

            List<string> keep = new List<string>();
            for (int i = 0; i < source.Count; i++)
            {
                string id = source[i];
                if (!string.IsNullOrEmpty(id) && FindBanditPartyById(id) != null)
                {
                    keep.Add(id);
                }
            }
            return keep;
        }

        private static MobileParty FindBanditPartyById(string stringId)
        {
            MBReadOnlyList<MobileParty> bandits = MobileParty.AllBanditParties;
            if (bandits == null)
            {
                return null;
            }

            for (int i = 0; i < bandits.Count; i++)
            {
                MobileParty party = bandits[i];
                if (party != null && party.StringId == stringId)
                {
                    return party;
                }
            }

            return null;
        }

        private void ProcessEligibleParties()
        {
            MBReadOnlyList<MobileParty> bandits = MobileParty.AllBanditParties;
            if (bandits == null || bandits.Count == 0)
            {
                return;
            }

            for (int i = 0; i < bandits.Count; i++)
            {
                MobileParty party = bandits[i];
                if (!IsEligibleForConversion(party))
                {
                    continue;
                }

                string id = party.StringId;
                if (_convertedPartyIds.Contains(id) || _skippedPartyIds.Contains(id))
                {
                    continue;
                }

                bool pending = _pendingConversionPartyIds.Contains(id);
                if (!pending)
                {
                    if (MBRandom.RandomFloat > PartyConversionChance)
                    {
                        _skippedPartyIds.Add(id);
                        continue;
                    }

                    _pendingConversionPartyIds.Add(id);
                }

                if (TryConvertRoster(party))
                {
                    _convertedPartyIds.Add(id);
                    _pendingConversionPartyIds.Remove(id);
                }
                // Technical failure: stays pending and retries next day without re-rolling.
            }
        }

        private static bool IsEligibleForConversion(MobileParty party)
        {
            if (party == null || !party.IsActive || party.IsDisbanding)
            {
                return false;
            }

            if (!party.IsBandit)
            {
                return false;
            }

            if (party.IsMainParty || party.IsLordParty || party.IsCaravan || party.IsVillager
                || party.IsMilitia || party.IsGarrison)
            {
                return false;
            }

            if (party.IsCurrentlyUsedByAQuest)
            {
                return false;
            }

            if (party.MapEvent != null || party.SiegeEvent != null)
            {
                return false;
            }

            // War Sails: do not put cavalry / horse archers on ships or naval-only parties.
            if (party.IsCurrentlyAtSea)
            {
                return false;
            }
            if (party.HasNavalNavigationCapability && !party.HasLandNavigationCapability)
            {
                return false;
            }

            if (party.MemberRoster == null || party.MemberRoster.TotalRegulars <= 0)
            {
                return false;
            }

            if (string.IsNullOrEmpty(party.StringId))
            {
                return false;
            }

            int ignoredPercent;
            if (!HorseBanditsSettings.AllowsParty(GetClanId(party), GetCultureId(party), out ignoredPercent))
            {
                return false;
            }

            return IsInTargetRegion(party);
        }

        private static string GetClanId(MobileParty party)
        {
            if (party.ActualClan != null && !string.IsNullOrEmpty(party.ActualClan.StringId))
            {
                return party.ActualClan.StringId;
            }

            return null;
        }

        private static string GetCultureId(MobileParty party)
        {
            if (party.ActualClan != null && party.ActualClan.Culture != null)
            {
                return party.ActualClan.Culture.StringId;
            }

            if (party.HomeSettlement != null && party.HomeSettlement.Culture != null)
            {
                return party.HomeSettlement.Culture.StringId;
            }

            return null;
        }

        private static bool TryGetRosterPercent(MobileParty party, out int percent)
        {
            return HorseBanditsSettings.AllowsParty(GetClanId(party), GetCultureId(party), out percent);
        }

        /// <summary>
        /// Removes VR-gated Soldier upgrades from previously converted parties and
        /// replaces them with Pillager / Ransacker so existing saves stay safe.
        /// </summary>
        private void SanitizeConvertedRosters()
        {
            if (_convertedPartyIds == null || _convertedPartyIds.Count == 0)
            {
                return;
            }

            List<CharacterObject> replacementPool;
            if (!TryBuildPillagerPool(out replacementPool) || replacementPool == null || replacementPool.Count == 0)
            {
                return;
            }

            for (int i = 0; i < _convertedPartyIds.Count; i++)
            {
                MobileParty party = FindBanditPartyById(_convertedPartyIds[i]);
                if (party == null || party.MemberRoster == null)
                {
                    continue;
                }

                StripGatedTroops(party.MemberRoster, replacementPool);
            }
        }

        private static void StripGatedTroops(TroopRoster roster, List<CharacterObject> replacementPool)
        {
            for (int g = 0; g < VeteransRespectGatedTroopIds.Length; g++)
            {
                CharacterObject gated = MBObjectManager.Instance.GetObject<CharacterObject>(VeteransRespectGatedTroopIds[g]);
                if (gated == null)
                {
                    continue;
                }

                int total = 0;
                int wounded = 0;
                for (int i = 0; i < roster.Count; i++)
                {
                    TroopRosterElement element = roster.GetElementCopyAtIndex(i);
                    if (element.Character == gated && element.Number > 0)
                    {
                        total += element.Number;
                        wounded += element.WoundedNumber > 0 ? element.WoundedNumber : 0;
                    }
                }

                if (total <= 0)
                {
                    continue;
                }

                if (wounded > total)
                {
                    wounded = total;
                }

                roster.AddToCounts(gated, -total, false, -wounded, 0, true, -1);

                int remaining = total;
                int woundedLeft = wounded;
                while (remaining > 0)
                {
                    CharacterObject troop = replacementPool[MBRandom.RandomInt(replacementPool.Count)];
                    int chunk = remaining < 5 ? remaining : MBRandom.RandomInt(1, 6);
                    if (chunk > remaining)
                    {
                        chunk = remaining;
                    }

                    int chunkWounded = 0;
                    if (woundedLeft > 0)
                    {
                        chunkWounded = woundedLeft > chunk ? chunk : woundedLeft;
                    }

                    roster.AddToCounts(troop, chunk, false, chunkWounded, 0, true, -1);
                    remaining -= chunk;
                    woundedLeft -= chunkWounded;
                }
            }
        }

        /// <summary>
        /// Region = nearest town culture is Empire, Vlandia, or Sturgia, based on the
        /// party's current map position (or CurrentSettlement if inside one).
        /// </summary>
        private static bool IsInTargetRegion(MobileParty party)
        {
            Settlement nearestTown = FindNearestTown(party);
            if (nearestTown == null || nearestTown.Culture == null)
            {
                return false;
            }

            string cultureId = nearestTown.Culture.StringId;
            for (int i = 0; i < TargetTownCultureIds.Length; i++)
            {
                if (cultureId == TargetTownCultureIds[i])
                {
                    return true;
                }
            }

            return false;
        }

        private static Settlement FindNearestTown(MobileParty party)
        {
            // Prefer live geography: current settlement if entered, otherwise party position.
            CampaignVec2 origin = party.Position;
            if (party.CurrentSettlement != null)
            {
                origin = party.CurrentSettlement.Position;
            }

            MBReadOnlyList<Town> towns = Town.AllTowns;
            if (towns == null || towns.Count == 0)
            {
                return null;
            }

            Settlement best = null;
            float bestDist = float.MaxValue;
            for (int i = 0; i < towns.Count; i++)
            {
                Town town = towns[i];
                if (town == null || town.Settlement == null)
                {
                    continue;
                }

                float dx = origin.X - town.Settlement.Position.X;
                float dy = origin.Y - town.Settlement.Position.Y;
                float dist = dx * dx + dy * dy;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = town.Settlement;
                }
            }

            return best;
        }

        private static bool TryConvertRoster(MobileParty party)
        {
            TroopRoster roster = party.MemberRoster;
            if (roster == null)
            {
                return false;
            }

            int rosterPercent;
            if (!TryGetRosterPercent(party, out rosterPercent))
            {
                return false;
            }

            float rosterRatio = rosterPercent / 100f;

            List<CharacterObject> pool;
            if (!TryBuildPillagerPool(out pool))
            {
                return false;
            }

            List<TroopRosterElement> regulars = new List<TroopRosterElement>();
            int totalRegulars = 0;
            for (int i = 0; i < roster.Count; i++)
            {
                TroopRosterElement element = roster.GetElementCopyAtIndex(i);
                if (element.Character == null || element.Character.IsHero || element.Number <= 0)
                {
                    continue;
                }

                regulars.Add(element);
                totalRegulars += element.Number;
            }

            if (totalRegulars <= 0 || regulars.Count == 0)
            {
                return false;
            }

            int toReplace = (int)Math.Round(totalRegulars * rosterRatio);
            if (toReplace < 1)
            {
                toReplace = 1;
            }
            if (toReplace > totalRegulars)
            {
                toReplace = totalRegulars;
            }

            // Plan removals first; only mutate the roster after the plan is valid.
            List<RosterDelta> removals = new List<RosterDelta>();
            int plannedTotal = 0;
            int plannedWounded = 0;

            for (int i = 0; i < regulars.Count && plannedTotal < toReplace; i++)
            {
                TroopRosterElement element = regulars[i];
                int stackShare = (int)Math.Round(element.Number * rosterRatio);
                if (stackShare < 1)
                {
                    stackShare = 1;
                }

                int remainingNeed = toReplace - plannedTotal;
                if (stackShare > remainingNeed)
                {
                    stackShare = remainingNeed;
                }
                if (stackShare > element.Number)
                {
                    stackShare = element.Number;
                }
                if (stackShare <= 0)
                {
                    continue;
                }

                int woundedInStack = element.WoundedNumber;
                if (woundedInStack < 0)
                {
                    woundedInStack = 0;
                }
                if (woundedInStack > element.Number)
                {
                    woundedInStack = element.Number;
                }

                int woundedRemove = 0;
                if (element.Number > 0)
                {
                    woundedRemove = (int)Math.Round(woundedInStack * ((float)stackShare / (float)element.Number));
                }
                if (woundedRemove > stackShare)
                {
                    woundedRemove = stackShare;
                }
                if (woundedRemove > woundedInStack)
                {
                    woundedRemove = woundedInStack;
                }

                RosterDelta plan = new RosterDelta();
                plan.Character = element.Character;
                plan.Count = stackShare;
                plan.Wounded = woundedRemove;
                removals.Add(plan);
                plannedTotal += stackShare;
                plannedWounded += woundedRemove;
            }

            if (plannedTotal <= 0 || removals.Count == 0)
            {
                return false;
            }

            int chiefTotal = 0;
            int chiefPercent;
            if (party.IsBanditBossParty && HorseBanditsSettings.TryChiefShare(out chiefPercent))
            {
                chiefTotal = (int)Math.Round(plannedTotal * (chiefPercent / 100f));
                if (chiefTotal > plannedTotal)
                {
                    chiefTotal = plannedTotal;
                }
                if (chiefTotal < 0)
                {
                    chiefTotal = 0;
                }
            }

            int regularCount = plannedTotal - chiefTotal;
            int chiefCount = chiefTotal / 2;
            int bossCount = chiefTotal - chiefCount;
            if (chiefTotal > 0 && MBRandom.RandomInt(2) == 0)
            {
                int swap = chiefCount;
                chiefCount = bossCount;
                bossCount = swap;
            }

            CharacterObject chiefTroop = null;
            CharacterObject bossTroop = null;
            if (chiefTotal > 0 && !TryResolveChiefAndBoss(out chiefTroop, out bossTroop))
            {
                return false;
            }

            int regularWounded;
            int chiefWounded;
            int bossWounded;
            SplitWounded(plannedWounded, plannedTotal, regularCount, chiefCount, bossCount, out regularWounded, out chiefWounded, out bossWounded);

            List<RosterDelta> appliedRemovals = new List<RosterDelta>();
            List<RosterDelta> appliedReplacements = new List<RosterDelta>();
            try
            {
                for (int i = 0; i < removals.Count; i++)
                {
                    RosterDelta plan = removals[i];
                    roster.AddToCounts(plan.Character, -plan.Count, false, -plan.Wounded, 0, true, -1);
                    appliedRemovals.Add(plan);
                }

                if (!AddReplacementShare(roster, pool, regularCount, regularWounded, appliedReplacements)
                    || !AddReplacementShare(roster, SingleTroopPool(chiefTroop), chiefCount, chiefWounded, appliedReplacements)
                    || !AddReplacementShare(roster, SingleTroopPool(bossTroop), bossCount, bossWounded, appliedReplacements))
                {
                    RollbackConversion(roster, appliedRemovals, appliedReplacements);
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                RollbackConversion(roster, appliedRemovals, appliedReplacements);
                return false;
            }
        }

        private static List<CharacterObject> SingleTroopPool(CharacterObject troop)
        {
            List<CharacterObject> pool = new List<CharacterObject>();
            if (troop != null)
            {
                pool.Add(troop);
            }

            return pool;
        }

        private static bool AddReplacementShare(
            TroopRoster roster,
            List<CharacterObject> pool,
            int count,
            int wounded,
            List<RosterDelta> appliedReplacements)
        {
            if (count <= 0)
            {
                return true;
            }

            return TryAddReplacementTroops(roster, pool, count, wounded, appliedReplacements);
        }

        private static void SplitWounded(
            int totalWounded,
            int totalCount,
            int regularCount,
            int chiefCount,
            int bossCount,
            out int regularWounded,
            out int chiefWounded,
            out int bossWounded)
        {
            regularWounded = 0;
            chiefWounded = 0;
            bossWounded = 0;
            if (totalWounded <= 0 || totalCount <= 0)
            {
                return;
            }

            if (totalWounded > totalCount)
            {
                totalWounded = totalCount;
            }

            regularWounded = ShareOf(totalWounded, regularCount, totalCount);
            chiefWounded = ShareOf(totalWounded, chiefCount, totalCount);
            bossWounded = totalWounded - regularWounded - chiefWounded;
            if (regularWounded > regularCount)
            {
                regularWounded = regularCount;
            }
            if (chiefWounded > chiefCount)
            {
                chiefWounded = chiefCount;
            }
            if (bossWounded > bossCount)
            {
                bossWounded = bossCount;
            }
            if (bossWounded < 0)
            {
                bossWounded = 0;
            }

            int missing = totalWounded - (regularWounded + chiefWounded + bossWounded);
            missing = GiveWounded(ref regularWounded, regularCount, missing);
            missing = GiveWounded(ref chiefWounded, chiefCount, missing);
            GiveWounded(ref bossWounded, bossCount, missing);
        }

        private static int ShareOf(int total, int part, int whole)
        {
            if (total <= 0 || part <= 0 || whole <= 0)
            {
                return 0;
            }

            return (int)Math.Round(total * ((float)part / (float)whole));
        }

        private static int GiveWounded(ref int wounded, int cap, int missing)
        {
            if (missing <= 0)
            {
                return missing;
            }

            int room = cap - wounded;
            if (room < 0)
            {
                room = 0;
            }

            int add = missing < room ? missing : room;
            wounded += add;
            return missing - add;
        }

        private static bool TryResolveChiefAndBoss(out CharacterObject chief, out CharacterObject boss)
        {
            chief = null;
            boss = null;
            List<CharacterObject> resolved = new List<CharacterObject>();
            if (!TryAppendAllTroops(resolved, BossPartyExtraTroopIds) || resolved.Count < 2)
            {
                return false;
            }

            chief = resolved[0];
            boss = resolved[1];
            return true;
        }

        private static void RollbackConversion(
            TroopRoster roster,
            List<RosterDelta> appliedRemovals,
            List<RosterDelta> appliedReplacements)
        {
            if (roster == null)
            {
                return;
            }

            // Undo partial replacements first so the roster is not left with extras.
            if (appliedReplacements != null)
            {
                for (int i = appliedReplacements.Count - 1; i >= 0; i--)
                {
                    RosterDelta add = appliedReplacements[i];
                    try
                    {
                        roster.AddToCounts(add.Character, -add.Count, false, -add.Wounded, 0, true, -1);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            if (appliedRemovals != null)
            {
                for (int i = 0; i < appliedRemovals.Count; i++)
                {
                    RosterDelta removed = appliedRemovals[i];
                    try
                    {
                        roster.AddToCounts(removed.Character, removed.Count, false, removed.Wounded, 0, true, -1);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private static bool TryAddReplacementTroops(
            TroopRoster roster,
            List<CharacterObject> pool,
            int total,
            int wounded,
            List<RosterDelta> appliedReplacements)
        {
            if (pool == null || pool.Count == 0 || total <= 0 || appliedReplacements == null)
            {
                return false;
            }

            int woundedLeft = wounded;
            if (woundedLeft > total)
            {
                woundedLeft = total;
            }

            int remaining = total;
            while (remaining > 0)
            {
                CharacterObject troop = pool[MBRandom.RandomInt(pool.Count)];
                if (troop == null)
                {
                    return false;
                }

                int chunk = 1;
                if (remaining > 1)
                {
                    int maxChunk = remaining < 5 ? remaining : 5;
                    chunk = MBRandom.RandomInt(1, maxChunk + 1);
                }

                int chunkWounded = 0;
                if (woundedLeft > 0)
                {
                    int remainingAfter = remaining - chunk;
                    int woundedMustReserve = remainingAfter < woundedLeft ? woundedLeft - remainingAfter : 0;
                    chunkWounded = woundedLeft - woundedMustReserve;
                    if (chunkWounded < 0)
                    {
                        chunkWounded = 0;
                    }
                    if (chunkWounded > chunk)
                    {
                        chunkWounded = chunk;
                    }
                    if (chunkWounded > woundedLeft)
                    {
                        chunkWounded = woundedLeft;
                    }
                }

                roster.AddToCounts(troop, chunk, false, chunkWounded, 0, true, -1);

                RosterDelta added = new RosterDelta();
                added.Character = troop;
                added.Count = chunk;
                added.Wounded = chunkWounded;
                appliedReplacements.Add(added);

                remaining -= chunk;
                woundedLeft -= chunkWounded;
            }

            return true;
        }

        /// <summary>
        /// Requires both Mounted Pillager and Mounted Ransacker. A partial pool is rejected
        /// so conversion never starts with an incomplete replacement set.
        /// </summary>
        private static bool TryBuildPillagerPool(out List<CharacterObject> pool)
        {
            pool = new List<CharacterObject>();
            if (!TryAppendAllTroops(pool, ConversionTroopIds))
            {
                pool = null;
                return false;
            }

            return true;
        }

        private static bool TryAppendAllTroops(List<CharacterObject> pool, string[] ids)
        {
            for (int i = 0; i < ids.Length; i++)
            {
                CharacterObject troop = MBObjectManager.Instance.GetObject<CharacterObject>(ids[i]);
                if (troop == null || troop.IsHero)
                {
                    return false;
                }

                pool.Add(troop);
            }

            return true;
        }

        private struct RosterDelta
        {
            public CharacterObject Character;
            public int Count;
            public int Wounded;
        }
    }
}
