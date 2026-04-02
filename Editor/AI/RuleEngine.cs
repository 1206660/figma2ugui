using System.Text.RegularExpressions;
using Figma2Ugui.Models;

namespace Figma2Ugui.AI
{
    public class RuleEngine
    {
        private RuleSet ruleSet;

        public RuleEngine(RuleSet rules)
        {
            ruleSet = rules;
        }

        public UguiComponentType DetermineComponentType(FigmaNode node)
        {
            foreach (var rule in ruleSet.namingRules)
            {
                if (Regex.IsMatch(node.name, rule.pattern))
                {
                    return ParseComponentType(rule.componentType);
                }
            }

            return node.type switch
            {
                "TEXT" => UguiComponentType.Text,
                "RECTANGLE" => UguiComponentType.Image,
                _ => UguiComponentType.Panel
            };
        }

        private UguiComponentType ParseComponentType(string type)
        {
            return type switch
            {
                "Button" => UguiComponentType.Panel,
                "Text" => UguiComponentType.Text,
                "Image" => UguiComponentType.Image,
                _ => UguiComponentType.Panel
            };
        }
    }
}
