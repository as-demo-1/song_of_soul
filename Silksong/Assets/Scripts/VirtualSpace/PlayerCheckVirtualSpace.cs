using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckVirtualSpace : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("VirtualSpace"))
        {
            collision.gameObject.GetComponent<VirtualSpace>().TroggleRender(false);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("VirtualSpace"))
        {
            collision.gameObject.GetComponent<VirtualSpace>().TroggleRender(true);
        }
    }
}
