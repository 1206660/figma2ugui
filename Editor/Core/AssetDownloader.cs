using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Figma2Ugui.Core
{
    public class AssetDownloader
    {
        private const string CACHE_DIR = "Assets/FigmaCache";

        public async Task<string> DownloadImageAsync(string fileKey, string nodeId, byte[] imageData)
        {
            var fileName = $"{fileKey}_{nodeId}.png";
            var filePath = Path.Combine(CACHE_DIR, fileName);

            Directory.CreateDirectory(CACHE_DIR);
            File.WriteAllBytes(filePath, imageData);

            AssetDatabase.Refresh();

            var importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }

            return filePath;
        }

        public Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
