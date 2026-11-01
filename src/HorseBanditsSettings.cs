using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using TaleWorlds.Localization;

namespace HorseBandits
{
    public sealed class HorseBanditsSettings : AttributeGlobalSettings<HorseBanditsSettings>
    {
        internal const int DefaultRosterPercent = 35;
        internal const int DefaultChiefPercent = 50;
        internal const int MinPercent = 1;
        internal const int MaxPercent = 80;

        private const string GroupSpawn = "{=hb_mcm_group_spawn}Mounted Pillagers and Ransackers";
        private const string GroupLooters = GroupSpawn + "/{=hb_mcm_looters_name}Looters";
        private const string GroupDeserters = GroupSpawn + "/{=hb_mcm_deserters_name}Deserters";
        private const string GroupMountain = GroupSpawn + "/{=hb_mcm_mountain_name}Mountain Bandits";
        private const string GroupForest = GroupSpawn + "/{=hb_mcm_forest_name}Forest Bandits";
        private const string GroupDesert = GroupSpawn + "/{=hb_mcm_desert_name}Desert Bandits";
        private const string GroupSea = GroupSpawn + "/{=hb_mcm_sea_name}Sea Raiders";
        private const string GroupSteppe = GroupSpawn + "/{=hb_mcm_steppe_name}Steppe Bandits";
        private const string GroupWarSails = GroupSpawn + "/{=hb_mcm_group_warsails}War Sails";
        private const string GroupNorthern = GroupWarSails + "/{=hb_mcm_northern_name}Northern Sea Raiders";
        private const string GroupCorsairs = GroupWarSails + "/{=hb_mcm_corsairs_name}Corsairs";
        private const string GroupChief = GroupSpawn + "/{=hb_mcm_group_chief}Horse Bandit Chief and Boss";
        private const string GroupHighwayman = "{=hb_mcm_group_highwayman}Highwayman";

        private const string PercentName = "{=hb_mcm_percent_name}Replaced troops percent";
        private const string PercentHint = "{=hb_mcm_percent_hint}Percent of that clan's regular troops replaced with Mounted Pillagers and Mounted Ransackers. Default 35, which is what the mod does now. From 1 to 80. The rest of the party stays vanilla.";
        private const string OffHint = "{=hb_mcm_off_hint}Off by default, as the mod is now. Land parties of this clan in Empire, Vlandia, and Sturgia can be partly replaced. Parties at sea, and parties that cannot travel on land, stay vanilla.";
        private const string OnHint = "{=hb_mcm_on_hint}Land parties of this clan in Empire, Vlandia, and Sturgia can be partly replaced. Parties at sea, and parties that cannot travel on land, stay vanilla.";

        public override string Id { get { return "HorseBandits"; } }
        public override string DisplayName { get { return new TextObject("{=hb_mcm_title}Horse Bandits").ToString(); } }
        public override string FolderName { get { return "HorseBandits"; } }
        public override string FormatType { get { return "json2"; } }

        public HorseBanditsSettings()
        {
            SpawnMountedPillagers = true;
            Looters = true;
            LootersPercent = DefaultRosterPercent;
            Deserters = true;
            DesertersPercent = DefaultRosterPercent;
            MountainBandits = true;
            MountainBanditsPercent = DefaultRosterPercent;
            ForestBandits = true;
            ForestBanditsPercent = DefaultRosterPercent;
            DesertBandits = true;
            DesertBanditsPercent = DefaultRosterPercent;
            SeaRaiders = false;
            SeaRaidersPercent = DefaultRosterPercent;
            SteppeBandits = false;
            SteppeBanditsPercent = DefaultRosterPercent;
            WarSailsSupport = true;
            NorthernSeaRaiders = false;
            NorthernSeaRaidersPercent = DefaultRosterPercent;
            Corsairs = false;
            CorsairsPercent = DefaultRosterPercent;
            ChiefAndBoss = true;
            ChiefAndBossPercent = DefaultChiefPercent;
            HighwaymanUpgrade = true;
        }

