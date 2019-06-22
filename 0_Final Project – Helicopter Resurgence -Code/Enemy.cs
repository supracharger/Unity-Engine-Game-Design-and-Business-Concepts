using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    [HideInInspector]
    public float Speed;

	// Use this for initialization
	//void Start () {}
	
	// Update is called once per frame
	void Update ()
    {
        var direction = new Vector3(-Speed, 0f);
        // Same Speed Across all platforms
        gameObject.transform.Translate(direction * Time.deltaTime);
    }

    public void RotateEnemy(float rotate)
    {
        gameObject.transform.Rotate(Vector3.up, rotate);
    }

    public void DestroyTimeOut(float timeOut)
    {
        // invote the DestroyNow funtion to run after timeOut seconds
        Invoke("DestroyNow", timeOut);
    }
}
