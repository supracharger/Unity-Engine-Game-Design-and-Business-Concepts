using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PromptTransfer : MonoBehaviour {
    public string NextLevel;
    public float IntroTimeOut = 3f;
    public GameObject[] Scenes;

    bool _onKey;

    // Use this for initialization
    void Start ()
    {
        StartCoroutine(TimedNext(IntroTimeOut));
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!_onKey) return;

        if (Input.GetButtonDown("Submit"))
            SceneManager.LoadScene(NextLevel);
	}

    IEnumerator TimedNext(float timeout)
    {
        Scenes[0].SetActive(true);
        Scenes[1].SetActive(false);
        yield return new WaitForSeconds(timeout);
        Scenes[0].SetActive(false);
        Scenes[1].SetActive(true);
        _onKey = true;
    }
}