        internal static void ApplyHighwaymanBranch()
        {
            HorseBanditsSettings settings = Instance;
            bool enabled = settings == null || settings.HighwaymanUpgrade;
            HorseBanditsTroopTree.ApplyHighwaymanBranch(enabled);
        }

        internal static bool AllowsParty(string clanId, string cultureId, out int percent)
        {
            percent = DefaultRosterPercent;
            FactionRules rules = CurrentRules();
            if (!rules.Spawn)
            {
                return false;
            }

            if (IsListedClan(clanId))
            {
                return IsEnabled(rules, clanId, out percent);
            }

            // No clan id: the culture is the only name the party has.
            if (string.IsNullOrEmpty(clanId))
            {
                string mapped = ClanForCulture(cultureId);
                if (IsListedClan(mapped))
                {
                    return IsEnabled(rules, mapped, out percent);
                }
            }

            // Old behavior excluded these by culture even when the clan id was unexpected.
            // Southern pirates were not excluded; they are a listed clan and default off.
            if (cultureId == "sea_raiders" || cultureId == "steppe_bandits" || cultureId == "southern_pirates")
            {
                return IsEnabled(rules, ClanForCulture(cultureId), out percent);
            }

            percent = DefaultRosterPercent;
            return true;
        }

        internal static bool TryChiefShare(out int percent)
        {
            FactionRules rules = CurrentRules();
            percent = ClampPercent(rules.ChiefAndBossPercent);
            return rules.Spawn && rules.ChiefAndBoss;
        }

        [SettingPropertyBool("{=hb_mcm_group_spawn}Mounted Pillagers and Ransackers", IsToggle = true, Order = 0, RequireRestart = false, HintText = "{=hb_mcm_spawn_hint}On by default. Each day, about 35 percent of eligible bandit parties in Empire, Vlandia, and Sturgia are picked once. The sliders set how much of a picked party is replaced with Mounted Pillagers and Mounted Ransackers. Off stops new placements. Parties already changed are left as they are. Parties at sea are never changed.")]
        [SettingPropertyGroup(GroupSpawn, GroupOrder = 0)]
        public bool SpawnMountedPillagers { get; set; }

        [SettingPropertyBool("{=hb_mcm_looters_name}Looters", IsToggle = true, Order = 0, RequireRestart = false, HintText = OnHint)]
        [SettingPropertyGroup(GroupLooters, GroupOrder = 1)]
        public bool Looters { get; set; }

        [SettingPropertyInteger(PercentName, MinPercent, MaxPercent, "0", Order = 1, RequireRestart = false, HintText = PercentHint)]
        [SettingPropertyGroup(GroupLooters)]
        public int LootersPercent { get; set; }

        [SettingPropertyBool("{=hb_mcm_deserters_name}Deserters", IsToggle = true, Order = 0, RequireRestart = false, HintText = OnHint)]
        [SettingPropertyGroup(GroupDeserters, GroupOrder = 2)]
        public bool Deserters { get; set; }

        [SettingPropertyInteger(PercentName, MinPercent, MaxPercent, "0", Order = 1, RequireRestart = false, HintText = PercentHint)]
        [SettingPropertyGroup(GroupDeserters)]
        public int DesertersPercent { get; set; }

        [SettingPropertyBool("{=hb_mcm_mountain_name}Mountain Bandits", IsToggle = true, Order = 0, RequireRestart = false, HintText = OnHint)]
        [SettingPropertyGroup(GroupMountain, GroupOrder = 3)]
        public bool MountainBandits { get; set; }

        [SettingPropertyInteger(PercentName, MinPercent, MaxPercent, "0", Order = 1, RequireRestart = false, HintText = PercentHint)]
        [SettingPropertyGroup(GroupMountain)]
        public int MountainBanditsPercent { get; set; }

        [SettingPropertyBool("{=hb_mcm_forest_name}Forest Bandits", IsToggle = true, Order = 0, RequireRestart = false, HintText = OnHint)]
        [SettingPropertyGroup(GroupForest, GroupOrder = 4)]
        public bool ForestBandits { get; set; }

        [SettingPropertyInteger(PercentName, MinPercent, MaxPercent, "0", Order = 1, RequireRestart = false, HintText = PercentHint)]
        [SettingPropertyGroup(GroupForest)]
        public int ForestBanditsPercent { get; set; }

