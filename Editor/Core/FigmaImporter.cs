using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Figma2Ugui.Models;

namespace Figma2Ugui.Core
{
    public class FigmaImporter
    {
        private FigmaApiClient apiClient;
        private FigmaParser parser;
        private UguiConverter converter;
        private PrefabCreator prefabCreator;

        public string LastError { get; private set; }

        public FigmaImporter(string accessToken)
        {
            apiClient = new FigmaApiClient(accessToken);
            parser = new FigmaParser();
            converter = new UguiConverter();
            prefabCreator = new PrefabCreator();
        }

        public async Task ImportFile(string fileKey, Action<float> onProgress = null)
        {
            const string LOG = "[Figma2Ugui]";
            LastError = null;

            // 1. 获取文件数据
            Debug.Log($"{LOG} (1/6) Fetching file data from Figma API...");
            onProgress?.Invoke(0.05f);
            FigmaFile file;
            try
            {
                file = await apiClient.GetFileAsync(fileKey);
                Debug.Log($"{LOG} File \"{file.name}\" fetched successfully.");
            }
            catch (Exception e)
            {
                LastError = $"API Error: {e.Message}";
                throw;
            }

            // 2. 解析节点 + 收集 imageRef
            Debug.Log($"{LOG} (2/6) Parsing node tree...");
            onProgress?.Invoke(0.15f);
            var imageRefs = new HashSet<string>();
            var rootNode = parser.Parse(file.document, imageRefs: imageRefs);

            // 3. 建立 imageRef → nodeId 映射，收集需要导出的 nodeId
            onProgress?.Invoke(0.2f);
            var refToNodeId = new Dictionary<string, string>();
            CollectImageNodeIds(rootNode, refToNodeId);
            var uniqueImageCount = refToNodeId.Count;
            Debug.Log($"{LOG} Found {uniqueImageCount} unique images to download.");

            // 4. 批量下载图片（用 nodeId 调 API）
            Dictionary<string, Sprite> imageRefToSprite;
            try
            {
                var nodeIdToRef = new Dictionary<string, string>();
                foreach (var kvp in refToNodeId)
                {
                    nodeIdToRef[kvp.Value] = kvp.Key;
                }

                Debug.Log($"{LOG} (3/6) Downloading {uniqueImageCount} images from Figma...");
                imageRefToSprite = await apiClient.ExportImagesAsync(
                    fileKey, nodeIdToRef, 2f,
                    (done, total) => {
                        var t = 0.2f + 0.5f * ((float)done / total);
                        onProgress?.Invoke(t);
                    }
                );
                Debug.Log($"{LOG} Downloaded {imageRefToSprite.Count}/{uniqueImageCount} images.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{LOG} Image export warning: {e.Message}");
                imageRefToSprite = new Dictionary<string, Sprite>();
            }

            // 5. 将 Sprite 分配到对应的节点
            Debug.Log($"{LOG} (4/6) Assigning sprites to nodes...");
            onProgress?.Invoke(0.75f);
            AssignSprites(rootNode, imageRefToSprite);

            // 6. 转换为 UGUI
            Debug.Log($"{LOG} (5/6) Converting to UGUI hierarchy...");
            onProgress?.Invoke(0.85f);
            var rootObject = converter.Convert(rootNode);

            // 7. 保存为 Prefab
            Debug.Log($"{LOG} (6/6) Saving prefab...");
            onProgress?.Invoke(0.95f);
            prefabCreator.SaveAsPrefab(rootObject, file.name);

            onProgress?.Invoke(1.0f);
            Debug.Log($"{LOG} Import complete! Prefab saved as \"{file.name}\".");
        }

        private void CollectImageNodeIds(UguiNode node, Dictionary<string, string> refToNodeId)
        {
            if (node.componentData.imageRef != null && node.componentData.imageNodeId != null)
            {
                // 同一张图(imageRef)可能被多个节点使用，只记录第一个的 nodeId
                if (!refToNodeId.ContainsKey(node.componentData.imageRef))
                {
                    refToNodeId[node.componentData.imageRef] = node.componentData.imageNodeId;
                }
            }

            foreach (var child in node.children)
            {
                CollectImageNodeIds(child, refToNodeId);
            }
        }

        private void AssignSprites(UguiNode node, Dictionary<string, Sprite> spriteMap)
        {
            if (node.componentData.imageRef != null && spriteMap.TryGetValue(node.componentData.imageRef, out var sprite))
            {
                node.componentData.sprite = sprite;
            }

            foreach (var child in node.children)
            {
                AssignSprites(child, spriteMap);
            }
        }
    }
}
