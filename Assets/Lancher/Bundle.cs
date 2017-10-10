using UnityEngine;
using System.Collections;
namespace Lancher
{
    public class Bundle
    {
        public enum TYPE
        {
            AB,
            CONFIG,
        }
        public TYPE mType = TYPE.AB;
        public string mName;
        public string mVersion;
        public string[] depences;
    }
}