        [SettingPropertyBool("{=hb_mcm_desert_name}Desert Bandits", IsToggle = true, Order = 0, RequireRestart = false, HintText = OnHint)]
        [SettingPropertyGroup(GroupDesert, GroupOrder = 5)]
        public bool DesertBandits { get; set; }

        [SettingPropertyInteger(PercentName, MinPercent, MaxPercent, "0", Order = 1, RequireRestart = false, HintText = PercentHint)]
        [SettingPropertyGroup(GroupDesert)]
        public int DesertBanditsPercent { get; set; }

        [SettingPropertyBool("{=hb_mcm_sea_name}Sea Raiders", IsToggle = true, Order = 0, RequireRestart = false, HintText = OffHint)]
        [SettingPropertyGroup(GroupSea, GroupOrder = 6)]
        public bool SeaRaiders { get; set; }

        [SettingPropertyInteger(PercentName, MinPercent, MaxPercent, "0", Order = 1, RequireRestart = false, HintText = PercentHint)]
        [SettingPropertyGroup(GroupSea)]
        public int SeaRaidersPercent { get; set; }

        [SettingPropertyBool("{=hb_mcm_steppe_name}Steppe Bandits", IsToggle = true, Order = 0, RequireRestart = false, HintText = OffHint)]
        [SettingPropertyGroup(GroupSteppe, GroupOrder = 7)]
        public bool SteppeBandits { get; set; }

        [SettingPropertyInteger(PercentName, MinPercent, MaxPercent, "0", Order = 1, RequireRestart = false, HintText = PercentHint)]
        [SettingPropertyGroup(GroupSteppe)]
        public int SteppeBanditsPercent { get; set; }

        [SettingPropertyBool("{=hb_mcm_group_warsails}War Sails", IsToggle = true, Order = 0, RequireRestart = false, HintText = "{=hb_mcm_warsails_hint}On by default. Shows the War Sails bandit clans below. Both start off, matching the mod now. Off leaves those parties vanilla. Does nothing when War Sails is not installed.")]
        [SettingPropertyGroup(GroupWarSails, GroupOrder = 8)]
        public bool WarSailsSupport { get; set; }

        [SettingPropertyBool("{=hb_mcm_northern_name}Northern Sea Raiders", IsToggle = true, Order = 0, RequireRestart = false, HintText = "{=hb_mcm_northern_hint}Off by default, as the mod is now. War Sails Sea Raiders: Sea Raider troops, looter culture, and ships. Only their land parties in Empire, Vlandia, and Sturgia can be partly replaced. Parties at sea stay vanilla.")]
        [SettingPropertyGroup(GroupNorthern, GroupOrder = 0)]
        public bool NorthernSeaRaiders { get; set; }

        [SettingPropertyInteger(PercentName, MinPercent, MaxPercent, "0", Order = 1, RequireRestart = false, HintText = PercentHint)]
        [SettingPropertyGroup(GroupNorthern)]
        public int NorthernSeaRaidersPercent { get; set; }

        [SettingPropertyBool("{=hb_mcm_corsairs_name}Corsairs", IsToggle = true, Order = 0, RequireRestart = false, HintText = "{=hb_mcm_corsairs_hint}Off by default. Corsairs have their own troops and ships. Only their land parties in Empire, Vlandia, and Sturgia can be partly replaced. Parties at sea stay vanilla.")]
        [SettingPropertyGroup(GroupCorsairs, GroupOrder = 1)]
        public bool Corsairs { get; set; }

        [SettingPropertyInteger(PercentName, MinPercent, MaxPercent, "0", Order = 1, RequireRestart = false, HintText = PercentHint)]
        [SettingPropertyGroup(GroupCorsairs)]
        public int CorsairsPercent { get; set; }

