using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class _2DC_ShaderLerpDemo : MonoBehaviour {

    public Material mat;
    public string variable;
    public AnimationCurve anm;
    public float Mul=1;
    public float Speed=1;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (mat!=null) mat.SetFloat(variable, anm.Evaluate(Time.time * Speed)*Mul);
    }
}
