using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
namespace Lancher
{
    public class LancherFolders
    {
        List<LancherFolder> mFolders = new List<LancherFolder>();
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
            if (GUILayout.Button("add" ,GUILayout.MaxWidth(100)))
            {
                mFolders.Add(new LancherFolder());
            }
            EditorGUILayout.EndFadeGroup();
        }
        void DrawBuild()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("BuildPc", GUILayout.MaxWidth(100)))
            {

            }
            if (GUILayout.Button("BuildAndroid", GUILayout.MaxWidth(100)))
            {

            }
            if (GUILayout.Button("BuildIOS", GUILayout.MaxWidth(100)))
            {

            }
            EditorGUILayout.EndHorizontal();
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
