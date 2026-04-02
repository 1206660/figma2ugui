using System;
using UnityEngine;

namespace Figma2Ugui.Core
{
    [Serializable]
    public class ProjectSettings
    {
        public string accessToken;
        public string fileUrl;
        public string outputPath = "Assets/Prefabs";
        public bool downloadImages = true;
        public float imageScale = 1f;
    }
}
