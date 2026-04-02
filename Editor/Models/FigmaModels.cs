using System;
using UnityEngine;

namespace Figma2Ugui.Models
{
    [Serializable]
    public class FigmaFile
    {
        public string name;
        public FigmaNode document;
    }

    [Serializable]
    public class FigmaNode
    {
        public string id;
        public string name;
        public string type;
        public FigmaNode[] children;
        public AbsoluteBoundingBox absoluteBoundingBox;
        public Paint[] fills;
        public Paint[] strokes;
        public TypeStyle style;
        public string characters;
    }

    [Serializable]
    public class AbsoluteBoundingBox
    {
        public float x;
        public float y;
        public float width;
        public float height;
    }

    [Serializable]
    public class Paint
    {
        public string type;
        public Color color;
        public float opacity = 1f;
    }

    [Serializable]
    public class TypeStyle
    {
        public string fontFamily;
        public float fontSize;
        public float fontWeight;
        public float letterSpacing;
        public float lineHeightPx;
    }
}
