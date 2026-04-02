using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Figma2Ugui.Models;

namespace Figma2Ugui.Core
{
    public class FigmaApiClient
    {
        private const string BASE_URL = "https://api.figma.com/v1";
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
                return JsonUtility.FromJson<FigmaFile>(request.downloadHandler.text);
            }

            throw new Exception($"Failed to fetch file: {request.error}");
        }

        public async Task<byte[]> ExportImageAsync(string fileKey, string nodeId)
        {
            var endpoint = $"images/{fileKey}?ids={nodeId}&format=png&scale=1";
            var request = CreateRequest(endpoint);
            await SendRequest(request);

            var response = JsonUtility.FromJson<ExportResponse>(request.downloadHandler.text);
            var imageUrl = response.images[nodeId];

            var imageRequest = UnityWebRequest.Get(imageUrl);
            await SendRequest(imageRequest);

            return imageRequest.downloadHandler.data;
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
    public class ExportResponse
    {
        public System.Collections.Generic.Dictionary<string, string> images;
    }
}
