using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace HorseBandits
{
    public class HorseBanditsSubModule : MBSubModuleBase
    {
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            CampaignGameStarter starter = gameStarterObject as CampaignGameStarter;
            if (game.GameType is Campaign && starter != null)
            {
                starter.AddBehavior(new HorseBanditsLootBehavior());
                starter.AddBehavior(new HorseBanditsConversionBehavior());
            }
        }
    }
}
