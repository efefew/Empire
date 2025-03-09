using System;
using UnityEngine;

namespace Array2DEditor
{
    [Serializable]
    public class Array2DString : Array2D<string>
    {
        [SerializeField] private CellRowString[] cells = new CellRowString[Consts.defaultGridSize];

        protected override CellRow<string> GetCellRow(int idx)
        {
            return cells[idx];
        }
    }
}