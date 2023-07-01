using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpritePixelsPerUnitSetTo120 : AssetPostprocessor
{
    /// <summary>
    /// pixel在这里设置成了的120，理论上可以在这里修改成需要的大小，但是修改成本估计会很恐怖
    /// </summary>
    void OnPreprocessTexture()
    {
        //Debug.Log("pre");
        TextureImporter textureImporter = (TextureImporter)assetImporter;
        textureImporter.spritePixelsPerUnit = 120;

    }


}
