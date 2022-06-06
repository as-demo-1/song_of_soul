using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedGrowing : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    [Range(0, 1)]
    public float boundery = 0;
    private static int _boundery = Shader.PropertyToID("_Boundery");
    private Material _mat;
    // Update is called once per frame
    void Update()
    {
        if (_mat == null) _mat = GetComponent<SpriteRenderer>().material;
        _mat.SetFloat(_boundery, boundery);
    }
}
