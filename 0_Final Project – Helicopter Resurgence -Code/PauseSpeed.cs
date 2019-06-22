using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseSpeed : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != "Player") return;
        PlayerController._player.PauseX = true;
        GetComponent<Collider2D>().enabled = false;
    }
}
