using UnityEngine;
using UnityEditor;
using System;
using Figma2Ugui.Core;

namespace Figma2Ugui.UI
{
    public class FigmaImporterWindow : EditorWindow
    {
        private string accessToken = "";
        private string fileUrl = "";
        private bool isImporting = false;
        private float progress = 0f;

        [MenuItem("Tools/Figma Importer")]
        public static void ShowWindow()
        {
            GetWindow<FigmaImporterWindow>("Figma Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Figma to UGUI Importer", EditorStyles.boldLabel);
            GUILayout.Space(10);

            accessToken = EditorGUILayout.TextField("Access Token", accessToken);
            fileUrl = EditorGUILayout.TextField("Figma File URL", fileUrl);

            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(isImporting || string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(fileUrl));
            if (GUILayout.Button("Import", GUILayout.Height(30)))
            {
                StartImport();
            }
            EditorGUI.EndDisabledGroup();

            if (isImporting)
            {
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, $"Importing... {(int)(progress * 100)}%");
            }
        }

        private async void StartImport()
        {
            isImporting = true;
            progress = 0f;

            try
            {
                var fileKey = ExtractFileKey(fileUrl);
                var importer = new FigmaImporter(accessToken);

                await importer.ImportFile(fileKey, p => {
                    progress = p;
                    Repaint();
                });

                EditorUtility.DisplayDialog("Success", "Import completed!", "OK");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", e.Message, "OK");
            }
            finally
            {
                isImporting = false;
                Repaint();
            }
        }

        private string ExtractFileKey(string url)
        {
            var parts = url.Split('/');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "file" && i + 1 < parts.Length)
                {
                    return parts[i + 1];
                }
            }
            throw new Exception("Invalid Figma URL");
        }
    }
}
