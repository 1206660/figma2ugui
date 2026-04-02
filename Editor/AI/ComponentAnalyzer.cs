using Figma2Ugui.Models;

namespace Figma2Ugui.AI
{
    public class ComponentAnalyzer
    {
        public UguiComponentType AnalyzeComponent(FigmaNode node)
        {
            // TEXT 节点直接映射
            if (node.type == "TEXT")
                return UguiComponentType.Text;

            // ScrollView 检测
            if (IsScrollView(node))
                return UguiComponentType.ScrollView;

            // Button 检测
            if (IsButton(node))
                return UguiComponentType.Button;

            // RECTANGLE / ELLIPSE 等矢量图形 = Image
            if (node.type == "RECTANGLE" || node.type == "ELLIPSE" || node.type == "VECTOR")
                return UguiComponentType.Image;

            // 有子节点的 FRAME 始终是 Panel（容器）
            if (HasChildren(node))
                return UguiComponentType.Panel;

            // 无子节点的 FRAME + 有填充 = 纯色/渐变矩形 → Image
            if (node.type == "FRAME" && HasAnyFill(node))
                return UguiComponentType.Image;

            // 其他一律 Panel
            return UguiComponentType.Panel;
        }

        private bool IsScrollView(FigmaNode node)
        {
            var name = node.name.ToLower();
            return name.Contains("scroll") || name.Contains("list");
        }

        private bool IsButton(FigmaNode node)
        {
            var name = node.name.ToLower();
            return name.StartsWith("btn") || name.Contains("button");
        }

        private bool HasAnyFill(FigmaNode node)
        {
            return node.fills != null && node.fills.Count > 0;
        }

        private bool HasChildren(FigmaNode node)
        {
            return node.children != null && node.children.Length > 0;
        }
    }
}
