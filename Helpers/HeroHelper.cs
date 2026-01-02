using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace Brut.FamilyTree.Helpers
{
    public static class HeroHelper
    {
        /// <summary>
        /// Finds the oldest ancestor of a hero following clan hierarchy.
        /// Prioritizes: Kingdom leader -> Ruling clan -> Kingdom clan leader -> Kingdom clan -> Minor faction leader -> Clan leader -> Any parent
        /// </summary>
        public static Hero FindAncestorOf(Hero hero)
        {
            List<Hero> parents = new List<Hero>();

            if (hero.Father?.Clan != null)
            {
                parents.Add(hero.Father);
            }

            if (hero.Mother?.Clan != null)
            {
                parents.Add(hero.Mother);
            }

            // Kingdom Ruling Clan Leader
            foreach (var parent in parents.Where(parent => parent.Clan.Kingdom?.Leader == parent))
            {
                return FindAncestorOf(parent);
            }
            // Kingdom Ruling Clan
            foreach (var parent in parents.Where(parent => parent.Clan.Kingdom?.RulingClan == parent.Clan))
            {
                return FindAncestorOf(parent);
            }

            // Kingdom Clan Leader
            foreach (var parent in parents.Where(parent => parent.MapFaction.IsKingdomFaction && parent.IsFactionLeader))
            {
                return FindAncestorOf(parent);
            }
            // Kingdom Clan
            foreach (var parent in parents.Where(parent => parent.MapFaction.IsKingdomFaction))
            {
                return FindAncestorOf(parent);
            }

            // Minor Faction Leader
            foreach (var parent in parents.Where(parent => parent.Clan.IsMinorFaction && parent.IsFactionLeader))
            {
                return FindAncestorOf(parent);
            }
            // Minor Faction Clan
            foreach (var parent in parents.Where(parent => parent.Clan.IsMinorFaction))
            {
                return FindAncestorOf(parent);
            }

            // Clan Leader
            foreach (var parent in parents.Where(parent => parent.Clan.Leader == parent))
            {
                return FindAncestorOf(parent);
            }

            // Any parent
            foreach (var parent in parents)
            {
                return FindAncestorOf(parent);
            }

            return hero;
        }
    }
}
