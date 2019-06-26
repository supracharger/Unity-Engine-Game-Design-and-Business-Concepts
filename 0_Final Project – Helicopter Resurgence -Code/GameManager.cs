using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    // Unity Inputs
    public GameObject Player;                       // Player GameObject
    public GameObject Parent;                       // Parent of the World
    public Text EndGameText;
    public AudioClip GameWinSFX, GameLoseSFX;       // Sound Effects
    public Text Score;
    public string LevelAfterVictory;                // Next Level or if Empty: end of game
    public GameObject[] lifeObjs;                   // GameObjects that show how many lives left

    int lives;                              // Lives Left
    public static GameManager gm;           // Game Manager Obj to be referenced in other Scripts
    [HideInInspector]
    int score;                              // Running Score of player across all levels
    PlayerController _playerCtrl;           // PlayerController Obj. used in CollectCoin()

    // Use this for initialization
    private void Awake()
    {
        gm = this;                                                  // Save Game Manager to be Referenced
        lives = lifeObjs.Length;                                    // Lives Left
        EndGameText.text = string.Empty;                            // Clear EndGame Text
        _playerCtrl = Player.GetComponent<PlayerController>();      // PlayerController Obj. used in CollectCoin()
        // Load Prefs
        // Get Current Level & Have accumulated Score over Levels
        // Level 10 == GameEnd or Game Victory
        int level = (LevelAfterVictory.Trim() == string.Empty) ? 10 : int.Parse(LevelAfterVictory.Trim().Split(' ')[1]) - 1;
        if (level > 1) score = GetPrefs();
        // Display Score
        Score.text = "Score: " + score.ToString();
    }

    // Update is called once per frame
    //void Update () { }

    // Player Died Coroutine 
    // wait: time to wait to continue
    public IEnumerator Death(float wait)
    {
        // Remove Life
        lives--;
        // NO Lives left ........................
        if (lives < 0) // Game Over
        {
            // Display GameOver Text
            EndGameText.text = "GAME OVER";
            // Play GameOver Audio
            var audio = GetComponent<AudioSource>();
            audio.clip = GameLoseSFX;
            audio.Play();
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);   // Reload
        }
        // Lives Remaining ......................
        else
        {
            // Wait Amount of time to Reload Level
            yield return new WaitForSeconds(wait);
            // Have number of live icons equal to the number of lives left
            lifeObjs.Select((g, i) => { g.SetActive(i < lives); return true; }).ToArray();
            // Respawn Player
            Player.GetComponent<PlayerController>().Respawn();
        }
    }

    // Player Level Victory
    // wait: time to wait to continue
    public IEnumerator Victory(float wait)
    {
        // Bool: is there a next level
        bool nextLevel = LevelAfterVictory.Trim() != string.Empty;
        // Victory Message of Next Level or Game Victory
        string levelMsg = "On to " + LevelAfterVictory.Trim();
        EndGameText.text = (nextLevel) ? levelMsg : "Victory!";
        // Play Victory Audio
        var audio = GetComponent<AudioSource>();
        audio.clip = GameWinSFX;
        audio.Play();
        // Wait to Load
        yield return new WaitForSeconds(wait);
        // Load Next Level if there is one
        if (nextLevel)
        {
            SavePrefs(score);                               // Save Preferences like score
            SceneManager.LoadScene(LevelAfterVictory);      // Load next level
        }
    }

    // Collects Coin: increases score & changes speed according to CoinSpeedCollect()
    public void CollectCoin(int coinType)
    {
        score++;                                        // Update Score
        Score.text = "Score: " + score.ToString();      // Update Score Display
        _playerCtrl.CoinSpeedCollect(coinType);         // Change Speed according to the CoinType
    }

    // Saves Score
    public void SavePrefs(int score)
    {
        PlayerPrefs.SetInt("Score", score);
    }

    // Get Saved Score
    public int GetPrefs()
    {
        if (!PlayerPrefs.HasKey("Score")) return 0;     // No Score, score = 0
        return PlayerPrefs.GetInt("Score");
    }
}
