using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class Image : MonoBehaviour
{
    public InputField inputField;
    public Dropdown ratioDropdown;
    public GameObject rawImagePrefab;
    public Transform rawImageParent;
    public string folderPath;
    public InputHandler inputHandler;
    public OpenAIRequest openAIRequest;

    private bool isGenerating = false;

    public void OnButtonClick2()
    {
        if (isGenerating) return;
        isGenerating = true;

        if (openAIRequest == null || inputHandler == null)
        {
            UnityEngine.Debug.LogError("组件未赋值！");
            return;
        }

        openAIRequest.onStoryParsed = () =>
        {
            UnityEngine.Debug.Log("剧情解析完成，开始生成图片...");
            StartCoroutine(GenerateImagesFromScenes());
        };

        openAIRequest.OnSendStoryButtonClick();
    }

    IEnumerator GenerateImagesFromScenes()
    {
        foreach (Transform child in rawImageParent)
        {
            Destroy(child.gameObject);
        }

        float widthRatio = 1f, heightRatio = 1f;
        switch (ratioDropdown.value)
        {
            case 0: widthRatio = 3f; heightRatio = 4f; break;
            case 1: widthRatio = 1f; heightRatio = 1f; break;
            case 2: widthRatio = 16f; heightRatio = 9f; break;
            default: UnityEngine.Debug.LogError("无效比例"); yield break;
        }

        if (!int.TryParse(inputField.text, out int n))
        {
            UnityEngine.Debug.LogError("请输入数字数量");
            yield break;
        }

        var segments = openAIRequest.GetStorySegments()
            .OrderBy(kvp => kvp.Key)
            .ToList();

        int count = Mathf.Min(n, segments.Count);

        for (int i = 0; i < count; i++)
        {
            string text = segments[i].Value;
            inputHandler.PythonCaller();
            UnityEngine.Debug.Log($"生成第 {i + 1} 段图片：\n{text}");

            //这里是延迟显示生成
            yield return new WaitForSeconds(0.1f);

            string latestImagePath = GetLatestImagePath(folderPath);
            if (string.IsNullOrEmpty(latestImagePath))
            {
                UnityEngine.Debug.LogWarning("未找到图片，跳过...");
                continue;
            }

            GameObject rawImageObj = Instantiate(rawImagePrefab, rawImageParent);
            RawImage rawImage = rawImageObj.GetComponent<RawImage>();
            if (rawImage == null)
            {
                UnityEngine.Debug.LogError("RawImage组件缺失");
                continue;
            }

            GridLayoutGroup gridLayout = rawImageParent.GetComponent<GridLayoutGroup>();
            if (gridLayout != null)
            {
                float baseSize = 200f;
                float aspect = widthRatio / heightRatio;
                gridLayout.cellSize = new Vector2(
                    baseSize * Mathf.Sqrt(aspect),
                    baseSize / Mathf.Sqrt(aspect)
                );
            }

            try
            {
                byte[] data = File.ReadAllBytes(latestImagePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(data);
                rawImage.texture = texture;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("加载图片失败: " + e.Message);
            }
        }

        isGenerating = false;
    }

    string GetLatestImagePath(string folder)
    {
        try
        {
            return Directory.GetFiles(folder, "*.jpg")
                .OrderByDescending(f => File.GetCreationTime(f))
                .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }
}
