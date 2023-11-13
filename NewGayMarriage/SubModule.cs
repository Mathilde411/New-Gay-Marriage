using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;

namespace NewGayMarriage
{
    using TaleWorlds.MountAndBlade;

    public class SubModule : MBSubModuleBase
    {

        private static void ReplaceModel<T>(IGameStarter gameStarterObject, Func<T> modelGetter) where T : GameModel
        {
            bool removedModel = false;
            
            foreach (var oldModel in gameStarterObject.Models)
            {
                if (oldModel is T)
                {
                    ((List<GameModel>)gameStarterObject.Models).Remove(oldModel);
                    removedModel = true;
                    break;
                }
            }
            
            if(removedModel)
                gameStarterObject.AddModel(modelGetter());
        }
        
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            ReplaceModel<MarriageModel>(gameStarterObject, () => new GayMarriageModel());
        }
    }
}