using System;
using UnityEngine;

namespace Array2DEditor
{
    [Serializable]
    public class Array2DBool : Array2D<bool>
    {
        [SerializeField] private CellRowBool[] cells = new CellRowBool[Consts.defaultGridSize];

        protected override CellRow<bool> GetCellRow(int idx)
        {
            return cells[idx];
        }
    }
}