using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Text;
using System;

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
        private  string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail, error:" +ex.Message);
            }
        }
        void BuildBundle(BuildTarget bt)
        {
          string path=  EditorUtility.SaveFolderPanel(string.Empty, string.Empty, string.Empty);
          if(!string.IsNullOrEmpty(path))
            {
                BundleManifest bundleManifest = new BundleManifest();
                List<AssetBundleBuild> abs = new List<AssetBundleBuild>();
       
                Bundles bundlList = new Bundles();
                Bundles ConfiglList = new Bundles();
                string tempPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "tmp";
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
                Directory.CreateDirectory(tempPath);
                string bundlePath = path + "/bundles";
                string configPath = path + "/config";
                if (!Directory.Exists(bundlePath))
                    Directory.CreateDirectory(bundlePath);
                if (!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);
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
                        case LancherFolder.TYPE.CONFIG:
                            {
                                string[] files = Directory.GetFiles(f.mFolder);
                                foreach(var file in files)
                                {
                                    if (file.EndsWith(".meta"))
                                        continue;
                                    Bundle b = new Bundle();
                                    b.mName = Path.GetFileName(file);
                                    b.mType = Bundle.TYPE.CONFIG;
                                    b.mVersion = GetMD5HashFromFile(file);
                                    ConfiglList.mList.Add(b);
                                    string tagetPath = configPath + "/" + b.mName;
                                    string srcPath = file;
                                    if (File.Exists(tagetPath))
                                    {
                                        FileUtil.ReplaceFile(srcPath, tagetPath);
                                    }
                                    else
                                        FileUtil.CopyFileOrDirectory(srcPath, tagetPath);

                                }
                            }
                            break;
                    }

                    
                    
                    
                }
         
                AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(tempPath, abs.ToArray(), BuildAssetBundleOptions.None, bt);
                string[] bundels = manifest.GetAllAssetBundles();
                foreach(var b in bundels)
                {
                    Bundle bundle = new Bundle();
                    bundle.mName = b;
                    uint crc = 0;
                    if(BuildPipeline.GetCRCForAssetBundle(tempPath+"/"+b,out crc))
                    {
                        bundle.mType = Bundle.TYPE.AB;
                        bundle.mVersion = crc.ToString();
                        bundle.depences = manifest.GetAllDependencies(b);
                        bundlList.mList.Add(bundle);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("error", "something wrong happened", "ok");
                    }
                    string tagetPath = bundlePath + "/" + b;
                    string srcPath = tempPath + "/" + b;
                    if(File.Exists(tagetPath))
                    {
                        FileUtil.ReplaceFile(srcPath, tagetPath);
                    }
                    else
                        FileUtil.CopyFileOrDirectory(srcPath, tagetPath);

                }
                bundleManifest.mBundles = bundlList;
                bundleManifest.mBundles.mList.AddRange(ConfiglList.mList);
                Directory.Delete(tempPath, true);
                XmlHelper.XmlSerializeToFile(bundleManifest, path + "/bundleManifest", System.Text.Encoding.UTF8);
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
