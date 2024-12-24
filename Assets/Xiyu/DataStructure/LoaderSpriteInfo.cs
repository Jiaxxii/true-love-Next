using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Xiyu.DataStructure
{
    [System.Serializable]
    public struct LoaderSpriteInfo
    {
        public LoaderSpriteInfo(params string[] resourceCode)
        {
            this.resourceCode = resourceCode;
            allLoader = false;
        }

        public LoaderSpriteInfo(IEnumerable<string> resourceCode)
        {
            this.resourceCode = resourceCode.ToArray();
            allLoader = false;
        }

        public LoaderSpriteInfo(bool allLoader)
        {
            this.allLoader = allLoader;
            resourceCode = null;
        }


        [SerializeField] private bool allLoader;
        [SerializeField] private string[] resourceCode;

        public bool AllLoader => allLoader;
        public string[] ResourceCode => resourceCode;
    }
}