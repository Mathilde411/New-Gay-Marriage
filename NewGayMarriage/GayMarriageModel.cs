using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace NewGayMarriage
{
    public class GayMarriageModel : MarriageModel
    {
        private const float BaseMarriageChanceForNpcs = 0.002f;

        private Random rdm = new Random();

        public override int MinimumMarriageAgeMale => 18;

        public override int MinimumMarriageAgeFemale => 18;

        public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero)
        {
            if (!IsClanSuitableForMarriage(firstHero.Clan) || !IsClanSuitableForMarriage(secondHero.Clan) ||
                firstHero.Clan?.Leader == firstHero && secondHero.Clan?.Leader == secondHero ||
                AreHeroesRelated(firstHero, secondHero, 3))
                return false;
            Hero courtedHeroInOtherClan1 = Romance.GetCourtedHeroInOtherClan(firstHero, secondHero);
            if (courtedHeroInOtherClan1 != null && courtedHeroInOtherClan1 != secondHero)
                return false;
            Hero courtedHeroInOtherClan2 = Romance.GetCourtedHeroInOtherClan(secondHero, firstHero);
            return (courtedHeroInOtherClan2 == null || courtedHeroInOtherClan2 == firstHero) && firstHero.CanMarry() &&
                   secondHero.CanMarry();
        }

        public override bool IsClanSuitableForMarriage(Clan clan) =>
            clan != null && !clan.IsBanditFaction && !clan.IsRebelClan;

        public override float NpcCoupleMarriageChance(Hero firstHero, Hero secondHero)
        {
            if (!IsCoupleSuitableForMarriage(firstHero, secondHero))
                return 0.0f;
            float num1 = 1f / 500f * (float)(1.0 + ((double)firstHero.Age - 18.0) / 50.0) *
                         (float)(1.0 + (secondHero.Age - 18.0) / 50.0) *
                         (float)(1.0 - MathF.Abs(secondHero.Age - firstHero.Age) / 50.0);
            if (firstHero.Clan.Kingdom != secondHero.Clan.Kingdom)
                num1 *= 0.5f;
            float num2 = (float)(0.5 + firstHero.Clan.GetRelationWithClan(secondHero.Clan) / 200.0);
            return num1 * num2;
        }

        public override bool ShouldNpcMarriageBetweenClansBeAllowed(
            Clan consideringClan,
            Clan targetClan)
        {
            return targetClan != consideringClan && !consideringClan.IsAtWarWith(targetClan) &&
                   consideringClan.GetRelationWithClan(targetClan) >= -50;
        }

        public override List<Hero> GetAdultChildrenSuitableForMarriage(Hero hero)
        {
            List<Hero> suitableForMarriage = new List<Hero>();
            foreach (Hero child in hero.Children)
            {
                if (child.CanMarry())
                    suitableForMarriage.Add(child);
            }

            return suitableForMarriage;
        }

        private bool AreHeroesRelatedAux1(Hero firstHero, Hero secondHero, int ancestorDepth)
        {
            if (firstHero == secondHero)
                return true;
            if (ancestorDepth <= 0)
                return false;
            if (secondHero.Mother != null && AreHeroesRelatedAux1(firstHero, secondHero.Mother, ancestorDepth - 1))
                return true;
            return secondHero.Father != null &&
                   AreHeroesRelatedAux1(firstHero, secondHero.Father, ancestorDepth - 1);
        }

        private bool AreHeroesRelatedAux2(
            Hero firstHero,
            Hero secondHero,
            int ancestorDepth,
            int secondAncestorDepth)
        {
            if (AreHeroesRelatedAux1(firstHero, secondHero, secondAncestorDepth))
                return true;
            if (ancestorDepth <= 0)
                return false;
            if (firstHero.Mother != null &&
                AreHeroesRelatedAux2(firstHero.Mother, secondHero, ancestorDepth - 1, secondAncestorDepth))
                return true;
            return firstHero.Father != null &&
                   AreHeroesRelatedAux2(firstHero.Father, secondHero, ancestorDepth - 1, secondAncestorDepth);
        }

        private bool AreHeroesRelated(Hero firstHero, Hero secondHero, int ancestorDepth) =>
            AreHeroesRelatedAux2(firstHero, secondHero, ancestorDepth, ancestorDepth);

        public override int GetEffectiveRelationIncrease(Hero firstHero, Hero secondHero)
        {
            ExplainedNumber stat = new ExplainedNumber(20f);
            //TODO: Understand why this is here
            if(firstHero.IsFemale != secondHero.IsFemale)
                SkillHelper.AddSkillBonusForCharacter(DefaultSkills.Charm, DefaultSkillEffects.CharmRelationBonus,
                    firstHero.IsFemale ? secondHero.CharacterObject : firstHero.CharacterObject, ref stat);
            return MathF.Round(stat.ResultNumber);
        }

        public override bool IsSuitableForMarriage(Hero maidenOrSuitor)
        {
            if (!maidenOrSuitor.IsActive || maidenOrSuitor.Spouse != null || !maidenOrSuitor.IsLord ||
                maidenOrSuitor.IsMinorFactionHero || maidenOrSuitor.IsNotable || maidenOrSuitor.IsTemplate ||
                maidenOrSuitor.PartyBelongedTo?.MapEvent != null || maidenOrSuitor.PartyBelongedTo?.Army != null)
                return false;
            return maidenOrSuitor.IsFemale
                ? maidenOrSuitor.CharacterObject.Age >= MinimumMarriageAgeFemale
                : maidenOrSuitor.CharacterObject.Age >= MinimumMarriageAgeMale;
        }

        public override Clan GetClanAfterMarriage(Hero firstHero, Hero secondHero)
        {
            if (firstHero.IsHumanPlayerCharacter)
                return firstHero.Clan;
            if(secondHero.IsHumanPlayerCharacter)
                return secondHero.Clan;
            
            if (firstHero == firstHero.Clan.Leader)
                return firstHero.Clan;
            if(secondHero == secondHero.Clan.Leader)
                return secondHero.Clan;
            
            //TODO: Option for no gender difference
            if (!firstHero.IsFemale && secondHero.IsFemale)
                return firstHero.Clan;
            if (firstHero.IsFemale && !secondHero.IsFemale)
                return secondHero.Clan;

            if (rdm.NextDouble() > 0.5d)
                return firstHero.Clan;
            return secondHero.Clan;

        }
    }
}