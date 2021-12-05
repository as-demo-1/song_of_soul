using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpritePixelsPerUnitSetTo120 : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        //Debug.Log("pre");
        TextureImporter textureImporter = (TextureImporter)assetImporter;
        textureImporter.spritePixelsPerUnit = 120;

    }


}
