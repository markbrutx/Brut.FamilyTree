using HarmonyLib;
using SandBox.GauntletUI;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.ScreenSystem;
using Brut.FamilyTree.ViewModels;

namespace Brut.FamilyTree.Patches
{
    [HarmonyPatch(typeof(ScreenBase))]
    public class ClanScreenPatch
    {
        internal static GauntletLayer? FamilyTreeLayer;
        internal static FamilyTreeVM? FamilyTreeVM;
        internal static GauntletMovieIdentifier? FamilyTreeMovie;

        [HarmonyPatch("AddLayer")]
        [HarmonyPostfix]
        public static void Postfix(ref ScreenBase __instance)
        {
            if (__instance is not GauntletClanScreen clanScreen)
                return;

            if (FamilyTreeLayer != null)
                return;

            FamilyTreeLayer = new GauntletLayer("FamilyTreeLayer", 100);
            FamilyTreeVM = new FamilyTreeVM(clanScreen);
            FamilyTreeMovie = FamilyTreeLayer.LoadMovie("FamilyTreeOverlay", FamilyTreeVM);
            FamilyTreeLayer.InputRestrictions.SetInputRestrictions();
            clanScreen.AddLayer(FamilyTreeLayer);
        }

        [HarmonyPatch("RemoveLayer")]
        [HarmonyPostfix]
        public static void PostfixRemove(ref ScreenBase __instance)
        {
            if (__instance is GauntletClanScreen && FamilyTreeLayer != null)
            {
                FamilyTreeLayer = null;
                FamilyTreeVM = null;
                FamilyTreeMovie = null;
            }
        }
    }
}
