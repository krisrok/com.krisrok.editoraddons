using UnityEngine;

public static class LayerMaskExtensions
{
    /// <summary>
    /// Extension method to check if a layer is in a layermask
    /// </summary>
    /// <param name="mask"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static bool Contains(this LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }

    /// <summary> Converts given mask to layer number </summary>
    /// <returns> layer number </returns>
    public static int ToSingleLayerNumber(this LayerMask mask)
    {
        int result = mask > 0 ? 0 : 31;
        while (mask > 1)
        {
            mask = mask >> 1;
            result++;
        }
        return result;
    }
}
