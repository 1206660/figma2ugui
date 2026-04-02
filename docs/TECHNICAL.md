# 技术实现文档

## 项目概述

**项目**: figma2ugui  
**版本**: 1.0.0  
**日期**: 2026-04-02

## 1. Figma API 集成

### 1.1 认证

使用 Personal Access Token 进行认证：

```csharp
public class FigmaApiClient
{
    private const string BASE_URL = "https://api.figma.com/v1";
    private string accessToken;

    public FigmaApiClient(string token)
    {
        accessToken = token;
    }

    private UnityWebRequest CreateRequest(string endpoint)
    {
        var request = UnityWebRequest.Get($"{BASE_URL}/{endpoint}");
        request.SetRequestHeader("X-Figma-Token", accessToken);
        return request;
    }
}
```

### 1.2 获取文件数据

```csharp
public async Task<FigmaFile> GetFileAsync(string fileKey)
{
    var request = CreateRequest($"files/{fileKey}");
    await request.SendWebRequest();
    
    if (request.result == UnityWebRequest.Result.Success)
    {
        return JsonUtility.FromJson<FigmaFile>(request.downloadHandler.text);
    }
    
    throw new Exception($"Failed to fetch file: {request.error}");
}
```

### 1.3 导出图片

```csharp
public async Task<byte[]> ExportImageAsync(string fileKey, string nodeId, string format = "png", float scale = 1f)
{
    var endpoint = $"images/{fileKey}?ids={nodeId}&format={format}&scale={scale}";
    var request = CreateRequest(endpoint);
    await request.SendWebRequest();
    
    var response = JsonUtility.FromJson<ExportResponse>(request.downloadHandler.text);
    var imageUrl = response.images[nodeId];
    
    // 下载图片
    var imageRequest = UnityWebRequest.Get(imageUrl);
    await imageRequest.SendWebRequest();
    
    return imageRequest.downloadHandler.data;
}
```

## 2. 数据模型

### 2.1 Figma 数据模型

```csharp
[Serializable]
public class FigmaFile
{
    public string name;
    public FigmaNode document;
}

[Serializable]
public class FigmaNode
{
    public string id;
    public string name;
    public string type;
    public FigmaNode[] children;
    public AbsoluteBoundingBox absoluteBoundingBox;
    public Paint[] fills;
    public Paint[] strokes;
    public TypeStyle style;
    public LayoutConstraint constraints;
}

[Serializable]
public class AbsoluteBoundingBox
{
    public float x;
    public float y;
    public float width;
    public float height;
}

[Serializable]
public class Paint
{
    public string type;
    public Color color;
    public float opacity;
}

[Serializable]
public class TypeStyle
{
    public string fontFamily;
    public float fontSize;
    public float fontWeight;
    public float letterSpacing;
    public float lineHeightPx;
}
```

### 2.2 中间数据模型

```csharp
public class UguiNode
{
    public string name;
    public UguiComponentType componentType;
    public RectTransformData rectTransform;
    public ComponentData componentData;
    public List<UguiNode> children = new List<UguiNode>();
}

public enum UguiComponentType
{
    Canvas,
    Panel,
    Image,
    Text,
    Button,
    HorizontalLayout,
    VerticalLayout
}

public class RectTransformData
{
    public Vector2 anchoredPosition;
    public Vector2 sizeDelta;
    public Vector2 anchorMin;
    public Vector2 anchorMax;
    public Vector2 pivot;
}
```

## 3. 解析器实现

### 3.1 节点解析

```csharp
public class FigmaParser
{
    public UguiNode Parse(FigmaNode figmaNode)
    {
        var uguiNode = new UguiNode
        {
            name = figmaNode.name,
            componentType = DetermineComponentType(figmaNode),
            rectTransform = CalculateRectTransform(figmaNode),
            componentData = ExtractComponentData(figmaNode)
        };

        if (figmaNode.children != null)
        {
            foreach (var child in figmaNode.children)
            {
                uguiNode.children.Add(Parse(child));
            }
        }

        return uguiNode;
    }

    private UguiComponentType DetermineComponentType(FigmaNode node)
    {
        switch (node.type)
        {
            case "FRAME":
                return UguiComponentType.Panel;
            case "TEXT":
                return UguiComponentType.Text;
            case "RECTANGLE":
            case "VECTOR":
                return UguiComponentType.Image;
            default:
                return UguiComponentType.Panel;
        }
    }
}
```

### 3.2 布局计算

```csharp
public class LayoutCalculator
{
    public RectTransformData CalculateRectTransform(FigmaNode node, FigmaNode parent = null)
    {
        var bounds = node.absoluteBoundingBox;
        var data = new RectTransformData();

        if (parent == null)
        {
            // 根节点
            data.anchorMin = new Vector2(0.5f, 0.5f);
            data.anchorMax = new Vector2(0.5f, 0.5f);
            data.pivot = new Vector2(0.5f, 0.5f);
            data.anchoredPosition = Vector2.zero;
            data.sizeDelta = new Vector2(bounds.width, bounds.height);
        }
        else
        {
            var parentBounds = parent.absoluteBoundingBox;
            
            // 计算相对位置
            float relativeX = bounds.x - parentBounds.x;
            float relativeY = parentBounds.height - (bounds.y - parentBounds.y) - bounds.height;
            
            data.anchorMin = Vector2.zero;
            data.anchorMax = Vector2.zero;
            data.pivot = new Vector2(0, 1);
            data.anchoredPosition = new Vector2(relativeX, -relativeY);
            data.sizeDelta = new Vector2(bounds.width, bounds.height);
        }

        return data;
    }
}
```

