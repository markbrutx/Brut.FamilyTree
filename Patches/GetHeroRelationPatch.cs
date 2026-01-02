using Brut.FamilyTree.Helpers;
using HarmonyLib;
using SandBox.GauntletUI;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;

namespace Brut.FamilyTree.Patches
{
    /// <summary>
    /// Patches ConversationHelper.GetHeroRelationToHeroTextShort to show full lineage chains.
    /// Instead of just "Grandson", shows "Son of Biba and Boba, Grandson of Blink"
    /// </summary>
    [HarmonyPatch(typeof(ConversationHelper), "GetHeroRelationToHeroTextShort")]
    public static class GetHeroRelationPatch
    {
        /// <summary>
        /// Prefix patch that replaces the original method's behavior.
        /// Returns false to skip the original method.
        /// Only active when Clan Screen is open and Encyclopedia is NOT open.
        /// </summary>
        public static bool Prefix(Hero queriedHero, Hero baseHero, bool uppercaseFirst, ref string __result)
        {
            // Only apply our logic when Clan Screen is the active top screen
            if (ScreenManager.TopScreen is not GauntletClanScreen)
            {
                return true; // Run original - not in Clan Screen
            }

            // Don't apply if Encyclopedia is open (it opens as overlay on top of Clan Screen)
            try
            {
                var mapScreen = MapScreen.Instance;
                if (mapScreen?.EncyclopediaScreenManager?.IsEncyclopediaOpen == true)
                {
                    return true; // Run original - Encyclopedia is open
                }
            }
            catch
            {
                // Ignore if MapScreen not available
            }

            // Skip if either hero is null
            if (queriedHero == null || baseHero == null)
            {
                return true; // Run original
            }

            // Skip if same hero
            if (queriedHero == baseHero)
            {
                __result = "";
                return false;
            }

            try
            {
                // Get our enhanced relation chain
                var enhancedRelation = FamilyHelper.GetFullRelationChain(queriedHero, baseHero);

                // If we got a valid result, use it
                if (enhancedRelation != null && !string.IsNullOrEmpty(enhancedRelation.ToString()))
                {
                    __result = enhancedRelation.ToString();
                    return false; // Skip original
                }
            }
            catch
            {
                // On any error, fall back to original
            }

            // Fall back to original behavior
            return true;
        }
    }
}
