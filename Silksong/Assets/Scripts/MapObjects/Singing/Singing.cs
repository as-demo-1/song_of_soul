using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface SingComponent
{
    void WhenSinging();
}
public class Singing : MonoBehaviour
{
    public float singRadius = 5f;
    public float stretchSpeedMax=1f;
    public List<Vector2> effectPos;
    public GameObject singingEffect;
    Vector3 maxSize;
    Vector3 minSize;
    public bool ifRetraction = false;
    float speedReduce;
    float currentSpeed;
    Collider2D collider;
    public int effectNum = 0;
    //public bool isOnGround = true;
    private void Start()
    {
        collider = GetComponent<Collider2D>();
        maxSize = new Vector3(singRadius, singRadius);
        minSize = Vector3.zero;
        currentSpeed = stretchSpeedMax;
        speedReduce = stretchSpeedMax * stretchSpeedMax / (2 * (maxSize.x - minSize.x));
        GameObject player = FindObjectOfType<PlayerController>().gameObject;
        transform.position = player.transform.position;
        transform.SetParent(player.transform);
    }
    bool IfSinging()
    {
        if (PlayerInput.Instance.Singing.Held)
        {
            PlayerInput.Instance.jump.Disable();
            PlayerInput.Instance.normalAttack.Disable();
            PlayerInput.Instance.vertical.Disable();
            PlayerInput.Instance.horizontal.Disable();
            return true;
        }
        return false;
    }
    private void Update()
    {      
        if (IfSinging() &&transform.localScale.x < maxSize.x && !ifRetraction)
        {
            SingingEffects();
            transform.localScale += new Vector3(1, 1, 1) * currentSpeed* Time.deltaTime;
            if (currentSpeed > 0)
                currentSpeed -= speedReduce * Time.deltaTime;
            else
            {
                currentSpeed = 0;
                transform.localScale = maxSize;
            }
        }else if (!IfSinging()&&!ifRetraction&&transform.localScale.x>minSize.x)
        {
            collider.enabled = false;
            StartCoroutine(Disapear());
        }
        if ((transform.localScale.x - maxSize.x)>=0&&!ifRetraction)
        {
           if(!collider.enabled)
                collider.enabled = true;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<SingComponent>() != null)
        {
            collision.GetComponent<SingComponent>().WhenSinging();
        }
    }
    public void SingingEffects()
    {
        if (transform.localScale.x > effectPos[effectNum].x && transform.localScale.x < effectPos[effectNum + 1].x)
        {
            GameObject effect = Instantiate(singingEffect);
            effect.transform.position = transform.position;
            effect.transform.localScale = effectPos[effectNum];
            effectNum++;
        }
    }
    IEnumerator Disapear()
    {
        effectNum = 0;
        ifRetraction = true;

        while (transform.localScale.x > minSize.x)
        {
            transform.localScale -= new Vector3(1, 1, 1) * currentSpeed * Time.deltaTime;
            if (currentSpeed < stretchSpeedMax)
                currentSpeed += speedReduce * Time.deltaTime;
            else currentSpeed = stretchSpeedMax;
            yield return null;
        }
        transform.localScale = minSize;
        ifRetraction = false;
        PlayerInput.Instance.jump.Enable();
        PlayerInput.Instance.normalAttack.Enable();
        PlayerInput.Instance.vertical.Enable();
        PlayerInput.Instance.horizontal.Enable();
    }
}
