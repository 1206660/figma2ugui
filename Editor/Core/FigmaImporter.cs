using System;
using System.Threading.Tasks;
using Figma2Ugui.Models;

namespace Figma2Ugui.Core
{
    public class FigmaImporter
    {
        private FigmaApiClient apiClient;
        private FigmaParser parser;
        private UguiConverter converter;
        private PrefabCreator prefabCreator;

        public FigmaImporter(string accessToken)
        {
            apiClient = new FigmaApiClient(accessToken);
            parser = new FigmaParser();
            converter = new UguiConverter();
            prefabCreator = new PrefabCreator();
        }

        public async Task ImportFile(string fileKey, Action<float> onProgress = null)
        {
            onProgress?.Invoke(0.1f);
            var file = await apiClient.GetFileAsync(fileKey);

            onProgress?.Invoke(0.4f);
            var rootNode = parser.Parse(file.document);

            onProgress?.Invoke(0.7f);
            var rootObject = converter.Convert(rootNode);

            onProgress?.Invoke(0.9f);
            prefabCreator.SaveAsPrefab(rootObject, file.name);

            onProgress?.Invoke(1.0f);
        }
    }
}
