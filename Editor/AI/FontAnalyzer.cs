using Figma2Ugui.Models;

namespace Figma2Ugui.AI
{
    public class FontAnalyzer
    {
        public void AnalyzeFont(FigmaNode node, ComponentData data)
        {
            if (node.type != "TEXT" || node.style == null)
                return;

            data.fontFamily = node.style.fontFamily ?? "Arial";
            data.fontSize = node.style.fontSize > 0 ? node.style.fontSize : 14f;
            data.fontWeight = (int)node.style.fontWeight;
        }
    }
}
