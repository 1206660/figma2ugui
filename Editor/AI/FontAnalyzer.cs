using Figma2Ugui.Models;
using TMPro;
using UnityEngine;

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
            data.fontWeight = (int)(node.style.fontWeight > 0 ? node.style.fontWeight : 400);
            data.textAlign = ParseAlignment(node.style.textAlignHorizontal);
            data.letterSpacing = node.style.letterSpacing;
        }

        private TextAlignmentOptions ParseAlignment(string align)
        {
            return align switch
            {
                "CENTER" => TextAlignmentOptions.Center,
                "RIGHT" => TextAlignmentOptions.Right,
                "LEFT" => TextAlignmentOptions.Left,
                _ => TextAlignmentOptions.Left
            };
        }
    }
}
