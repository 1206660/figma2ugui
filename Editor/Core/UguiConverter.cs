using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using Figma2Ugui.Models;

namespace Figma2Ugui.Core
{
    public class UguiConverter
    {
        private Sprite whiteSprite;

        public UguiConverter()
        {
            whiteSprite = CreateWhiteSprite();
        }

        private Sprite CreateWhiteSprite()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
        }

        public GameObject Convert(UguiNode node, Transform parent = null)
        {
            var go = new GameObject(node.name);

            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            var rectTransform = go.AddComponent<RectTransform>();
            ApplyRectTransform(rectTransform, node.rectTransform);

            switch (node.componentType)
            {
                case UguiComponentType.Canvas:
                    go.AddComponent<Canvas>();
                    go.AddComponent<CanvasScaler>();
                    go.AddComponent<GraphicRaycaster>();
                    break;
                case UguiComponentType.Image:
                    var image = go.AddComponent<Image>();
                    image.sprite = node.componentData.sprite ?? whiteSprite;
                    image.color = node.componentData.color;
                    if (node.componentData.imageRef != null && node.componentData.sprite != null)
                    {
                        image.type = Image.Type.Simple;
                    }
                    break;
                case UguiComponentType.Text:
                    var text = go.AddComponent<TextMeshProUGUI>();
                    text.text = node.componentData.text;
                    text.fontSize = node.componentData.fontSize;
                    text.color = node.componentData.color;
                    text.alignment = node.componentData.textAlign;
                    text.characterSpacing = node.componentData.letterSpacing;
                    break;
                case UguiComponentType.ScrollView:
                    AddScrollView(go);
                    break;
                case UguiComponentType.Button:
                    go.AddComponent<Button>();
                    break;
            }

            foreach (var child in node.children)
            {
                Convert(child, go.transform);
            }

            return go;
        }

        private void ApplyRectTransform(RectTransform rt, RectTransformData data)
        {
            rt.anchorMin = data.anchorMin;
            rt.anchorMax = data.anchorMax;
            rt.pivot = data.pivot;
            rt.anchoredPosition = data.anchoredPosition;
            rt.sizeDelta = data.sizeDelta;
        }

        private void AddScrollView(GameObject go)
        {
            var scrollRect = go.AddComponent<ScrollRect>();
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(go.transform, false);
            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewport.AddComponent<Image>();
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
        }
    }
}
