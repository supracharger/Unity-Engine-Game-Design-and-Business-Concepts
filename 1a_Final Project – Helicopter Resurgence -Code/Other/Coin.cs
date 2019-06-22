using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public int CoinType = 1;

    // Use this for initialization
    //void Start() { }
    // Update is called once per frame
    //void Update() { }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag != "Player") return;   // Exit
        GameManager.gm.CollectCoin(CoinType);
        Object.Destroy(gameObject);
    }
}
