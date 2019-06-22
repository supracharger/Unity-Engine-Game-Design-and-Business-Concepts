using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

public class NextLayer : MonoBehaviour {
    public GameObject NextRespawn, StopCollider;
    public GameObject DisableCoins, EnableCoins;
    public bool Final = false;
    public int Direction = -1;

    bool once;
    // Use this for initialization
    private void Awake()
    {
        StopCollider.GetComponent<Collider2D>().enabled = false;
    }
    //void Start () {}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (once || collision.gameObject.tag != "Player") return;
        once = true;
        GetComponent<Collider2D>().enabled = false;
        StopCollider.GetComponent<Collider2D>().enabled = true;
        PlayerController._player.FlipWorld(Direction, NextRespawn, !Final);
        // Camera
        Camera2DFollow.Follow.plusOnX *= (!Final) ? -1 : 0;
        // Coins
        DisableCoins.SetActive(false);
        if (EnableCoins) EnableCoins.SetActive(true);
        if (Final)
        {
            PlayerController._player.AllAxis = true;
            PlayerController._player.SpawnEnemyObject.GetComponent<SpawnEnemy>().Active = false;
        }
    }

    // Update is called once per frame
    //void Update () {}
}
