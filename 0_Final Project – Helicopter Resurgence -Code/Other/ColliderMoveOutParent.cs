using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderMoveOutParent : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController._player.transform.SetParent(null);
        gameObject.GetComponent<Collider2D>().enabled = false;
    }
}
