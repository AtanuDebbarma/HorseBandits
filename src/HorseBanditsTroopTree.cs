using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace HorseBandits
{
    /// <summary>
    /// The Highwayman to Vlandian Brigand Champion link is written by XSLT, which cannot
    /// see MCM. This reapplies or restores that one link when a campaign starts and when
    /// the switch changes. Hillman and Brigand keep their vanilla upgrades.
    /// </summary>
    internal static class HorseBanditsTroopTree
    {
        private const string HighwaymanId = "mountain_bandits_chief";
        private const string BrigandChampionId = "horse_bandits_vlandian_brigand_champion";
        private const string VanillaChampionId = "vlandian_champion";

        // Public getter, private setter. Calling the setter keeps this tied to the property
        // name instead of the compiler's backing-field name.
        private static readonly MethodInfo UpgradeTargetsSetter = FindUpgradeTargetsSetter();

        private static bool _reportedMissingSetter;

        internal static void ApplyHighwaymanBranch(bool enabled)
        {
            try
            {
                Apply(enabled);
            }
            catch (Exception)
            {
                // The options menu can open before troops exist. Session launch tries again.
            }
        }

        private static void Apply(bool enabled)
        {
            MBObjectManager manager = MBObjectManager.Instance;
            if (manager == null)
            {
                return;
            }

            CharacterObject highwayman = manager.GetObject<CharacterObject>(HighwaymanId);
            CharacterObject desired = manager.GetObject<CharacterObject>(enabled ? BrigandChampionId : VanillaChampionId);
            CharacterObject remove = manager.GetObject<CharacterObject>(enabled ? VanillaChampionId : BrigandChampionId);
            if (highwayman == null || desired == null)
            {
                return;
            }

            if (UpgradeTargetsSetter == null)
            {
                if (!_reportedMissingSetter)
                {
                    _reportedMissingSetter = true;
                    Debug.Print("[Horse Bandits] Could not change the Highwayman upgrade. The base game setter was not found.");
                }

                return;
            }

            CharacterObject[] next = Retarget(highwayman.UpgradeTargets, remove, desired);
            if (highwayman.IsBasicTroop == enabled && SameTargets(highwayman.UpgradeTargets, next))
            {
                return;
            }

            highwayman.IsBasicTroop = enabled;
            UpgradeTargetsSetter.Invoke(highwayman, new object[] { next });
        }

        private static MethodInfo FindUpgradeTargetsSetter()
        {
            PropertyInfo property = typeof(CharacterObject).GetProperty(
                "UpgradeTargets",
                BindingFlags.Instance | BindingFlags.Public);
            if (property == null)
            {
                return null;
            }

            return property.GetSetMethod(true);
        }

        private static bool SameTargets(CharacterObject[] current, CharacterObject[] next)
        {
            if (current == null || next == null || current.Length != next.Length)
            {
                return false;
            }

            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] != next[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static CharacterObject[] Retarget(CharacterObject[] current, CharacterObject remove, CharacterObject add)
        {
            List<CharacterObject> kept = new List<CharacterObject>();
            if (current != null)
            {
                for (int i = 0; i < current.Length; i++)
                {
                    CharacterObject troop = current[i];
                    if (troop == null || troop == remove || troop == add)
                    {
                        continue;
                    }

                    kept.Add(troop);
                }
            }

            kept.Add(add);
            return kept.ToArray();
        }
    }
}
