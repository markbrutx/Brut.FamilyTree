using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Brut.FamilyTree.Helpers
{
    /// <summary>
    /// Represents a single step in a family relation chain.
    /// </summary>
    public enum RelationType
    {
        Self,
        Father,
        Mother,
        Son,
        Daughter,
        Spouse,
        ExSpouse,
        Brother,
        Sister,
        Grandfather,
        Grandmother,
        Grandson,
        Granddaughter,
        Uncle,
        Aunt,
        Nephew,
        Niece,
        Cousin
    }

    /// <summary>
    /// A step in the relation path from one hero to another.
    /// </summary>
    public class RelationStep
    {
        public Hero Hero { get; }
        public RelationType Relation { get; }
        public Hero? Parent1 { get; }
        public Hero? Parent2 { get; }

        public RelationStep(Hero hero, RelationType relation, Hero? parent1 = null, Hero? parent2 = null)
        {
            Hero = hero;
            Relation = relation;
            Parent1 = parent1;
            Parent2 = parent2;
        }
    }

    /// <summary>
    /// Helper class for building family relation chains.
    /// </summary>
    public static class FamilyHelper
    {
        /// <summary>
        /// Gets a full relation chain description from hero to comparedHero.
        /// Example: "Son of Biba and Boba, Grandson of Blink"
        /// </summary>
        public static TextObject GetFullRelationChain(Hero hero, Hero comparedHero)
        {
            if (hero == comparedHero)
            {
                return new TextObject("");
            }

            var path = FindRelationPath(hero, comparedHero);
            if (path == null || path.Count == 0)
            {
                return GetDirectRelation(hero, comparedHero);
            }

            return BuildRelationDescription(hero, path);
        }

        /// <summary>
        /// Finds the shortest path of relations from hero to target using BFS.
        /// </summary>
        private static List<RelationStep>? FindRelationPath(Hero hero, Hero target)
        {
            var visited = new HashSet<Hero> { hero };
            var queue = new Queue<(Hero current, List<RelationStep> path)>();
            queue.Enqueue((hero, new List<RelationStep>()));

            while (queue.Count > 0)
            {
                var (current, path) = queue.Dequeue();

                // Check parents
                if (current.Father != null && !visited.Contains(current.Father))
                {
                    var newPath = new List<RelationStep>(path)
                    {
                        new RelationStep(current.Father, RelationType.Father)
                    };
                    if (current.Father == target) return newPath;
                    visited.Add(current.Father);
                    queue.Enqueue((current.Father, newPath));
                }

                if (current.Mother != null && !visited.Contains(current.Mother))
                {
                    var newPath = new List<RelationStep>(path)
                    {
                        new RelationStep(current.Mother, RelationType.Mother)
                    };
                    if (current.Mother == target) return newPath;
                    visited.Add(current.Mother);
                    queue.Enqueue((current.Mother, newPath));
                }

                // Check children
                if (current.Children != null)
                {
                    foreach (var child in current.Children)
                    {
                        if (!visited.Contains(child))
                        {
                            var relType = child.IsFemale ? RelationType.Daughter : RelationType.Son;
                            var newPath = new List<RelationStep>(path)
                            {
                                new RelationStep(child, relType, current.Father, current.Mother)
                            };
                            if (child == target) return newPath;
                            visited.Add(child);
                            queue.Enqueue((child, newPath));
                        }
                    }
                }

                // Check spouse
                if (current.Spouse != null && !visited.Contains(current.Spouse))
                {
                    var newPath = new List<RelationStep>(path)
                    {
                        new RelationStep(current.Spouse, RelationType.Spouse)
                    };
                    if (current.Spouse == target) return newPath;
                    visited.Add(current.Spouse);
                    queue.Enqueue((current.Spouse, newPath));
                }

                // Check siblings (same father or mother)
                var siblings = GetSiblings(current);
                foreach (var sibling in siblings)
                {
                    if (!visited.Contains(sibling))
                    {
                        var relType = sibling.IsFemale ? RelationType.Sister : RelationType.Brother;
                        var newPath = new List<RelationStep>(path)
                        {
                            new RelationStep(sibling, relType)
                        };
                        if (sibling == target) return newPath;
                        visited.Add(sibling);
                        queue.Enqueue((sibling, newPath));
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all siblings of a hero (same father or mother).
        /// </summary>
        private static List<Hero> GetSiblings(Hero hero)
        {
            var siblings = new HashSet<Hero>();

            if (hero.Father?.Children != null)
            {
                foreach (var child in hero.Father.Children)
                {
                    if (child != hero)
                        siblings.Add(child);
                }
            }

            if (hero.Mother?.Children != null)
            {
                foreach (var child in hero.Mother.Children)
                {
                    if (child != hero)
                        siblings.Add(child);
                }
            }

            return new List<Hero>(siblings);
        }

        /// <summary>
        /// Builds a human-readable description of the relation path.
        /// </summary>
        private static TextObject BuildRelationDescription(Hero hero, List<RelationStep> path)
        {
            var parts = new List<string>();

            // Build description based on path
            if (path.Count == 1)
            {
                var step = path[0];
                // For single parent step, hero is the CHILD of target
                // Path "Father" means target is hero's father, so hero is son/daughter
                // Game adds the target name, so we just return the role
                if (step.Relation == RelationType.Father || step.Relation == RelationType.Mother)
                {
                    var childRel = hero.IsFemale
                        ? LocalizationHelper.GetString("str_daughter")
                        : LocalizationHelper.GetString("str_son");
                    return new TextObject(childRel);
                }
                return GetRelationText(hero, step);
            }

            // For multi-step paths, describe each step
            if (path.Count >= 2)
            {
                // Check for grandparent pattern (Father/Mother -> Father/Mother)
                // Format: "Son of X and Y, Grandson" (game adds grandparent name)
                if (IsGrandparentPattern(path))
                {
                    var grandchildRel = hero.IsFemale
                        ? LocalizationHelper.GetString("str_granddaughter")
                        : LocalizationHelper.GetString("str_grandson");

                    if (hero.Father != null || hero.Mother != null)
                    {
                        var parentText = GetParentOfText(hero);
                        return new TextObject($"{parentText}, {grandchildRel}");
                    }

                    return new TextObject(grandchildRel);
                }

                // Check for grandchild pattern (Son/Daughter -> Son/Daughter)
                // This is from grandparent's perspective - hero is the grandchild
                if (IsGrandchildPattern(path))
                {
                    var grandchildRel = hero.IsFemale
                        ? LocalizationHelper.GetString("str_granddaughter")
                        : LocalizationHelper.GetString("str_grandson");

                    if (hero.Father != null || hero.Mother != null)
                    {
                        var parentText = GetParentOfText(hero);
                        return new TextObject($"{parentText}, {grandchildRel}");
                    }

                    return new TextObject(grandchildRel);
                }

                // Check for spouse-of-child pattern (Spouse -> Father/Mother)
                // Format: "Wife of X, son" (game adds parent name)
                if (IsSpouseOfChildPattern(path))
                {
                    var spouse = path[0].Hero; // The person hero is married to

                    var spouseRel = hero.IsFemale
                        ? LocalizationHelper.GetString("str_wife_of_child_short")
                        : LocalizationHelper.GetString("str_husband_of_child_short");

                    var childRel = spouse.IsFemale
                        ? LocalizationHelper.GetString("str_daughter_of_short")
                        : LocalizationHelper.GetString("str_son_of_short");

                    return new TextObject(spouseRel
                        .Replace("{SPOUSE}", spouse.FirstName.ToString())
                        .Replace("{CHILD_REL}", childRel));
                }

                // Check for nephew/niece pattern (Father/Mother -> Brother/Sister)
                // Format: "Son of Nathanos and Megenhilda, nephew" + game adds "of Brut"
                if (IsNephewPattern(path))
                {
                    var siblingParent = path[0].Hero; // Parent of hero who is sibling of comparedHero

                    var template = hero.IsFemale
                        ? LocalizationHelper.GetString("str_daughter_of_sibling")
                        : LocalizationHelper.GetString("str_son_of_sibling");

                    // Get the other parent
                    var otherParent = hero.Father == siblingParent ? hero.Mother : hero.Father;
                    var otherParentName = otherParent?.FirstName?.ToString() ?? "";

                    return new TextObject(template
                        .Replace("{PARENT}", siblingParent.FirstName.ToString())
                        .Replace("{PARENT2}", otherParentName));
                }

                // Check for sibling's spouse pattern (Brother/Sister -> Spouse)
                // Format: "Wife of Nathanos, sister-in-law" + game adds "of Brut"
                if (IsSiblingSpousePattern(path))
                {
                    var sibling = path[0].Hero; // The sibling of comparedHero

                    var template = hero.IsFemale
                        ? LocalizationHelper.GetString("str_wife_of_sibling")
                        : LocalizationHelper.GetString("str_husband_of_sibling");

                    return new TextObject(template
                        .Replace("{SPOUSE}", sibling.FirstName.ToString()));
                }
            }

            // Build step-by-step description
            var firstStep = path[0];
            parts.Add(GetRelationText(hero, firstStep).ToString());

            for (int i = 1; i < path.Count && i < 3; i++) // Limit to 3 steps for readability
            {
                var step = path[i];
                var prevHero = path[i - 1].Hero;
                parts.Add(GetRelationText(prevHero, step).ToString());
            }

            return new TextObject(string.Join(", ", parts));
        }

        private static bool IsGrandparentPattern(List<RelationStep> path)
        {
            if (path.Count < 2) return false;
            var r1 = path[0].Relation;
            var r2 = path[1].Relation;
            return (r1 == RelationType.Father || r1 == RelationType.Mother) &&
                   (r2 == RelationType.Father || r2 == RelationType.Mother);
        }

        private static bool IsGrandchildPattern(List<RelationStep> path)
        {
            if (path.Count < 2) return false;
            var r1 = path[0].Relation;
            var r2 = path[1].Relation;
            return (r1 == RelationType.Son || r1 == RelationType.Daughter) &&
                   (r2 == RelationType.Son || r2 == RelationType.Daughter);
        }

        private static bool IsSpouseOfChildPattern(List<RelationStep> path)
        {
            if (path.Count < 2) return false;
            var r1 = path[0].Relation;
            var r2 = path[1].Relation;
            return r1 == RelationType.Spouse &&
                   (r2 == RelationType.Father || r2 == RelationType.Mother);
        }

        /// <summary>
        /// Checks if path represents nephew/niece pattern (Father/Mother -> Brother/Sister).
        /// hero is the child of comparedHero's sibling.
        /// </summary>
        private static bool IsNephewPattern(List<RelationStep> path)
        {
            if (path.Count < 2) return false;
            var r1 = path[0].Relation;
            var r2 = path[1].Relation;
            return (r1 == RelationType.Father || r1 == RelationType.Mother) &&
                   (r2 == RelationType.Brother || r2 == RelationType.Sister);
        }

        /// <summary>
        /// Checks if path represents sibling's spouse pattern (Spouse -> Brother/Sister).
        /// hero is married to comparedHero's sibling.
        /// Path: hero -> spouse -> sibling (comparedHero)
        /// </summary>
        private static bool IsSiblingSpousePattern(List<RelationStep> path)
        {
            if (path.Count < 2) return false;
            var r1 = path[0].Relation;
            var r2 = path[1].Relation;
            return r1 == RelationType.Spouse &&
                   (r2 == RelationType.Brother || r2 == RelationType.Sister);
        }

        /// <summary>
        /// Gets a text description for a single relation step.
        /// </summary>
        private static TextObject GetRelationText(Hero from, RelationStep step)
        {
            var text = step.Relation switch
            {
                RelationType.Father => LocalizationHelper.GetString("str_father"),
                RelationType.Mother => LocalizationHelper.GetString("str_mother"),
                RelationType.Son => GetChildOfText(from, step.Hero, false),
                RelationType.Daughter => GetChildOfText(from, step.Hero, true),
                RelationType.Spouse => LocalizationHelper.GetString("str_spouse"),
                RelationType.Brother => LocalizationHelper.GetString("str_brother"),
                RelationType.Sister => LocalizationHelper.GetString("str_sister"),
                RelationType.Grandfather => LocalizationHelper.GetString("str_grandfather"),
                RelationType.Grandmother => LocalizationHelper.GetString("str_grandmother"),
                RelationType.Grandson => LocalizationHelper.GetString("str_grandson"),
                RelationType.Granddaughter => LocalizationHelper.GetString("str_granddaughter"),
                _ => step.Hero.FirstName.ToString()
            };

            return new TextObject(text);
        }

        /// <summary>
        /// Gets "Son/Daughter of X and Y" text.
        /// </summary>
        private static string GetChildOfText(Hero parent, Hero child, bool isFemale)
        {
            var father = child.Father;
            var mother = child.Mother;

            if (father != null && mother != null)
            {
                var template = isFemale
                    ? LocalizationHelper.GetString("str_daughter_of")
                    : LocalizationHelper.GetString("str_son_of");

                return template
                    .Replace("{PARENT1}", father.FirstName.ToString())
                    .Replace("{PARENT2}", mother.FirstName.ToString());
            }
            else if (father != null)
            {
                var childWord = isFemale
                    ? LocalizationHelper.GetString("str_daughter")
                    : LocalizationHelper.GetString("str_son");
                return $"{childWord} {father.FirstName}";
            }
            else if (mother != null)
            {
                var childWord = isFemale
                    ? LocalizationHelper.GetString("str_daughter")
                    : LocalizationHelper.GetString("str_son");
                return $"{childWord} {mother.FirstName}";
            }

            return isFemale
                ? LocalizationHelper.GetString("str_daughter")
                : LocalizationHelper.GetString("str_son");
        }

        /// <summary>
        /// Gets "Son/Daughter of X and Y" for the given hero.
        /// </summary>
        private static string GetParentOfText(Hero hero)
        {
            return GetChildOfText(hero.Father ?? hero.Mother!, hero, hero.IsFemale);
        }

        /// <summary>
        /// Gets direct relation without path traversal (fallback).
        /// </summary>
        private static TextObject GetDirectRelation(Hero hero, Hero comparedHero)
        {
            // Direct parent check
            if (hero.Father == comparedHero)
                return new TextObject(LocalizationHelper.GetString("str_father"));
            if (hero.Mother == comparedHero)
                return new TextObject(LocalizationHelper.GetString("str_mother"));

            // Direct child check
            if (comparedHero.Children != null && comparedHero.Children.Contains(hero))
            {
                return new TextObject(GetChildOfText(comparedHero, hero, hero.IsFemale));
            }

            // Spouse check
            if (hero.Spouse == comparedHero)
                return new TextObject(LocalizationHelper.GetString("str_spouse"));

            // Sibling check
            if (AreSiblings(hero, comparedHero))
            {
                return new TextObject(hero.IsFemale
                    ? LocalizationHelper.GetString("str_sister")
                    : LocalizationHelper.GetString("str_brother"));
            }

            return new TextObject("");
        }

        /// <summary>
        /// Checks if two heroes are siblings (share at least one parent).
        /// </summary>
        private static bool AreSiblings(Hero h1, Hero h2)
        {
            if (h1.Father != null && h1.Father == h2.Father) return true;
            if (h1.Mother != null && h1.Mother == h2.Mother) return true;
            return false;
        }
    }
}
