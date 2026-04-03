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

        public async Task<Dictionary<string, Sprite>> ExportImagesAsync(
            string fileKey,
            Dictionary<string, string> nodeIdToRef,
            float scale,
            Action<int, int> onProgress)
        {
            var result = new Dictionary<string, Sprite>();
            if (nodeIdToRef.Count == 0) return result;

            Directory.CreateDirectory(CACHE_DIR);

            // Phase 1: 检查本地缓存，收集需要新下载的 nodeId
            var needDownload = new Dictionary<string, string>(); // nodeId → imageRef
            foreach (var kvp in nodeIdToRef)
            {
                var nodeId = kvp.Key;    // Key = nodeId
                var imageRef = kvp.Value;  // Value = imageRef
                var cachedPath = Path.Combine(CACHE_DIR, $"{imageRef}.png");

                if (File.Exists(cachedPath))
                {
                    Debug.Log($"[Figma2Ugui] Cache hit: {imageRef.Substring(0, 12)}...");
                }
                else
                {
                    needDownload[nodeId] = imageRef;
                }
            }

            var cachedCount = nodeIdToRef.Count - needDownload.Count;
            if (cachedCount > 0)
            {
                Debug.Log($"[Figma2Ugui] {cachedCount}/{nodeIdToRef.Count} images found in local cache.");
            }

            if (needDownload.Count > 0)
            {
                // Phase 2: 批量从 Figma 下载，写入磁盘
                Debug.Log($"[Figma2Ugui] Downloading {needDownload.Count} new images from Figma...");
                var newFiles = new HashSet<string>(); // 写入的新文件路径

                var nodeIds = new List<string>(needDownload.Keys);
                var batchSize = 10;
                var done = 0;

                for (var i = 0; i < nodeIds.Count; i += batchSize)
                {
                    var batch = nodeIds.GetRange(i, Math.Min(batchSize, nodeIds.Count));
                    var ids = string.Join(",", batch);
                    var endpoint = $"images/{fileKey}?ids={ids}&format=png&scale={scale}";
                    Debug.Log($"[Figma2Ugui] Batch {i / batchSize + 1}/{Mathf.CeilToInt((float)nodeIds.Count / batchSize)}: requesting {batch.Count} images");

                    var request = CreateRequest(endpoint);
                    await SendRequest(request);

                    if ((long)request.responseCode == 429)
                    {
                        Debug.LogWarning("[Figma2Ugui] Rate limited, waiting 5s...");
                        await Task.Delay(5000);
                        i -= batchSize;
                        continue;
                    }

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"[Figma2Ugui] Batch failed [{request.responseCode}]: {request.error}");
                        done += batch.Count;
                        continue;
                    }

                    ImageExportResponse response;
                    try
                    {
                        response = JsonConvert.DeserializeObject<ImageExportResponse>(request.downloadHandler.text);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Figma2Ugui] Batch parse error: {e.Message}");
                        done += batch.Count;
                        continue;
                    }

                    if (response?.images == null)
                    {
                        Debug.LogError("[Figma2Ugui] Batch returned null images.");
                        done += batch.Count;
                        continue;
                    }

                    foreach (var imgKvp in response.images)
                    {
                        var nodeId = imgKvp.Key;
                        var imageUrl = imgKvp.Value;

                        if (!needDownload.TryGetValue(nodeId, out var imageRef)) continue;

                        if (string.IsNullOrEmpty(imageUrl))
                        {
                            Debug.LogWarning($"[Figma2Ugui] Skip {nodeId}: empty URL from Figma");
                            done++;
                            continue;
                        }

                        try
                        {
                            var bytes = await DownloadBytesAsync(imageUrl);
                            if (bytes == null || bytes.Length == 0)
                            {
                                Debug.LogWarning($"[Figma2Ugui] Skip {nodeId} (ref={imageRef.Substring(0, 12)}...): download returned 0 bytes");
                                done++;
                                continue;
                            }

                            var filePath = Path.Combine(CACHE_DIR, $"{imageRef}.png");
                            File.WriteAllBytes(filePath, bytes);
                            newFiles.Add(filePath);
                            Debug.Log($"[Figma2Ugui] Downloaded {nodeId} -> {imageRef.Substring(0, 12)}... ({bytes.Length} bytes)");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[Figma2Ugui] Error downloading {nodeId}: {e.Message}");
                        }

                        done++;
                    }

                    onProgress?.Invoke(cachedCount + done, nodeIdToRef.Count);
                    await Task.Delay(200);
                }

                Debug.Log($"[Figma2Ugui] All downloads complete. {newFiles.Count} new files written to disk.");
            }

            // Phase 3: 统一 Refresh + 配置 Sprite + 加载
            Debug.Log($"[Figma2Ugui] Refreshing asset database...");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var allImageRefs = new HashSet<string>(nodeIdToRef.Values); // Values = imageRef
            foreach (var imageRef in allImageRefs)
            {
                var filePath = Path.Combine(CACHE_DIR, $"{imageRef}.png");

                var importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
                if (importer == null)
                {
                    Debug.LogWarning($"[Figma2Ugui] Skip {imageRef.Substring(0, 12)}...: no TextureImporter at {filePath}");
                    continue;
                }

                // 检查是否已配置为 Sprite
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 100;
                    importer.alphaSource = TextureImporterAlphaSource.FromInput;
                    importer.SaveAndReimport();
                }

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
                if (sprite != null)
                {
                    result[imageRef] = sprite;
                    Debug.Log($"[Figma2Ugui] OK: {imageRef.Substring(0, 12)}... ({sprite.texture.width}x{sprite.texture.height})");
                }
                else
                {
                    Debug.LogWarning($"[Figma2Ugui] Sprite load failed for {filePath}");
                }
            }

            Debug.Log($"[Figma2Ugui] Image import complete: {result.Count}/{nodeIdToRef.Count} sprites loaded.");
            return result;
        }

        private async Task<byte[]> DownloadBytesAsync(string url)
        {
            var request = UnityWebRequest.Get(url);
            await SendRequest(request);

            if (request.result != UnityWebRequest.Result.Success
                || request.downloadHandler.data == null
                || request.downloadHandler.data.Length == 0)
            {
                Debug.LogWarning($"[Figma2Ugui] Download failed: {request.error}, url={url}");
                return null;
            }

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
