using System.Collections.Generic;
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

        /// <summary>
        /// 解析并收集所有需要下载图片的 imageRef
        /// </summary>
        public UguiNode Parse(FigmaNode figmaNode, FigmaNode parent = null, HashSet<string> imageRefs = null)
        {
            // DOCUMENT / CANVAS 等顶层节点没有 bounds，跳过它们，直接解析子节点
            if (figmaNode.absoluteBoundingBox == null)
            {
                var container = new UguiNode
                {
                    name = figmaNode.name,
                    componentType = UguiComponentType.Panel,
                    rectTransform = new RectTransformData
                    {
                        anchorMin = new Vector2(0.5f, 0.5f),
                        anchorMax = new Vector2(0.5f, 0.5f),
                        pivot = new Vector2(0.5f, 0.5f),
                        anchoredPosition = Vector2.zero,
                        sizeDelta = new Vector2(1920f, 1080f)
                    },
                    componentData = new ComponentData()
                };

                if (figmaNode.children != null)
                {
                    foreach (var child in figmaNode.children)
                    {
                        container.children.Add(Parse(child, null, imageRefs));
                    }
                }

                return container;
            }

            var uguiNode = new UguiNode
            {
                name = figmaNode.name,
                componentType = DetermineComponentType(figmaNode),
                rectTransform = CalculateRectTransform(figmaNode, parent),
                componentData = ExtractComponentData(figmaNode, imageRefs)
            };

            if (figmaNode.children != null)
            {
                foreach (var child in figmaNode.children)
                {
                    uguiNode.children.Add(Parse(child, figmaNode, imageRefs));
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

            if (parent == null || parent.absoluteBoundingBox == null)
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
                // Figma Y 向下增大, Unity Y 向上增大, pivot (0,1) 从左上角定位
                float x = bounds.x - parentBounds.x;
                float y = -(bounds.y - parentBounds.y);

                data.anchorMin = Vector2.zero;
                data.anchorMax = Vector2.zero;
                data.pivot = new Vector2(0, 1);
                data.anchoredPosition = new Vector2(x, y);
                data.sizeDelta = new Vector2(bounds.width, bounds.height);
            }

            return data;
        }

        private ComponentData ExtractComponentData(FigmaNode node, HashSet<string> imageRefs)
        {
            var data = new ComponentData();

            if (node.fills != null)
            {
                foreach (var fill in node.fills)
                {
                    if (fill.type == "SOLID" && fill.color != null)
                    {
                        data.color = fill.color.ToUnityColor();
                        break;
                    }
                    if ((fill.type == "GRADIENT_LINEAR" || fill.type == "GRADIENT_RADIAL")
                        && fill.gradientStops != null && fill.gradientStops.Count > 0
                        && fill.gradientStops[0].color != null)
                    {
                        data.color = fill.gradientStops[0].color.ToUnityColor();
                        break;
                    }
                    // IMAGE fill → 记录 node ID 用于 API 导出，imageRef 用于去重
                    if (fill.type == "IMAGE" && !string.IsNullOrEmpty(fill.imageRef))
                    {
                        data.imageRef = fill.imageRef;
                        data.imageNodeId = node.id;
                        // 用 imageRef 去重，同一张图只下载一次
                        imageRefs?.Add(fill.imageRef);
                        break;
                    }
                }
            }

            if (node.type == "TEXT")
            {
                data.text = node.characters ?? "";
                fontAnalyzer.AnalyzeFont(node, data);
            }

            return data;
        }
    }
}
