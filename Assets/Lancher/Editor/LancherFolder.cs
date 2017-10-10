using UnityEngine;
using System.Collections;
namespace Lancher
{
    public class LancherFolder
    {
        public enum TYPE
        {
            BUNDLE,
            UNBUNDLE,
        }

        public string mFolder = "Assets";
        public TYPE mType = TYPE.BUNDLE;
    }
}


