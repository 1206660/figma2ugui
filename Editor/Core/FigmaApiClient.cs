using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using Newtonsoft.Json;
using Figma2Ugui.Models;

namespace Figma2Ugui.Core
{
    public class FigmaApiClient
    {
        private const string BASE_URL = "https://api.figma.com/v1";
        private const string CACHE_DIR = "Assets/FigmaCache";
        private string accessToken;

        public FigmaApiClient(string token)
        {
            accessToken = token;
        }

        public async Task<FigmaFile> GetFileAsync(string fileKey)
        {
            var request = CreateRequest($"files/{fileKey}");
            await SendRequest(request);

            if (request.result == UnityWebRequest.Result.Success)
            {
                var json = request.downloadHandler.text;
                var debugPath = Path.Combine(Application.dataPath, "FigmaCache", "debug_response.json");
                Directory.CreateDirectory(Path.GetDirectoryName(debugPath));
                File.WriteAllText(debugPath, json);
                Debug.Log($"[Figma2Ugui] Debug JSON saved to: {debugPath}");
                return JsonConvert.DeserializeObject<FigmaFile>(json);
            }

            throw new Exception($"Failed to fetch file: {request.error}");
        }

        /// <summary>
        /// 批量导出图片
        /// nodeIdToRef: nodeId → imageRef 的映射
        /// 返回: imageRef → Sprite 的映射
        /// </summary>
        public async Task<Dictionary<string, Sprite>> ExportImagesAsync(
            string fileKey,
            Dictionary<string, string> nodeIdToRef,
            float scale,
            Action<int, int> onProgress)
        {
            var result = new Dictionary<string, Sprite>();
            if (nodeIdToRef.Count == 0) return result;

            Directory.CreateDirectory(CACHE_DIR);

            AssetDatabase.StartAssetEditing();
            var allNodeIds = new List<string>(nodeIdToRef.Keys);
            var total = allNodeIds.Count;
            var batchSize = 10;
            var downloaded = 0;

            // imageRef 去重：已下载过的 imageRef 不重复下载
            var downloadedRefs = new HashSet<string>();

            for (var i = 0; i < allNodeIds.Count; i += batchSize)
            {
                var batch = allNodeIds.GetRange(i, Math.Min(batchSize, total - i));
                var ids = string.Join(",", batch);
                var endpoint = $"images/{fileKey}?ids={ids}&format=png&scale={scale}";
                Debug.Log($"[Figma2Ugui] Exporting batch {i / batchSize + 1}/{Mathf.CeilToInt((float)total / batchSize)} ({batch.Count} images)");
                var request = CreateRequest(endpoint);
                await SendRequest(request);

                if ((long)request.responseCode == 429)
                {
                    Debug.LogWarning("[Figma2Ugui] Rate limited, waiting 5s...");
                    Debug.LogWarning("[Figma2Ugui] Rate limited, waiting 5s...");
                    await Task.Delay(5000);
                    i -= batchSize;
                    continue;
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[Figma2Ugui] Failed to export images batch: {request.error}");
                    continue;
                }

                var response = JsonConvert.DeserializeObject<ImageExportResponse>(request.downloadHandler.text);
                if (response?.images == null) continue;

                foreach (var kvp in response.images)
                {
                    var nodeId = kvp.Key;
                    var imageUrl = kvp.Value;

                    // 找到对应的 imageRef
                    if (!nodeIdToRef.TryGetValue(nodeId, out var imageRef)) continue;

                    // 同一张图已下载过，复用 sprite
                    if (downloadedRefs.Contains(imageRef)) continue;

                    try
                    {
                        var bytes = await DownloadBytesAsync(imageUrl);
                        var sprite = CreateSprite(bytes, imageRef);
                        result[imageRef] = sprite;
                        downloadedRefs.Add(imageRef);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Figma2Ugui] Failed to download image {nodeId}: {e.Message}");
                    }

                    downloaded++;
                }

                onProgress?.Invoke(downloaded, total);
                await Task.Delay(200);
            }

            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
            Debug.Log($"[Figma2Ugui] Asset database refreshed.");

            return result;
        }

        private Sprite CreateSprite(byte[] bytes, string imageRef)
        {
            var fileName = $"{imageRef}.png";
            var filePath = Path.Combine(CACHE_DIR, fileName);

            // 已存在则直接加载
            if (File.Exists(filePath))
            {
                var existing = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
                if (existing != null) return existing;
            }

            File.WriteAllBytes(filePath, bytes);

            // 先让 Unity 识别这个文件
            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceSynchronousImport);

            // 配置为 Sprite
            var importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.SaveAndReimport();
            }
            else
            {
                Debug.LogError($"[Figma2Ugui] Failed to get TextureImporter for {filePath}");
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
            if (sprite == null)
            {
                Debug.LogError($"[Figma2Ugui] Failed to load sprite from {filePath}");
            }
            else
            {
                Debug.Log($"[Figma2Ugui] Created sprite: {sprite.name} ({sprite.texture.width}x{sprite.texture.height})");
            }
            return sprite;
        }

        private async Task<byte[]> DownloadBytesAsync(string url)
        {
            var request = UnityWebRequest.Get(url);
            await SendRequest(request);
            return request.downloadHandler.data;
        }

        private UnityWebRequest CreateRequest(string endpoint)
        {
            var request = UnityWebRequest.Get($"{BASE_URL}/{endpoint}");
            request.SetRequestHeader("X-Figma-Token", accessToken);
            return request;
        }

        private async Task SendRequest(UnityWebRequest request)
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }
        }
    }

    [Serializable]
    public class ImageExportResponse
    {
        public Dictionary<string, string> images;
    }
}
