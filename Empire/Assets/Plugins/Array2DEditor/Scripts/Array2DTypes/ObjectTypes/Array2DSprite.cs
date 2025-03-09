using System;
using UnityEngine;

namespace Array2DEditor
{
    [Serializable]
    public class Array2DSprite : Array2D<Sprite>
    {
        [SerializeField] private CellRowSprite[] cells = new CellRowSprite[Consts.defaultGridSize];

        protected override CellRow<Sprite> GetCellRow(int idx)
        {
            return cells[idx];
        }
    }
}