using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.NewStuff.Board.BoardSettings
{
    public class DemoBoardSettings : ScriptableObject, IBoardSettings
    {
        public int Rows { get; private set; }
        public int Cols { get; private set; }
    }
}
