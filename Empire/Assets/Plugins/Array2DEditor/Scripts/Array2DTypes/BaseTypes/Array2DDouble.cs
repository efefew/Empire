using UnityEngine;

namespace Array2DEditor
{
    [System.Serializable]
    public class Array2DDouble : Array2D<double>
    {
        [SerializeField]
        private CellRowDouble[] cells = new CellRowDouble[Consts.defaultGridSize];

        protected override CellRow<double> GetCellRow(int idx) => cells[idx];
    }
}
