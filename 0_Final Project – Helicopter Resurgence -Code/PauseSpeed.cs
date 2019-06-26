using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseSpeed : MonoBehaviour {

    // Pauses Player from moving forward
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Collision only for Player
        if (collision.gameObject.tag != "Player") return;
        PlayerController._player.PauseX = true;             // Freeze Player
        GetComponent<Collider2D>().enabled = false;
    }
}
