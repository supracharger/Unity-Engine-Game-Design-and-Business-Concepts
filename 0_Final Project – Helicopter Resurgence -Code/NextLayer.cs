using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

public class NextLayer : MonoBehaviour {
    public GameObject NextRespawn, StopCollider;        // NextRespawn Location, StopColider:Barrier once gone to next layer
    public GameObject DisableCoins, EnableCoins;        // Coins to Enable/ Disable when moving to a layer
    public bool Final = false;                          // If Final Layer of level, used in level 2
    public int Direction = -1;                          // Direction of Layer: 1:RightForward

    bool once;
    // Use this for initialization
    private void Awake()
    {
        // Disable Barrier to allow Player to Pass through
        StopCollider.GetComponent<Collider2D>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Exit: if collision was Not the Player or collision happend a 2nd time
        if (once || collision.gameObject.tag != "Player") return;
        once = true;        // Flag for a one time event
        // Disable Trigger to only happend once
        GetComponent<Collider2D>().enabled = false;
        // Enable Barrier to Stop Player from backtracking
        StopCollider.GetComponent<Collider2D>().enabled = true;
        // Flips the Entire World so helicopter goes the other direction
        PlayerController._player.FlipWorld(Direction, NextRespawn, !Final);
        // Move Camera for Next Layer
        Camera2DFollow.Follow.plusOnX *= (!Final) ? -1 : 0;
        // Disable Coins on Prev Layer
        DisableCoins.SetActive(false);
        // Enable Coins on Future Layer
        if (EnableCoins) EnableCoins.SetActive(true);
        // Final Layer goes North to South, instead of East to West. So Movable axis is on 'Y'
        //      instead of 'X'
        if (Final)
        {
            // Switch Axis
            PlayerController._player.AllAxis = true;
            // No Enemy Spawning on Final Layer
            PlayerController._player.SpawnEnemyObject.GetComponent<SpawnEnemy>().Active = false;
        }
    }
}
