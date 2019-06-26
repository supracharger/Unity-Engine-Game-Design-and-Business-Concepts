using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 1f;                        // Min/ Starting Speed
    public float XSpeed = 1f;                       // Forward Speed
    public float XSpeedPlus = 4f;                   // Max Speed
    public int SpeedInc = 5;                        // Max Coins to collect to hit Max Speed
    public int Direct = 1;                          // Direction of Helicopter
    public float gravityForce = 0.1f;               // The Force of Gravity to overcome
    public GameObject SpawnLocation;                // Player Respawn Location
    public GameObject SpawnEnemyObject;             // Moving Spawner for enemy spawns
    public AudioClip DeathSFX, FastSFX, SlowSFX;    // Event Sound Effects

    public static PlayerController _player;         // Static PlayerController Reference obj
    Animator _animator, _animFastSlow;              // Player Animation
    AudioSource _audio, _audioFastSlow;             // AudioSource for Slow Fast Animation
    bool _died, _freezeMotion;                      // Flag if Died or FreezeMotion
    int _speedPlus;                                 // value of how much to speed up or slow down player
    [HideInInspector]
    public bool PauseX, PauseAll;                   // Pause Motion of certian axis
    [HideInInspector]
    public bool AllAxis;                            // Used for Final Layer

    // Use this for initialization
    private void Awake()
    {
        _player = this;                                                 // PlayerController Game Object
        PauseAll = true;                                                // Player Pause
        _player.transform.position = SpawnLocation.transform.position;  // Move Player to Respawn Location
        // Player Animator & AudioSource
        _animator = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
        // SlowFast Icon Components
        // Shows user if Player is going really Fast or really Slow
        GameObject SlowFast = gameObject.transform.Find("SlowFast Icon").gameObject;
        _animFastSlow = SlowFast.GetComponent<Animator>();
        _audioFastSlow = SlowFast.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update () {
        // IF Freezemotion
        if (_freezeMotion) return;

        // Get Controller Input
        float ix = Input.GetAxisRaw("Horizontal");                  // Horizontal Input
        float iy = Input.GetAxisRaw("Vertical");                    // Vertical Input
        float speedplus = XSpeedPlus * SpeedPct();                  // Percent of MaxSpeed Controlled by the number of Coins the player has
        float x = ((ix * Direct) < 0) ? -1 : XSpeed + speedplus;    // Movement of X axis: Brakes/ Reverse of Helicopter; or Forward Speed
        Vector3 direction = new Vector3((PauseX) ? 0 : x,           // Movement of Y axis: overcomming the gravity force
                                        iy * speed - gravityForce);
        // Allows all axis of Freedom
        // Used in Final Layer on Level 2
        if (AllAxis) direction = new Vector3(ix * (speed + XSpeedPlus * 0.50f), 
                                            iy * (speed + XSpeedPlus * 0.50f) - gravityForce);
        // Pause All until Movement from Input
        if (PauseAll)
        { if (ix != 0 || iy != 0) PauseAll = false; else direction = Vector3.zero; }
        // Same Speed Across all platforms
        gameObject.transform.Translate(direction * Time.deltaTime);
    }

    // On Collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Reset Rotation: Do not alow it to rotate
        gameObject.transform.rotation = SpawnLocation.transform.rotation;
        // If Hit Obsticle or Enemy
        if (!_died && (collision.gameObject.tag=="Obsticle" || collision.gameObject.tag =="Enemy"))
        {
            _freezeMotion = _died = true;   // Freeze Player & Set Flag for Died
            _animator.SetTrigger("Died");   // Play Animation Died
            _audio.PlayOneShot(DeathSFX);   // Play AudioSource Died
            _speedPlus = 0;                 // Reset to normal speed
            // Destroy Enemy GameObject
            if (collision.gameObject.tag == "Enemy")
                UnityEngine.Object.Destroy(collision.gameObject);
            // Death Coroutine
            StartCoroutine(GameManager.gm.Death(3f));
        }
        // If Hit the Helipad: Victory
        if (!_died && collision.gameObject.tag == "HeliPad")
        {
            _freezeMotion = true;                           // Freeze Player
            StartCoroutine(GameManager.gm.Victory(5f));     // Play Victory Coroutine
        }
    }

    // Change Speed & Play Animation according to coin
    // coinType: >0:Faster; <0:Slower
    float _prevPct;
    public void CoinSpeedCollect(int coinType)
    {
        float pct;
        // Inc & Limit Range
        if (coinType > 0)
        {
            _speedPlus = Math.Min(_speedPlus + 1, SpeedInc);    // Increment Speed as long as smaller than SpeedInc
            // Speed Animator
            pct = SpeedPct();                                   // Get Percent of Max/ Min Speed
            // Play Fast Animation: if greater than or equal to 70% speed
            if (pct >= 0.70f && _prevPct < 0.70f)
            { _animFastSlow.SetTrigger("Fast"); _audioFastSlow.PlayOneShot(FastSFX); }
        }
        else if (coinType < 0)
        {
            _speedPlus = Math.Max(_speedPlus - 1, -SpeedInc);   // decrement Speed as long as larger than -SpeedInc
            // Speed Animator
            pct = SpeedPct();                                   // Get Percent of Max/ Min Speed
            // Play Slow Animation: if smaller than or equal to 30% speed
            if (pct <= 0.30f && _prevPct > 0.30f)
            { _animFastSlow.SetTrigger("Slow"); _audioFastSlow.PlayOneShot(SlowSFX); }
        }
        else throw new Exception("Unknown Coin Type");
        // Save Prev. Speed Percent
        _prevPct = pct;
    }

    // Respawn Player
    public void Respawn()
    {
        this.gameObject.transform.position = SpawnLocation.transform.position;  // Respawn at Spawn Location
        this.gameObject.transform.Rotate(Vector3.up, 0f);                       // Reset Rotation
        _freezeMotion = _died = false;                                          // Reset to False: freezeMotion & died
        PauseAll = true;                                                        // Pause Motion to allow player to get ready
        _animator.SetTrigger("Respawn");                                        // Respawn Animation
        _speedPlus = 0;                                                         // Set to normal speed
    }

    // FlipsWorld from the right forward perspective
    // direction: 1:rightForward; -1:leftForward
    // respawnLocation: Player Respawn Location
    // flipHelicopter: if true Helicopter would be facing left
    public void FlipWorld(int direction, GameObject respawnLocation, bool flipHelicopter = true)
    {
        // Flip Helicopter 180 degree's
        if (flipHelicopter) gameObject.transform.Rotate(Vector3.up, 180);
        // Save Respawn Location
        SpawnLocation = respawnLocation;
        // Save direction & unPause X Axis
        Direct = direction;
        PauseX = false;
        // Flip Enemy Spawner Direction
        SpawnEnemyObject.GetComponent<SpawnEnemy>().FlipWorld(direction);
    }

    // Speed Percent: Percent of Max/Min of Max Speed
    float SpeedPct() { return (_speedPlus + SpeedInc) / (float)(2 * SpeedInc); }
}
