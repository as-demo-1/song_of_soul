using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testBullet : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform setBulletPoint;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.J))
        {
            SetBullet(Vector3.right);
        }
    }

    protected void SetBullet(Vector3 BulletDir)
    {
        Instantiate(bulletPrefab, setBulletPoint.position, Quaternion.Euler(BulletDir));
    }
}
