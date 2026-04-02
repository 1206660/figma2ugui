using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Figma2Ugui.Models
{
    public class UguiNode
    {
        public string name;
        public UguiComponentType componentType;
        public RectTransformData rectTransform;
        public ComponentData componentData;
        public List<UguiNode> children = new List<UguiNode>();
    }

    public enum UguiComponentType
    {
        Canvas,
        Panel,
        Image,
        Text,
        Button,
        ScrollView,
        HorizontalLayout,
        VerticalLayout,
        GridLayout
    }

    public class RectTransformData
    {
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
    }

    public class ComponentData
    {
        public Color color = Color.white;
        public string text;
        public float fontSize;
        public string fontFamily;
        public int fontWeight;
        public TextAlignmentOptions textAlign = TextAlignmentOptions.Left;
        public float letterSpacing;
        public string imageRef;
        public string imageNodeId; // Figma node ID, used for API export
        public Sprite sprite;
        public bool isScrollable;
    }
}
