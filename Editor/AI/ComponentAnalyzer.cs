using Figma2Ugui.Models;

namespace Figma2Ugui.AI
{
    public class ComponentAnalyzer
    {
        public UguiComponentType AnalyzeComponent(FigmaNode node)
        {
            // 分析 ScrollView
            if (IsScrollView(node))
                return UguiComponentType.ScrollView;

            // 分析 Button
            if (IsButton(node))
                return UguiComponentType.Button;

            // 基础类型
            return node.type switch
            {
                "TEXT" => UguiComponentType.Text,
                "RECTANGLE" => UguiComponentType.Image,
                _ => UguiComponentType.Panel
            };
        }

        private bool IsScrollView(FigmaNode node)
        {
            return node.name.ToLower().Contains("scroll") ||
                   node.name.ToLower().Contains("list");
        }

        private bool IsButton(FigmaNode node)
        {
            return node.name.ToLower().StartsWith("btn") ||
                   node.name.ToLower().Contains("button");
        }
    }
}
