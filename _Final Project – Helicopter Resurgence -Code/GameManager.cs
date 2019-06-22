using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public GameObject Player;
    public GameObject Parent;
    public Text EndGameText;
    public AudioClip GameWinSFX, GameLoseSFX;
    public Text Score;
    public string LevelAfterVictory;
    public GameObject[] lifeObjs;

    int lives;
    public static GameManager gm;
    [HideInInspector]
    int score;
    PlayerController _playerCtrl;

    // Use this for initialization
    private void Awake()
    {
        gm = this;
        lives = lifeObjs.Length;
        EndGameText.text = string.Empty;
        _playerCtrl = Player.GetComponent<PlayerController>();
        // Load Prefs
        int level = (LevelAfterVictory.Trim() == string.Empty) ? 10 : int.Parse(LevelAfterVictory.Trim().Split(' ')[1]) - 1;
        if (level > 1) score = GetPrefs();
        //
        Score.text = "Score: " + score.ToString();
    }

    // Update is called once per frame
    //void Update () { }

    public IEnumerator Death(float wait)
    {
        lives--;
        if (lives < 0) // Game Over
        {
            EndGameText.text = "GAME OVER";
            var audio = GetComponent<AudioSource>();
            audio.clip = GameLoseSFX;
            audio.Play();
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);   // Reload
        }
        else
        {
            yield return new WaitForSeconds(wait);
            lifeObjs.Select((g, i) => { g.SetActive(i < lives); return true; }).ToArray();
            // Respawn
            Player.GetComponent<PlayerController>().Respawn();
        }
    }

    public IEnumerator Victory(float wait)
    {
        bool nextLevel = LevelAfterVictory.Trim() != string.Empty;
        string levelMsg = "On to " + LevelAfterVictory.Trim();
        EndGameText.text = (nextLevel) ? levelMsg : "Victory!";
        var audio = GetComponent<AudioSource>();
        audio.clip = GameWinSFX;
        audio.Play();
        yield return new WaitForSeconds(wait);
        if (nextLevel)
        {
            SavePrefs(score);
            SceneManager.LoadScene(LevelAfterVictory);
        }
    }

    public void CollectCoin(int coinType)
    {
        score++;
        Score.text = "Score: " + score.ToString();
        _playerCtrl.CoinSpeedCollect(coinType);
    }

    public void SavePrefs(int score)
    {
        PlayerPrefs.SetInt("Score", score);
    }

    public int GetPrefs()
    {
        if (!PlayerPrefs.HasKey("Score")) return 0;
        return PlayerPrefs.GetInt("Score");
    }
}