        [SettingPropertyBool("{=hb_mcm_group_chief}Horse Bandit Chief and Boss", IsToggle = true, Order = 0, RequireRestart = false, HintText = "{=hb_mcm_chief_hint}On by default. Boss parties only. One switch covers both, because Boss is the upgrade of Chief. This percent of the replaced troops are Chief or Boss, split between them. Default 50, matching the current mix. From 1 to 80. The rest of the replaced troops are Mounted Pillagers and Mounted Ransackers.")]
        [SettingPropertyGroup(GroupChief, GroupOrder = 9)]
        public bool ChiefAndBoss { get; set; }

        [SettingPropertyInteger("{=hb_mcm_chief_percent_name}Chief and Boss percent", MinPercent, MaxPercent, "0", Order = 1, RequireRestart = false, HintText = "{=hb_mcm_chief_percent_hint}Percent of the replaced troops on a boss party that are Horse Bandit Chief or Horse Bandit Boss. Default 50. From 1 to 80.")]
        [SettingPropertyGroup(GroupChief)]
        public int ChiefAndBossPercent { get; set; }

        [SettingPropertyBool("{=hb_mcm_highwayman_name}Highwayman upgrades to Brigand Champion", IsToggle = true, Order = 0, RequireRestart = false, HintText = "{=hb_mcm_highwayman_hint}Upgrades to Vlandian Brigand Champion, then Banner Knight. Off restores Vlandian Champion. Reopen the encyclopedia page.")]
        [SettingPropertyGroup(GroupHighwayman, GroupOrder = 1)]
        public bool HighwaymanUpgrade
        {
            get { return _highwaymanUpgrade; }
            set
            {
                _highwaymanUpgrade = value;
                // MCM writes this property directly. It does not call OnPropertyChanged with the property name.
                HorseBanditsTroopTree.ApplyHighwaymanBranch(value);
            }
        }

        private bool _highwaymanUpgrade;

        private static FactionRules CurrentRules()
        {
            FactionRules rules = DefaultRules();
            HorseBanditsSettings settings = Instance;
            if (settings == null)
            {
                return rules;
            }

            rules.Spawn = settings.SpawnMountedPillagers;
            rules.Looters = settings.Looters;
            rules.LootersPercent = settings.LootersPercent;
            rules.Deserters = settings.Deserters;
            rules.DesertersPercent = settings.DesertersPercent;
            rules.MountainBandits = settings.MountainBandits;
            rules.MountainBanditsPercent = settings.MountainBanditsPercent;
            rules.ForestBandits = settings.ForestBandits;
            rules.ForestBanditsPercent = settings.ForestBanditsPercent;
            rules.DesertBandits = settings.DesertBandits;
            rules.DesertBanditsPercent = settings.DesertBanditsPercent;
            rules.SeaRaiders = settings.SeaRaiders;
            rules.SeaRaidersPercent = settings.SeaRaidersPercent;
            rules.SteppeBandits = settings.SteppeBandits;
            rules.SteppeBanditsPercent = settings.SteppeBanditsPercent;
            rules.WarSails = settings.WarSailsSupport;
            rules.NorthernSeaRaiders = settings.NorthernSeaRaiders;
            rules.NorthernSeaRaidersPercent = settings.NorthernSeaRaidersPercent;
            rules.Corsairs = settings.Corsairs;
            rules.CorsairsPercent = settings.CorsairsPercent;
            rules.ChiefAndBoss = settings.ChiefAndBoss;
            rules.ChiefAndBossPercent = settings.ChiefAndBossPercent;
            return rules;
        }

        private static FactionRules DefaultRules()
        {
            FactionRules rules = new FactionRules();
            rules.Spawn = true;
            rules.Looters = true;
            rules.LootersPercent = DefaultRosterPercent;
            rules.Deserters = true;
            rules.DesertersPercent = DefaultRosterPercent;
            rules.MountainBandits = true;
            rules.MountainBanditsPercent = DefaultRosterPercent;
            rules.ForestBandits = true;
            rules.ForestBanditsPercent = DefaultRosterPercent;
            rules.DesertBandits = true;
            rules.DesertBanditsPercent = DefaultRosterPercent;
            rules.SeaRaiders = false;
            rules.SeaRaidersPercent = DefaultRosterPercent;
            rules.SteppeBandits = false;
            rules.SteppeBanditsPercent = DefaultRosterPercent;
            rules.WarSails = true;
            rules.NorthernSeaRaiders = false;
            rules.NorthernSeaRaidersPercent = DefaultRosterPercent;
            rules.Corsairs = false;
            rules.CorsairsPercent = DefaultRosterPercent;
            rules.ChiefAndBoss = true;
            rules.ChiefAndBossPercent = DefaultChiefPercent;
            return rules;
        }

