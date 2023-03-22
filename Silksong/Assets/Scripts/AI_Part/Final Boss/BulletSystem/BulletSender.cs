using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSender : MonoBehaviour
{
    public BulletObject bullet;
    float currentAngle = 0;
    float currentAngularVelocity = 0;
    float currentTime = 0;

    private void Awake()
    {
        pool = new BulletPool(bullet);

        currentAngle = bullet.initRotation;
        currentAngularVelocity = bullet.senderAngularVelocity;

    }

    private void FixedUpdate()
    {
        currentAngularVelocity = Mathf.Clamp(currentAngularVelocity + bullet.senderAcceleration * Time.fixedDeltaTime, -bullet.senderMaxAngularVelocity, bullet.senderMaxAngularVelocity);
        currentAngle += currentAngularVelocity * Time.fixedDeltaTime;
        if (Mathf.Abs(currentAngle) > 720f)
        {
            currentAngle -= Mathf.Sign(currentAngle) * 360f;
        }
        currentTime += Time.fixedDeltaTime;
        if (currentTime > bullet.interval)
        {
            currentTime -= bullet.interval;
            SendByCount(bullet.count, currentAngle);
        }
        
    }

    private void SendByCount(int count, float angle)
    {
        float temp = count % 2 == 0 ? angle + bullet.lineAngle /2 : angle;

        for (int i = 0; i < count; ++i)
        {
            temp += Mathf.Pow(-1, i) * i * bullet.lineAngle;
            Send(temp);
        }
    }

    public BulletPool pool;

    private void Send(float angle)
    {
        var bh = pool.GetObjectFromPool();
        pool.OnGetItem(bh);
        bh.gameObject.transform.position = transform.position;
        bh.gameObject.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
