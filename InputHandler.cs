using System.Diagnostics;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Debug; // 使用 using static 来引用 Debug 类



public class InputHandler : MonoBehaviour
{
    public InputField inputField; // 引用InputField组件
    public string pythonPath = "python"; // Python可执行文件的路径
    public string scriptPath = @"D:\unityFile\word-to-picture.py"; // Python脚本的路径
    // public Text outputText;//在文本框中输出
    
    public void PythonCaller()
    {
        string text = inputField.text; // 获取InputField中的文本

        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = pythonPath;
        start.Arguments = $"\"{scriptPath}\" \"{text}\""; // 将文本作为参数传递给Python脚本
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;

        using (Process process = Process.Start(start))
        {
            using (System.IO.StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                //int n=70;
                //string lastNChars=result.Substring(result.Length-n);
                Log("Python output: " + result); // 打印Python脚本的输出
                // outputText.text = lastNChars; 
                // outputText.text = result;//文本框中的内容变成result
         
            }
        }
        
    }

}
