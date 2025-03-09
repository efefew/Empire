using System;
using UnityEngine;

namespace Array2DEditor
{
    [Serializable]
    public class CellRowBool : CellRow<bool>
    {
    }

    [Serializable]
    public class CellRowFloat : CellRow<float>
    {
    }

    [Serializable]
    public class CellRowInt : CellRow<int>
    {
    }

    [Serializable]
    public class CellRowDouble : CellRow<double>
    {
    }

    [Serializable]
    public class CellRowString : CellRow<string>
    {
    }

    [Serializable]
    public class CellRowSprite : CellRow<Sprite>
    {
    }

    [Serializable]
    public class CellRowAudioClip : CellRow<AudioClip>
    {
    }
}