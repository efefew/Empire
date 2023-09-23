using UnityEngine;

namespace Array2DEditor
{
    [System.Serializable]
    public class Array2DFloat : Array2D<float>
    {
        [SerializeField]
        private CellRowFloat[] cells = new CellRowFloat[Consts.defaultGridSize];

        protected override CellRow<float> GetCellRow(int idx) => cells[idx];
    }
}
