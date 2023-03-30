using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SetAnotherSkeletonMat : MonoBehaviour
{
    // Start is called before the first frame update
    private SkeletonRenderer skeletonRenderer;
    public Material otherMaterial;
    private Material originMaterial;
    void Start()
    {
        skeletonRenderer = GetComponent<SkeletonRenderer>();
        List<Material> temp=new List<Material>();
        GetComponent<MeshRenderer>().GetSharedMaterials(temp);
        originMaterial = temp[0];

    }

    public void setMat(bool other)
    {
        if(other)
        {
            skeletonRenderer.CustomMaterialOverride[originMaterial] = otherMaterial;
        }         
        else
        {
            skeletonRenderer.CustomMaterialOverride[originMaterial] = originMaterial;
        }
            
    }
}
