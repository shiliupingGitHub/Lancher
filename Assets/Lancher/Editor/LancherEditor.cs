using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Text;
namespace Lancher
{

    public class LancherEditor : EditorWindow
    {
        public static LancherEditor Instance;
        LancherFolders fs = new LancherFolders();
        [MenuItem("Lancher/Editor")]
        static void OpenWindow()
        {
            Instance = GetWindow<LancherEditor>();
            Instance.LoadConfig();
        }
       public string ConfigPath
        {
            get
            {
                return Application.dataPath + "/Lancher/Editor/Lancher.txt";
            }
        }

        void LoadConfig()
        {
            string path = ConfigPath;
            if(File.Exists(path))
            {
                fs = XmlHelper.XmlDeserializeFromFile<LancherFolders>(path, Encoding.UTF8);
            }
        }
        void OnGUI()
        {
            fs.Draw();
        }

    }

}
