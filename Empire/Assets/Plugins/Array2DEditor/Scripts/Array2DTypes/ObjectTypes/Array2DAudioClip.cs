using System;
using UnityEngine;

namespace Array2DEditor
{
    [Serializable]
    public class Array2DAudioClip : Array2D<AudioClip>
    {
        [SerializeField] private CellRowAudioClip[] cells = new CellRowAudioClip[Consts.defaultGridSize];

        protected override CellRow<AudioClip> GetCellRow(int idx)
        {
            return cells[idx];
        }
    }
}