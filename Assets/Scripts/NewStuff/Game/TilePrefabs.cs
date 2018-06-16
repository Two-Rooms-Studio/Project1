using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TilePrefabs
{
    [System.Serializable]
    public class TilePrefab
    {
        public string Name;
        public GameObject Prefab;

        public TilePrefab(string Name, GameObject Prefab)
        {
            this.Name = Name;
            this.Prefab = Prefab;
        }
    }
}
