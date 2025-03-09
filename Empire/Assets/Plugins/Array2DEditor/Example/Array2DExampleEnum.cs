using System;
using UnityEngine;

namespace Array2DEditor
{
    [Serializable]
    public class Array2DExampleEnum : Array2D<ExampleEnum>
    {
        [SerializeField] private CellRowExampleEnum[] cells = new CellRowExampleEnum[Consts.defaultGridSize];

        protected override CellRow<ExampleEnum> GetCellRow(int idx)
        {
            return cells[idx];
        }
    }

    [Serializable]
    public class CellRowExampleEnum : CellRow<ExampleEnum>
    {
    }
}