using System;
using System.Collections.Generic;
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
        public List<Paint> fills;
        public List<Paint> strokes;
        public TypeStyle style;
        public string characters;
        public string blendMode;
        public bool clipsContent;
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
        public string type; // SOLID, GRADIENT_LINEAR, GRADIENT_RADIAL, IMAGE, etc.
        public string imageRef; // hash for IMAGE type fills
        public FigmaColor color; // only for SOLID
        public float opacity = 1f;
        public string scaleMode; // FILL, FIT, CROP, etc.
        public List<GradientStop> gradientStops;
    }

    [Serializable]
    public class GradientStop
    {
        public FigmaColor color;
        public float position;
    }

    [Serializable]
    public class FigmaColor
    {
        public float r;
        public float g;
        public float b;
        public float a = 1f;

        public Color ToUnityColor()
        {
            return new Color(r, g, b, a);
        }
    }

    [Serializable]
    public class TypeStyle
    {
        public string fontFamily;
        public string fontPostScriptName;
        public string fontStyle;
        public float fontWeight;
        public float fontSize;
        public float letterSpacing;
        public float lineHeightPx;
        public string textAlignHorizontal;
        public string textAlignVertical;
        public string textCase;
    }
}
