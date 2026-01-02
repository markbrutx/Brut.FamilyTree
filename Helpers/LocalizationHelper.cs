using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Brut.FamilyTree.Helpers
{
    public static class LocalizationHelper
    {
        private static readonly Dictionary<string, string> EnglishStrings = new()
        {
            { "str_family_tree", "Family Tree" },
            { "str_close", "Close" },
            { "str_father", "Father" },
            { "str_mother", "Mother" },
            { "str_son", "Son" },
            { "str_daughter", "Daughter" },
            { "str_spouse", "Spouse" },
            { "str_brother", "Brother" },
            { "str_sister", "Sister" },
            { "str_grandfather", "Grandfather" },
            { "str_grandmother", "Grandmother" },
            { "str_grandson", "Grandson" },
            { "str_granddaughter", "Granddaughter" },
            { "str_uncle", "Uncle" },
            { "str_aunt", "Aunt" },
            { "str_nephew", "Nephew" },
            { "str_niece", "Niece" },
            { "str_cousin", "Cousin" },
            { "str_son_of", "Son of {PARENT1} and {PARENT2}" },
            { "str_daughter_of", "Daughter of {PARENT1} and {PARENT2}" },
            { "str_grandson_of", "Grandson of {GRANDPARENT}" },
            { "str_granddaughter_of", "Granddaughter of {GRANDPARENT}" },
            { "str_wife_of_child", "Wife of {SPOUSE}, {CHILD_REL} {PARENT}" },
            { "str_husband_of_child", "Husband of {SPOUSE}, {CHILD_REL} {PARENT}" },
            { "str_wife_of_child_short", "Wife of {SPOUSE}, {CHILD_REL}" },
            { "str_husband_of_child_short", "Husband of {SPOUSE}, {CHILD_REL}" },
            { "str_son_of_short", "son of" },
            { "str_daughter_of_short", "daughter of" },
            { "str_son_of_sibling", "Son of {PARENT} and {PARENT2}, nephew" },
            { "str_daughter_of_sibling", "Daughter of {PARENT} and {PARENT2}, niece" },
            { "str_wife_of_sibling", "Wife of {SPOUSE}, sister-in-law" },
            { "str_husband_of_sibling", "Husband of {SPOUSE}, brother-in-law" }
        };

        private static readonly Dictionary<string, string> RussianStrings = new()
        {
            { "str_family_tree", "Родословная" },
            { "str_close", "Закрыть" },
            { "str_father", "Отец" },
            { "str_mother", "Мать" },
            { "str_son", "Сын" },
            { "str_daughter", "Дочь" },
            { "str_spouse", "Супруг" },
            { "str_brother", "Брат" },
            { "str_sister", "Сестра" },
            { "str_grandfather", "Дедушка" },
            { "str_grandmother", "Бабушка" },
            { "str_grandson", "Внук" },
            { "str_granddaughter", "Внучка" },
            { "str_uncle", "Дядя" },
            { "str_aunt", "Тётя" },
            { "str_nephew", "Племянник" },
            { "str_niece", "Племянница" },
            { "str_cousin", "Кузен" },
            { "str_son_of", "Сын {PARENT1} и {PARENT2}" },
            { "str_daughter_of", "Дочь {PARENT1} и {PARENT2}" },
            { "str_grandson_of", "Внук {GRANDPARENT}" },
            { "str_granddaughter_of", "Внучка {GRANDPARENT}" },
            { "str_wife_of_child", "Жена {SPOUSE}, {CHILD_REL} {PARENT}" },
            { "str_husband_of_child", "Муж {SPOUSE}, {CHILD_REL} {PARENT}" },
            { "str_wife_of_child_short", "Жена {SPOUSE}, {CHILD_REL}" },
            { "str_husband_of_child_short", "Муж {SPOUSE}, {CHILD_REL}" },
            { "str_son_of_short", "сына" },
            { "str_daughter_of_short", "дочери" },
            { "str_son_of_sibling", "Сын {PARENT} и {PARENT2}, племянник" },
            { "str_daughter_of_sibling", "Дочь {PARENT} и {PARENT2}, племянница" },
            { "str_wife_of_sibling", "Жена {SPOUSE}, невестка" },
            { "str_husband_of_sibling", "Муж {SPOUSE}, зять" }
        };

        private static bool IsRussian()
        {
            try
            {
                var lang = BannerlordConfig.Language;
                return lang != null && lang.Contains("Русский");
            }
            catch
            {
                return false;
            }
        }

        public static string GetString(string id)
        {
            var strings = IsRussian() ? RussianStrings : EnglishStrings;
            return strings.TryGetValue(id, out var value) ? value : id;
        }

        public static TextObject GetTextObject(string id)
        {
            return new TextObject(GetString(id));
        }

        public static TextObject GetTextObject(string id, params (string key, object value)[] variables)
        {
            var text = new TextObject(GetString(id));
            foreach (var (key, value) in variables)
            {
                text.SetTextVariable(key, value?.ToString() ?? "");
            }
            return text;
        }
    }
}
