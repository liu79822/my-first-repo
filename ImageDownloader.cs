using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ImageDownloader: MonoBehaviour
{
    [Header("UI References")]
    public RawImage targetImage; // 用于显示选中图片的 RawImage
    public Button saveButton; // 保存按钮
    public Dropdown formatDropdown; // 格式下拉框

    [Header("Default Settings")]
    public string defaultFileName = "MyImage";
    public string[] supportedFormats = { "PNG", "JPG" };

    private Texture2D selectedTexture; // 当前选中的纹理

    public static ImageDownloader Instance { get; private set; } // 单例

    void Awake()
    {
        // 初始化单例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 检查 formatDropdown 是否为 null
        if (formatDropdown == null)
        {
            Debug.LogError("formatDropdown is not assigned in Inspector!");
            return;
        }

        // 初始化UI组件
        formatDropdown.ClearOptions();
        formatDropdown.AddOptions(new List<string>(supportedFormats)); // 修复List引用

        // 绑定保存按钮事件
        // saveButton.onClick.AddListener(StartSaveProcess);
         // 绑定保存按钮事件
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(StartSaveProcess);
        }
        else
        {
            Debug.LogError("saveButton is not assigned in Inspector!");
        }
    }

    public void StartSaveProcess()
    {
        StartCoroutine(SaveImageCoroutine());
    }

    private IEnumerator SaveImageCoroutine()
    {
        // 在保存前检查 selectedTexture 是否为 Texture2D
        if (selectedTexture == null || !(selectedTexture is Texture2D))
        {
            Debug.LogError("No valid Texture2D selected!");
            yield break;
        }

        Texture2D texture2D = (Texture2D)selectedTexture;

        // 检查是否有选中的纹理
        if (selectedTexture == null)
        {
            Debug.LogError("No image selected!");
            yield break;
        }

        // 使用默认文件名
        string fileName = defaultFileName;
        string selectedFormat = supportedFormats[formatDropdown.value];

        // 构建默认路径
        string defaultPath = Path.Combine(
            Application.persistentDataPath,
            fileName + GetFileExtension(selectedFormat)
        );

        // 异步获取保存路径
        string selectedPath = null;
        yield return StartCoroutine(
            GetSavePathFromUser(
                defaultPath,
                selectedFormat,
                path => selectedPath = path
            )
        );

        if (!string.IsNullOrEmpty(selectedPath))
        {
            // 转换并保存图片
            byte[] imageData = ConvertTexture(selectedTexture, selectedFormat);
            File.WriteAllBytes(selectedPath, imageData);
            Debug.Log($"Image saved to: {selectedPath}");
        }
    }

    private byte[] ConvertTexture(Texture2D texture, string format)
    {
        switch (format.ToUpper())
        {
            case "PNG":
                return texture.EncodeToPNG();
            case "JPG":
                return texture.EncodeToJPG(85);
            default:
                Debug.LogError($"Unsupported format: {format}");
                return null;
        }
    }

    private string GetFileExtension(string format)
    {
        return format.ToUpper() switch
        {
            "PNG" => ".png",
            "JPG" => ".jpg",
            _ => ".dat"
        };
    }

    // 各平台文件对话框实现（使用回调返回结果）
    private IEnumerator GetSavePathFromUser(string defaultPath, string format, System.Action<string> callback)
    {
        string path = null;

        #if UNITY_EDITOR || UNITY_STANDALONE
        // Windows/Mac平台
        path = UnityEditor.EditorUtility.SaveFilePanel(
            "Save Image",
            Path.GetDirectoryName(defaultPath),
            Path.GetFileNameWithoutExtension(defaultPath),
            format.ToLower());
        #elif UNITY_ANDROID
        // Android平台
        AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment");
        AndroidJavaObject downloadDir = environment.CallStatic<AndroidJavaObject>(
            "getExternalStoragePublicDirectory",
            environment.GetStatic<string>("DIRECTORY_DOWNLOADS")
        );
        string androidPath = downloadDir.Call<string>("getAbsolutePath");
        path = Path.Combine(androidPath, Path.GetFileName(defaultPath));
        #elif UNITY_IOS
        // iOS平台
        path = Path.Combine(Application.persistentDataPath, "Documents", Path.GetFileName(defaultPath));
        #else
        // 其他平台
        path = defaultPath;
        #endif

        // 确保至少返回一次结果
        yield return null;
        callback(path);
    }

    // 公共方法，用于设置选中的纹理
    public void SetSelectedTexture(Texture2D texture)
    {
        selectedTexture = texture;
        if (targetImage != null)
        {
            targetImage.texture = texture;
        }
    }
}