## 4. 转换器实现

### 4.1 UGUI 转换

```csharp
public class UguiConverter
{
    private AssetManager assetManager;

    public GameObject Convert(UguiNode node, Transform parent = null)
    {
        var go = new GameObject(node.name);
        
        if (parent != null)
        {
            go.transform.SetParent(parent, false);
        }

        // 添加 RectTransform
        var rectTransform = go.AddComponent<RectTransform>();
        ApplyRectTransform(rectTransform, node.rectTransform);

        // 添加组件
        switch (node.componentType)
        {
            case UguiComponentType.Canvas:
                AddCanvas(go);
                break;
            case UguiComponentType.Image:
                AddImage(go, node.componentData);
                break;
            case UguiComponentType.Text:
                AddText(go, node.componentData);
                break;
            case UguiComponentType.HorizontalLayout:
                AddHorizontalLayout(go);
                break;
            case UguiComponentType.VerticalLayout:
                AddVerticalLayout(go);
                break;
        }

        // 递归处理子节点
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

    private void AddImage(GameObject go, ComponentData data)
    {
        var image = go.AddComponent<Image>();
        image.color = data.color;
        
        if (!string.IsNullOrEmpty(data.spritePath))
        {
            image.sprite = assetManager.LoadSprite(data.spritePath);
        }
    }

    private void AddText(GameObject go, ComponentData data)
    {
        var text = go.AddComponent<TextMeshProUGUI>();
        text.text = data.text;
        text.fontSize = data.fontSize;
        text.color = data.color;
        text.alignment = data.textAlignment;
    }
}
```

## 5. 资源管理

### 5.1 资源下载和缓存

```csharp
public class AssetManager
{
    private string cacheDir = "Assets/FigmaCache";
    
    public async Task<string> DownloadAndCacheImage(string fileKey, string nodeId, byte[] imageData)
    {
        var fileName = $"{fileKey}_{nodeId}.png";
        var filePath = Path.Combine(cacheDir, fileName);
        
        Directory.CreateDirectory(cacheDir);
        File.WriteAllBytes(filePath, imageData);
        
        AssetDatabase.Refresh();
        
        // 设置为 Sprite
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
```

## 6. 编辑器窗口

### 6.1 UI 实现

```csharp
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
        
        accessToken = EditorGUILayout.TextField("Access Token", accessToken);
        fileUrl = EditorGUILayout.TextField("Figma File URL", fileUrl);
        
        EditorGUI.BeginDisabledGroup(isImporting);
        if (GUILayout.Button("Import"))
        {
            StartImport();
        }
        EditorGUI.EndDisabledGroup();
        
        if (isImporting)
        {
            EditorGUI.ProgressBar(
                EditorGUILayout.GetControlRect(), 
                progress, 
                $"Importing... {(int)(progress * 100)}%"
            );
        }
    }
    
    private async void StartImport()
    {
        isImporting = true;
        progress = 0f;
        
        try
        {
            var fileKey = ExtractFileKey(fileUrl);
            await ImportFile(fileKey);
            EditorUtility.DisplayDialog("Success", "Import completed!", "OK");
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Error", e.Message, "OK");
        }
        finally
        {
            isImporting = false;
        }
    }
}
```

## 7. 完整导入流程

```csharp
public class FigmaImporter
{
    private FigmaApiClient apiClient;
    private FigmaParser parser;
    private UguiConverter converter;
    private AssetManager assetManager;
    
    public async Task ImportFile(string fileKey, Action<float> onProgress = null)
    {
        // 1. 获取文件数据
        onProgress?.Invoke(0.1f);
        var file = await apiClient.GetFileAsync(fileKey);
        
        // 2. 解析节点
        onProgress?.Invoke(0.3f);
        var rootNode = parser.Parse(file.document);
        
        // 3. 下载资源
        onProgress?.Invoke(0.5f);
        await DownloadAssets(file.document, fileKey);
        
        // 4. 转换为 UGUI
        onProgress?.Invoke(0.7f);
        var rootObject = converter.Convert(rootNode);
        
        // 5. 保存为 Prefab
        onProgress?.Invoke(0.9f);
        SaveAsPrefab(rootObject, file.name);
        
        onProgress?.Invoke(1.0f);
    }
    
    private void SaveAsPrefab(GameObject go, string name)
    {
        var path = $"Assets/Prefabs/{name}.prefab";
        Directory.CreateDirectory("Assets/Prefabs");
        PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
    }
}
```

## 8. 错误处理

```csharp
public class FigmaException : Exception
{
    public FigmaException(string message) : base(message) { }
}

public class ApiErrorHandler
{
    public static void HandleError(UnityWebRequest request)
    {
        if (request.responseCode == 403)
        {
            throw new FigmaException("Invalid access token");
        }
        else if (request.responseCode == 404)
        {
            throw new FigmaException("File not found");
        }
        else
        {
            throw new FigmaException($"API Error: {request.error}");
        }
    }
}
```

## 参考资料

- [Figma REST API](https://www.figma.com/developers/api)
- [Unity UGUI](https://docs.unity3d.com/Manual/UISystem.html)
- [UnityWebRequest](https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.html)
