using UnityEngine;
using System.Collections;
namespace Lancher
{
    public class LancherFolder
    {
        public enum TYPE
        {
            PREFABS,
            CONFIG,
        }

        public string mFolder = "Assets";
        public TYPE mType = TYPE.PREFABS;
    }
}


