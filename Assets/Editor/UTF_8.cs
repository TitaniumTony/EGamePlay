using Sirenix.OdinInspector.Editor;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Text;
using System;
using Sirenix.OdinInspector;

namespace Game.Editor 
{
    public class UTF_8 : OdinEditorWindow
    {
        private string Path = "Assets";
        private string Result;
        private string Finish;
        private int FileCount;
        private int ChangeCount;
        private Vector2 _scrollPos;

        [MenuItem("Tools/UTF-8编码", false)]
        public static void Open()
        {
            var window = (UTF_8)EditorWindow.GetWindow(typeof(UTF_8), false, "UTF-8编码");
            window.maxSize = window.minSize = new Vector2(400, 400);
            window.Show();
        }

        private void OnGUI()
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(20);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("File_Path");
                        GUILayout.FlexibleSpace();
                        Path = GUILayout.TextField(Path, GUILayout.Width(300));
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);

                    GUILayout.Space(30);
                    
                    if (GUILayout.Button("生成"))
                    {
                        Debug.Log("点击生成");
                        FileCount = 0;
                        ChangeCount = 0;
                        GetAllFile();
                    }
                    if (GUILayout.Button("清空log"))
                    {
                        Debug.Log("点击清空log");
                        Clear();
                    }
                    GUILayout.Space(30);
                    GUILayout.Label(Finish);
                    GUILayout.Space(10);
                    GUILayout.Label(Result);
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }
        
        [Button(ButtonSizes.Large, Name = "生成2"), HorizontalGroup("row5")]
        private void GetMoney()
        {
            Debug.Log("点击生成2");
            FileCount = 0;
            ChangeCount = 0;
            GetAllFile();
        }

        private void GetAllFile()
        {
            if (Path == null || Path == "")
            {
                Result = "路径不可以为null";
            }
            //获取指定路径下面的所有资源文件  
            if (Directory.Exists(Path))
            {
                DirectoryInfo direction = new DirectoryInfo(Path);
                FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

                for (int i = 0; i < files.Length; i++)
                {
                    if (!files[i].Name.EndsWith(".cs"))
                    {
                        continue;
                    }
                    Encoding _encoding = GetType(files[i].OpenRead());
                    if (_encoding != Encoding.UTF8)
                    {
                        var s = File.ReadAllText(files[i].FullName, Encoding.GetEncoding("GB2312"));

                        File.WriteAllText(files[i].FullName, s, new UTF8Encoding(false));
                        ChangeCount++;
                    }

                    FileCount++;
                }
                Result = $"总共找到{FileCount}个cs文件     {ChangeCount}个cs文件的编码格式被修改成了UTF-8";
                Finish = "完成";
            }
            else
            {
                Result = "未找到此路径";
            }
        }
        private static System.Text.Encoding GetType(FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM
            Encoding reVal = Encoding.Default;

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
            {
                reVal = Encoding.UTF8;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = Encoding.Unicode;
            }
            r.Close();
            return reVal;

        }
        /// <summary>
        /// 判断是否是不带 BOM 的 UTF8 格式
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1;
            //计算当前正分析的字符应还有的字节数
            byte curByte; //当前分析的字节.
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }
        private void Clear()
        {
            Result = null;
            Finish = null;
        }
    }
}
