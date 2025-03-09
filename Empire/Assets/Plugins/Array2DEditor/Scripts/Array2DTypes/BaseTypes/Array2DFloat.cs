using System;
using UnityEngine;

namespace Array2DEditor
{
    [Serializable]
    public class Array2DFloat : Array2D<float>
    {
        [SerializeField] private CellRowFloat[] cells = new CellRowFloat[Consts.defaultGridSize];

        protected override CellRow<float> GetCellRow(int idx)
        {
            return cells[idx];
        }
    }
}