using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 1f;
    public float XSpeed = 1f;
    public float XSpeedPlus = 4f;
    public int SpeedInc = 5;
    public int Direct = 1;
    public float gravityForce = 0.1f;
    public GameObject SpawnLocation;
    public GameObject SpawnEnemyObject;
    public AudioClip DeathSFX, FastSFX, SlowSFX;

    public static PlayerController _player;
    Animator _animator, _animFastSlow;
    AudioSource _audio, _audioFastSlow;
    bool _died, _freezeMotion;
    int _speedPlus;
    [HideInInspector]
    public bool PauseX, PauseAll;
    [HideInInspector]
    public bool AllAxis;

    // Use this for initialization
    private void Awake()
    {
        _player = this;
        PauseAll = true;
        _player.transform.position = SpawnLocation.transform.position;
        //_rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
        // SlowFast Icon Components
        GameObject SlowFast = gameObject.transform.Find("SlowFast Icon").gameObject;
        _animFastSlow = SlowFast.GetComponent<Animator>();
        _audioFastSlow = SlowFast.GetComponent<AudioSource>();
    }

    // Use this for initialization
    //void Start () {}

    // Update is called once per frame
    void Update () {
        // IF Freezemotion
        if (_freezeMotion) return;

        // Get Controller Input
        float ix = Input.GetAxisRaw("Horizontal");
        float iy = Input.GetAxisRaw("Vertical");
        float speedplus = XSpeedPlus * SpeedPct();
        float x = ((ix * Direct) < 0) ? -1 : XSpeed + speedplus;
        Vector3 direction = new Vector3((PauseX) ? 0 : x, 
                                        iy * speed - gravityForce);
        if (AllAxis) direction = new Vector3(ix * (speed + XSpeedPlus * 0.50f), 
                                            iy * (speed + XSpeedPlus * 0.50f) - gravityForce);
        // Pause All until Movement
        if (PauseAll)
        { if (ix != 0 || iy != 0) PauseAll = false; else direction = Vector3.zero; }
        // Same Speed Across all platforms
        gameObject.transform.Translate(direction * Time.deltaTime);

        // Gavity Force
        //_rigidBody.AddForce(new Vector2(0, gravityForce));// * Time.deltaTime));
    }

    // On Collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Reset Rotation
        //gameObject.transform.rotation = HeliRotation;
        gameObject.transform.rotation = SpawnLocation.transform.rotation;
        // If Hit Obsticle
        if (!_died && (collision.gameObject.tag=="Obsticle" || collision.gameObject.tag =="Enemy"))
        {
            _freezeMotion = _died = true;
            _animator.SetTrigger("Died");
            _audio.PlayOneShot(DeathSFX);
            _speedPlus = 0;
            // Enemy
            if (collision.gameObject.tag == "Enemy")
            {
                //collision.gameObject.transform.DetachChildren();    // Doesn't Destroy RadarLine
                UnityEngine.Object.Destroy(collision.gameObject);
            }
            // Death Coroutine
            StartCoroutine(GameManager.gm.Death(3f));
        }
        // If Hit the Helipad: Victory
        if (!_died && collision.gameObject.tag == "HeliPad")
        {
            _freezeMotion = true;
            //_animator.SetTrigger("Victory");
            StartCoroutine(GameManager.gm.Victory(5f));
        }
    }

    float _prevPct;
    public void CoinSpeedCollect(int coinType)
    {
        float pct;
        // Inc & Limit Range
        if (coinType > 0)
        {
            _speedPlus = Math.Min(_speedPlus + 1, SpeedInc);
            // Speed Animator
            pct = SpeedPct();
            if (pct >= 0.70f && _prevPct < 0.70f)
            { _animFastSlow.SetTrigger("Fast"); _audioFastSlow.PlayOneShot(FastSFX); }
        }
        else if (coinType < 0)
        {
            _speedPlus = Math.Max(_speedPlus - 1, -SpeedInc);
            // Speed Animator
            pct = SpeedPct();
            if (pct <= 0.30f && _prevPct > 0.30f)
            { _animFastSlow.SetTrigger("Slow"); _audioFastSlow.PlayOneShot(SlowSFX); }
        }
        else throw new Exception();
        // Save Prev.
        _prevPct = pct;
    }

    public void Respawn()
    {
        this.gameObject.transform.position = SpawnLocation.transform.position;
        this.gameObject.transform.Rotate(Vector3.up, 0f);
        _freezeMotion = _died = false;
        PauseAll = true;
        _animator.SetTrigger("Respawn");
        _speedPlus = 0;
    }

    public void FlipWorld(int direction, GameObject respawnLocation, bool flipHelicopter = true)
    {
        // Flip Helicopter
        if (flipHelicopter) gameObject.transform.Rotate(Vector3.up, 180);
        // 
        SpawnLocation = respawnLocation;
        //gameObject.transform.rotation = HeliRotation;
        //
        Direct = direction;
        PauseX = false;
        //
        SpawnEnemyObject.GetComponent<SpawnEnemy>().FlipWorld(direction);
    }

    // Speed Percent: Percent of Speed plus to use
    float SpeedPct() { return (_speedPlus + SpeedInc) / (float)(2 * SpeedInc); }
}
