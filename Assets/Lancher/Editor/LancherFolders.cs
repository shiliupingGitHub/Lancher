using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
namespace Lancher
{
    public class LancherFolders
    {
       public List<LancherFolder> mFolders = new List<LancherFolder>();
        void DrawFolders()
        {
            EditorGUILayout.BeginFadeGroup(1);
            List<LancherFolder> rs = new List<LancherFolder>();
            foreach (var f in mFolders)
            {
                EditorGUILayout.BeginHorizontal();
                f.mType = (LancherFolder.TYPE)EditorGUILayout.EnumPopup(f.mType, GUILayout.MaxWidth(100));
                EditorGUILayout.LabelField(f.mFolder, GUILayout.MaxWidth(200));
                if (GUILayout.Button("browser", GUILayout.MaxWidth(100)))
                {
                    string folder = EditorUtility.OpenFolderPanel(string.Empty, string.Empty, string.Empty);
                    if (!string.IsNullOrEmpty(folder))
                    {
                        if (!folder.Contains(Application.dataPath))
                        {
                            EditorUtility.DisplayDialog("error", "you should select project path", "ok");
                            return;
                        }
                        f.mFolder = folder.Substring(Application.dataPath.Length - 6);
                    }
                }
                if (GUILayout.Button("del", GUILayout.MaxWidth(100)))
                {
                    rs.Add(f);
                }
                EditorGUILayout.EndHorizontal();
            }
            foreach (var r in rs)
            {
                mFolders.Remove(r);
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("add" ,GUILayout.MaxWidth(100)))
            {
                mFolders.Add(new LancherFolder());
            }
            if (GUILayout.Button("save", GUILayout.MaxWidth(100)))
            {
               string p = LancherEditor.Instance.ConfigPath;
                string content = XmlHelper.XmlSerialize(this, System.Text.Encoding.UTF8);
                File.WriteAllText(p, content);
                //XmlHelper.XmlSerializeToFile(this, p, System.Text.Encoding.UTF8);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndFadeGroup();
        }
        void DrawBuild()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("BuildPc", GUILayout.MaxWidth(100)))
            {
                BuildBundle(BuildTarget.StandaloneWindows);
            }
            if (GUILayout.Button("BuildAndroid", GUILayout.MaxWidth(100)))
            {
                BuildBundle(BuildTarget.Android);
            }
            if (GUILayout.Button("BuildIOS", GUILayout.MaxWidth(100)))
            {
                BuildBundle(BuildTarget.iOS);
            }
            EditorGUILayout.EndHorizontal();
        }
       void BuildBundle(BuildTarget bt)
        {
          string path=  EditorUtility.SaveFolderPanel(string.Empty, string.Empty, string.Empty);
          if(!string.IsNullOrEmpty(path))
            {
                List<AssetBundleBuild> abs = new List<AssetBundleBuild>();
                List<LancherFolder> mtxtFolders = new List<LancherFolder>();
                foreach (var f in mFolders)
                {
                    switch (f.mType)
                    {
                        case LancherFolder.TYPE.PREFABS:
                            {
                               string[] files= Directory.GetFiles(f.mFolder);
                                foreach(var file in files)
                                {
                                    if (!file.EndsWith(".prefab"))
                                        continue;
                                   string szName = Path.GetFileNameWithoutExtension(file);
                                    AssetBundleBuild ab = new AssetBundleBuild();
                                    ab.assetBundleName = szName;
                                    ab.assetNames = new string[] { file };
                                    abs.Add(ab);
                                }
                            }
                            break;
                        case LancherFolder.TYPE.TXT:
                            {
                                mtxtFolders.Add(f);
                            }
                            break;
                    }

                    
                    
                    
                }
                string tempPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) +"tmp";
                if(Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
                Directory.CreateDirectory(tempPath);
                AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(tempPath, abs.ToArray(), BuildAssetBundleOptions.None, bt);

            }


        }
       public void Draw()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label("------folder-------");
            DrawFolders();
            GUILayout.Label("------Build-------");
            DrawBuild();
            EditorGUILayout.EndVertical();
        }
    }
}
