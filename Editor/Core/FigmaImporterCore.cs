using System;
using UnityEngine;

namespace Figma2Ugui.Core
{
    [Serializable]
    public class FigmaImporterCore : MonoBehaviour
    {
        public FigmaApiClient ApiClient;
        public FigmaParser Parser;
        public UguiConverter Converter;
        public AssetDownloader AssetDownloader;
        public PrefabCreator PrefabCreator;
        public ProjectSettings Settings;

        private void Awake()
        {
            ApiClient = new FigmaApiClient();
            Parser = new FigmaParser();
            Converter = new UguiConverter();
            AssetDownloader = new AssetDownloader();
            PrefabCreator = new PrefabCreator();
            Settings = new ProjectSettings();
        }
    }
}