        private static bool IsListedClan(string clanId)
        {
            return clanId == "looters"
                || clanId == "deserters"
                || clanId == "mountain_bandits"
                || clanId == "forest_bandits"
                || clanId == "desert_bandits"
                || clanId == "sea_raiders"
                || clanId == "steppe_bandits"
                || clanId == "northern_pirates"
                || clanId == "southern_pirates";
        }

        private static string ClanForCulture(string cultureId)
        {
            if (cultureId == "looters")
            {
                return "looters";
            }
            if (cultureId == "sea_raiders")
            {
                return "sea_raiders";
            }
            if (cultureId == "mountain_bandits")
            {
                return "mountain_bandits";
            }
            if (cultureId == "forest_bandits")
            {
                return "forest_bandits";
            }
            if (cultureId == "desert_bandits")
            {
                return "desert_bandits";
            }
            if (cultureId == "steppe_bandits")
            {
                return "steppe_bandits";
            }
            if (cultureId == "southern_pirates")
            {
                return "southern_pirates";
            }

            return null;
        }

        private static bool IsEnabled(FactionRules rules, string clanId, out int percent)
        {
            percent = DefaultRosterPercent;
            if (clanId == "looters")
            {
                return Use(rules.Looters, rules.LootersPercent, true, out percent);
            }
            if (clanId == "deserters")
            {
                return Use(rules.Deserters, rules.DesertersPercent, true, out percent);
            }
            if (clanId == "mountain_bandits")
            {
                return Use(rules.MountainBandits, rules.MountainBanditsPercent, true, out percent);
            }
            if (clanId == "forest_bandits")
            {
                return Use(rules.ForestBandits, rules.ForestBanditsPercent, true, out percent);
            }
            if (clanId == "desert_bandits")
            {
                return Use(rules.DesertBandits, rules.DesertBanditsPercent, true, out percent);
            }
            if (clanId == "sea_raiders")
            {
                return Use(rules.SeaRaiders, rules.SeaRaidersPercent, true, out percent);
            }
            if (clanId == "steppe_bandits")
            {
                return Use(rules.SteppeBandits, rules.SteppeBanditsPercent, true, out percent);
            }
            if (clanId == "northern_pirates")
            {
                return Use(rules.NorthernSeaRaiders, rules.NorthernSeaRaidersPercent, rules.WarSails, out percent);
            }
            if (clanId == "southern_pirates")
            {
                return Use(rules.Corsairs, rules.CorsairsPercent, rules.WarSails, out percent);
            }

            return false;
        }

        private static bool Use(bool enabled, int rawPercent, bool gate, out int percent)
        {
            percent = ClampPercent(rawPercent);
            return gate && enabled;
        }

        private static int ClampPercent(int value)
        {
            if (value < MinPercent)
            {
                return MinPercent;
            }
            if (value > MaxPercent)
            {
                return MaxPercent;
            }

            return value;
        }

        private struct FactionRules
        {
            public bool Spawn;
            public bool Looters;
            public int LootersPercent;
            public bool Deserters;
            public int DesertersPercent;
            public bool MountainBandits;
            public int MountainBanditsPercent;
            public bool ForestBandits;
            public int ForestBanditsPercent;
            public bool DesertBandits;
            public int DesertBanditsPercent;
            public bool SeaRaiders;
            public int SeaRaidersPercent;
            public bool SteppeBandits;
            public int SteppeBanditsPercent;
            public bool WarSails;
            public bool NorthernSeaRaiders;
            public int NorthernSeaRaidersPercent;
            public bool Corsairs;
            public int CorsairsPercent;
            public bool ChiefAndBoss;
            public int ChiefAndBossPercent;
        }
    }
}
