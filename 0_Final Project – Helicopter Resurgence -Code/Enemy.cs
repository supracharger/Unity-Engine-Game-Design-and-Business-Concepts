using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    [HideInInspector]
    public float Speed;         // Speed of the Enemy

	// Update is called once per frame
	void Update ()
    {
        // Speed of Enemy. Speed will be the same across all platforms
        var direction = new Vector3(-Speed, 0f);
        // Same Speed Across all platforms
        gameObject.transform.Translate(direction * Time.deltaTime);
    }

    // Rotate enemy to go in the opposite direction
    // rotate: The amount to rotate the enemy. Mostlikly 180
    public void RotateEnemy(float rotate)
    {
        gameObject.transform.Rotate(Vector3.up, rotate);
    }

    // TimeOut to Destroy enemy to free Memory
    // timeOut: time needed for active enemy to be displayed to user
    public void DestroyTimeOut(float timeOut)
    {
        // invote the DestroyNow funtion to run after timeOut seconds
        Invoke("DestroyNow", timeOut);
    }
}
