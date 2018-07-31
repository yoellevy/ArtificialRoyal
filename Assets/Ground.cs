using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour {
    private float decreaseRate = 0.01f;
    private void FixedUpdate()
    {
        //transform.localScale -= new Vector3(decreaseRate, decreaseRate, 0);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerScript player = collision.GetComponent<PlayerScript>();
            player.Die();
        }
        else if (collision.gameObject.CompareTag("Bullet"))
        {
            Destroy(collision.gameObject);
        }
    }
}
