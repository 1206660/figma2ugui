using Figma2Ugui.Models;
using Figma2Ugui.AI;
using UnityEngine;

namespace Figma2Ugui.Core
{
    public class FigmaParser
    {
        private ComponentAnalyzer componentAnalyzer;
        private FontAnalyzer fontAnalyzer;

        public FigmaParser()
        {
            componentAnalyzer = new ComponentAnalyzer();
            fontAnalyzer = new FontAnalyzer();
        }
        public UguiNode Parse(FigmaNode figmaNode, FigmaNode parent = null)
        {
            var uguiNode = new UguiNode
            {
                name = figmaNode.name,
                componentType = DetermineComponentType(figmaNode),
                rectTransform = CalculateRectTransform(figmaNode, parent),
                componentData = ExtractComponentData(figmaNode)
            };

            if (figmaNode.children != null)
            {
                foreach (var child in figmaNode.children)
                {
                    uguiNode.children.Add(Parse(child, figmaNode));
                }
            }

            return uguiNode;
        }

        private UguiComponentType DetermineComponentType(FigmaNode node)
        {
            return componentAnalyzer.AnalyzeComponent(node);
        }

        private RectTransformData CalculateRectTransform(FigmaNode node, FigmaNode parent)
        {
            var bounds = node.absoluteBoundingBox;
            var data = new RectTransformData();

            if (parent == null)
            {
                data.anchorMin = new Vector2(0.5f, 0.5f);
                data.anchorMax = new Vector2(0.5f, 0.5f);
                data.pivot = new Vector2(0.5f, 0.5f);
                data.anchoredPosition = Vector2.zero;
                data.sizeDelta = new Vector2(bounds.width, bounds.height);
            }
            else
            {
                var parentBounds = parent.absoluteBoundingBox;
                float x = bounds.x - parentBounds.x;
                float y = parentBounds.height - (bounds.y - parentBounds.y) - bounds.height;

                data.anchorMin = Vector2.zero;
                data.anchorMax = Vector2.zero;
                data.pivot = new Vector2(0, 1);
                data.anchoredPosition = new Vector2(x, -y);
                data.sizeDelta = new Vector2(bounds.width, bounds.height);
            }

            return data;
        }

        private ComponentData ExtractComponentData(FigmaNode node)
        {
            var data = new ComponentData();

            if (node.fills != null && node.fills.Length > 0)
            {
                data.color = node.fills[0].color;
            }

            if (node.type == "TEXT")
            {
                data.text = node.characters;
                fontAnalyzer.AnalyzeFont(node, data);
            }

            return data;
        }
    }
}
