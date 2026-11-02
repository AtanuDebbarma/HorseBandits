# Horse Bandits

A Mount & Blade II: Bannerlord mod that places Mounted Pillagers and Mounted Ransackers into existing bandit parties in Empire, Vlandia, and Sturgia. It does not add hideouts or change the map.

**Source:** [Nexus Mods URL](https://www.nexusmods.com/mountandblade2bannerlord/mods/14448)

## Why this mod exists

Mounted Pillager and Mounted Ransacker exist in the base game, but only as troops from the caravan ambush quest. Bandit parties on the map never field them. This mod puts a share of those troops into bandit parties that are already there, and leaves the parties' clans, cultures, and homes alone.

## How it works

Each day, eligible bandit parties whose nearest town is Empire, Vlandia, or Sturgia may be picked:

| Rule | Value |
|---|---|
| Which parties | Existing bandit parties, including hideout defenders. Not quest, lord, caravan, villager, militia, garrison, or the player |
| Where | Nearest town to the party's current position is Empire, Vlandia, or Sturgia |
| How often | About 35 percent of eligible parties, once. A party that is not picked is not rolled again |
| How many troops | The clan's slider. Default 35 percent of its regulars. From 1 to 80. The rest stay vanilla |
| At sea | Parties at sea, and parties that cannot travel on land, are never changed |

A party the mod has already changed keeps that roster. Turning a switch off, or moving a slider, affects only parties picked after the change.

## Mod options ([MCM][mcm])

All settings are under **Options > Mod Options > Horse Bandits**.

**Mounted Pillagers and Ransackers** is the master switch. It starts on. Off stops new placements. Under it, each bandit clan has a switch. The slider for that clan appears only while the switch is on.

| Clan | Starts |
|---|---|
| Looters, Deserters, Mountain Bandits, Forest Bandits, Desert Bandits | On, 35 percent |
| Sea Raiders, Steppe Bandits | Off, slider at 35 when turned on |
| **War Sails** | On. Shows the two clans below. Off leaves them vanilla, and does nothing if War Sails is not installed |
| Northern Sea Raiders, Corsairs | Off, slider at 35 when turned on |

**Horse Bandit Chief and Boss** starts on. One switch covers both, and only boss parties receive them. The slider, default 50, is the share of the replaced troops that are Chief or Boss. The rest of that share are Mounted Pillagers and Mounted Ransackers.

**Highwayman upgrades to Brigand Champion** starts on. Highwayman upgrades to Vlandian Brigand Champion, then Vlandian Brigand Banner Knight. Off restores the base game upgrade, Vlandian Champion. Reopen the encyclopedia page after changing it. Hillmen and Brigands are unchanged.

## Troop trees

```
Mounted Pillager
└── Mounted Huntsman
    └── Mounted Packmaster

Mounted Ransacker
└── Imperial Bandit Horseman
    └── Imperial Elite Bandit Horseman

Highwayman
└── Vlandian Brigand Champion          while the switch is on
    └── Vlandian Brigand Banner Knight

Horse Bandit Chief
└── Horse Bandit Boss                  boss parties only
```

The Pillager and Ransacker upgrades need Veteran's Respect, as a Bandit upgrading to a Soldier does in the base game. Those soldier troops are not placed into parties. Chief upgrades to Boss without Veteran's Respect.

Bandit Javelins drop and sell as themselves. A Large Bag of Simple Javelins becomes a Simple Javelin when looted, and Stacked Piercing Arrows become two piercing quivers.

## Requirements and versions

| Dependency | Tested version | Expected to work with |
|---|---|---|
| Mount & Blade II: Bannerlord | 1.4.8 | 1.4.x |
| War Sails (NavalDLC), optional | 1.2.8 | 1.2.x |
| [Harmony][harmony] | 2.4.2 | 2.x |
| [ButterLib][butterlib] | 2.12.0 | 2.x |
| [UIExtenderEx][uiextenderex] | 3.0.3 | 3.x, as required by MCM 5.13 |
| [Mod Configuration Menu v5 (MCM)][mcm] | 5.13.1 | 5.x |

This mod needs [MCM][mcm] directly. [MCM][mcm] in turn needs [Harmony][harmony], [ButterLib][butterlib] and [UIExtenderEx][uiextenderex]. War Sails is not required. Without it the game still loads, and the War Sails clan switches match no parties.

## Compatibility

- **Vanilla and War Sails.** The mod edits rosters of existing bandit parties. It does not replace bandit clans, hideouts, or troop trees except the Highwayman upgrade, which can be switched off.
- **Other mods that also edit bandit rosters** can overlap with this one. Both can change the same party.
- **Other mods that add their own bandit clans** are left on the default 35 percent replacement when they roam into Empire, Vlandia, or Sturgia, unless their culture is Sea Raiders, Steppe Bandits, or Corsairs.

## Translations

The mod ships in English. All of its text, including every [MCM][mcm] setting and hint, is in `ModuleData/Languages/EN/hb_strings_en.xml`. Anyone may publish a translation as a separate mod that requires Horse Bandits, without changing its files. [TRANSLATORS.md](TRANSLATORS.md) explains how.

## Installation

1. Install [Harmony][harmony], [ButterLib][butterlib], [UIExtenderEx][uiextenderex] and [MCM][mcm].
2. Extract the `HorseBandits` folder into `Mount & Blade II Bannerlord\Modules`.
3. Start the game launcher. Enable Horse Bandits and place it after [MCM][mcm].

The mod can be added to an existing campaign.

## Removing the mod from a campaign

Removing Horse Bandits from a save that already contains its troops or items is unsafe. Those characters and items would be missing. Add it to a campaign you plan to keep playing with it, or remove it before those troops and items enter the save.

## Building from source

The repository contains only source code. The TaleWorlds and [MCM][mcm] DLLs are not included; the build references them from your game installation.

Requirements:

- Windows with .NET Framework 4.7.2 or later.
- Visual Studio 2019 or later, or the Visual Studio Build Tools. The .NET Framework's own MSBuild also works.
- Bannerlord with [MCM][mcm] installed.

The code is limited to C# 5 so it builds with the .NET Framework compiler too.

### Point the build at your game

Use one of the following:

- Copy `HorseBandits.user.props.example` to `HorseBandits.user.props` and set `BannerlordDir`. Git ignores this file.
- Set the `BANNERLORD_DIR` environment variable.
- Pass `/p:BannerlordDir="..."` to MSBuild.

If none is set, the default Steam folder `C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord` is used.

### Build

```bat
build.cmd
```

The DLL is written to `bin\Win64_Shipping_Client\HorseBandits.dll`, so the repository folder has the same layout as the installed module.

To also copy `SubModule.xml`, `ModuleData` and the DLL into the game's `Modules\HorseBandits` folder after each build, set `DeployToGame` to `true` in your `HorseBandits.user.props`, or run:

```bat
build.cmd /p:DeployToGame=true
```

Close the game first, otherwise the DLL is locked.

## Repository layout

```
HorseBandits.csproj               Project file
HorseBandits.user.props.example   Template for local build settings
build.cmd                         Finds MSBuild and builds the project
SubModule.xml                     Module definition loaded by the game
ModuleData/                       Troops, items, cultures, and English text
src/                              C# source
TRANSLATORS.md                    How to make a translation mod
```

## License and credit

Copyright (C) 2026 AtanuDebbarma.

Horse Bandits is free software under the [GNU General Public License v3.0](LICENSE), with an additional attribution term described in [NOTICE](NOTICE):

- **Using or modifying this code:** if you publish a mod or other work that uses or is based on this code, you must credit **Horse Bandits by AtanuDebbarma** with a link to [this repository][repo], in your source code and in your mod's documentation. Your work must also be released under the GPL v3.
- **Using it as inspiration:** if this project inspired your work without you copying its code, a credit and link are kindly requested.

Mount & Blade II: Bannerlord is a trademark of TaleWorlds Entertainment. This project is not affiliated with or endorsed by TaleWorlds.

[mcm]: https://www.nexusmods.com/mountandblade2bannerlord/mods/612
[butterlib]: https://www.nexusmods.com/mountandblade2bannerlord/mods/2018?tab=description
[harmony]: https://www.nexusmods.com/mountandblade2bannerlord/mods/2006?tab=description
[uiextenderex]: https://www.nexusmods.com/mountandblade2bannerlord/mods/2102?tab=description
[repo]: https://github.com/AtanuDebbarma/HorseBandits
