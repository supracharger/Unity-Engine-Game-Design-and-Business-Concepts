using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour {

    public float StartTime = 15f, EndTime = 30f;    // Interval span for when the radar is triggered
    public float EnemySpeed;                        // Speed of the Enemy
    public GameObject Radar;                        // Parent obj of the Radar Display
    public float RadarWait = 10f;
    public int SpawnMax = 2;                        // Max number of Enemies to Spawn at a time
    public AudioClip RadarWarningSFX;               // Radar Warning Sound
    public GameObject MainParent;
    public GameObject MapEnd;                       // End of the map game object
    public GameObject[] MapEndNext;                 // Next Layers Map End Location
    public GameObject[] Spawns;                     // All possible Enemy spawn locations
    public GameObject[] Enemies;                    // The different enemies to spawn

    [HideInInspector]
    public bool Active = true;                      // If Spawn Enemy is Active
    [HideInInspector]
    public int _spawnDir = 1;                       // Direction to Send the Enemies: 1:right Forward
    static System.Random _rand = new System.Random();   // Random Object
    float _time;                                    // Time left
    GameObject _player;                             // Player GameObject
    Dictionary<string, RadarHandle> _RadarAnimator; // Dictionary to Hold Radar info of that Enemy type
    AudioSource _radarAudio;                        // AudioSource for Radar
    float _radarNoticeSpan;                         // Interval of how much the radar SoundEffect is played
    Vector3 prevPlayer;                             // Previous Player Position

    // Use this for initialization
    private void Awake()
    {
        _time = NextTimeSpawn() + 6f;   // Time Till Next Enemies are Spawned
        _radarNoticeSpan = -1;          // Want radar sound to play on 1st run
        // Dictionary to Hold Radar info of that Enemy type
        _RadarAnimator = new Dictionary<string, RadarHandle>();
        _RadarAnimator["Laser"] = new RadarHandle(Radar.transform.Find("Laser").gameObject.GetComponent<Animator>());
        _RadarAnimator["Missle"] = new RadarHandle(Radar.transform.Find("Missle").gameObject.GetComponent<Animator>());
        _RadarAnimator["Jet"] = new RadarHandle(Radar.transform.Find("Jet").gameObject.GetComponent<Animator>());
        // Radar Audio Source
        _radarAudio = Radar.GetComponent<AudioSource>();
        // Not enouph spawn locations that SpawnMax requests
        if (SpawnMax > Spawns.Length) throw new Exception("ERROR!: Not enouph spawn locations that SpawnMax requests");
    }

    // Use this for initialization
    void Start ()
    {
        // Get Player Game Obj.
        _player = GameManager.gm.Player;
        // Save Player Position: to use as ref to the delta of how much it moved
        prevPlayer = _player.transform.position;
    }

    // Update is called once per frame
    void Update ()
    {
        // Waits until time interval has been reached
        _time -= Time.deltaTime;
        _radarNoticeSpan -= Time.deltaTime;
        if (_time > 0f) return;                 // Exit if time interval Not reached
        // Reset Time to Wait for next cycle
        _time = NextTimeSpawn();

        // bool If EnemySpawner passed the end of the Layer
        bool pastEnd = (MapEnd.transform.position.x - gameObject.transform.position.x) * _spawnDir < 0;
        // If EnemySpawner passed the end of the Layer or is Not Active:
        //      if either is true: do Not spawn any Enemies
        if (pastEnd || !Active) return;

        // Keep & Move Spawn objects continuously in front of player
        // vec: of how much to move EnemySpawner according to the X Axis
        var vec = new Vector3(_player.transform.position.x - prevPlayer.x, 0f);
        prevPlayer = _player.transform.position;
        
        // Translate EnemySpawner always a set distance in front of the player
        gameObject.transform.Translate(vec, MapEnd.transform);
        // Randomly select num. of Enemies to Spawn, Limited by SpawnMax
        var spawns = GetSpawnObj(_rand.Next(SpawnMax) + 1);
        // Choose Enemy Type Randomly
        var enemyType = Enemies[_rand.Next(Enemies.Length)];
        string name = enemyType.name.Replace("Enemy_", "");
        // Spawn each Selected Enemy
        for (int i = 0; i < spawns.Length; i++)
        {
            // SpawnLocation: Position & Rotation
            var trans = spawns[i].transform;
            // Spawn/ create a new Enemy gameObject
            GameObject enemy = Instantiate(enemyType, trans.position, trans.rotation) as GameObject;
            if (MainParent) enemy.transform.SetParent(MainParent.transform);        // Set Parent to move with total Game rotation
            Enemy script = enemy.GetComponent<Enemy>();                             // Get Enemy Script
            script.Speed = EnemySpeed;                                              // Set Enemy Speed
            if (_spawnDir < 0) script.RotateEnemy(180f);                            // Filp Spawn Direction if world is Flipped
            script.DestroyTimeOut(60f);                                             // 1min. Destroy Timeout
        }
        // Radar that detects the Specific Enemy ..............................
        StartCoroutine(RadarSignal(_RadarAnimator[name], 7));
        // Radar Notice SFX: Sound Effect
        if (_radarNoticeSpan <= 0)
        { if (RadarWarningSFX) _radarAudio.PlayOneShot(RadarWarningSFX); _radarNoticeSpan = RadarWait; }
	}

    // Gets a Spawn Location Randomly for each respective Enemy
    // nSpawn: num. of Spawn locations needed
    GameObject[] GetSpawnObj(int nSpawn)
    {
        if (nSpawn < 1) throw new Exception();      // Invalid Spawn Number
        // Array of where the Enemies will Spawn
        var spawns = new GameObject[nSpawn];
        // Indexs to check to make sure each spawnIndex is unique
        List<int> indexs = new List<int>();
        // Get a unique spawn location for each of the Enemies Spawned
        // Note!: Not all of the Spawn Locations in 'Spawns' are used
        // Enemies are spawned using different Spawn locations
        for (int i = 0; i < nSpawn; i++)
        {
            // Get a unique spawn index for each spawn
            int idx;
            do { idx = _rand.Next(Spawns.Length); }
            while (indexs.Contains(idx));
            // Save unique Spawn Location to use
            indexs.Add(idx);
            spawns[i] = Spawns[idx];
        }
        // Return Spawn Locations
        return spawns;
    }
    
    // Random between TimeFrame Interval when Enemies are Deployed
    float NextTimeSpawn() { return StartTime + (float)_rand.NextDouble() * (EndTime - StartTime); }

    // Flips the EnemySpawner to the next Layer
    // direction: 1: right forward
    int nextEndI = 0;
    public void FlipWorld(int direction)
    {
        int prevDir = _spawnDir;        // Prev. World Direction
        // Move EnemySpawner the correct distance on the X Axis
        gameObject.transform.Translate(new Vector3(_player.transform.position.x - prevPlayer.x, 0f), MapEnd.transform);
        var next = MapEndNext[nextEndI++].transform.position;       // Postion of Next Layer MapEnd Location
        var prev = MapEnd.transform.position;                       // Position of Prev MapEnd
        MapEnd.transform.position = next;                           // Move MapEnd Obj. to next Location
        MapEnd.transform.rotation = new Quaternion();               // Reset Rotation on MapEnd
        // Reset Enemy Spawner .......................
        // Spawner has to be translated like this because in level 2 the whole world rotates & this 
        //      is the only found way to change this within the world.
        float distance = Math.Abs(gameObject.transform.position.x - _player.transform.position.x);      // Correct Distance from Player to EnemySpawner
        prevPlayer = _player.transform.position;        // Save Prev. player pos. for next time around
        gameObject.transform.position = next;           // Move EnemySpawner to MapEnd
        float diff = (_player.transform.position.x - next.x) - distance * prevDir;      // Distance to Translate to have Correct Distance from Player to EnemySpawner
        gameObject.transform.Translate(new Vector3(diff, 0), MapEnd.transform);         // Translate for Correct Distance
        // Set Direction
        _spawnDir = direction;                          // Save prev. spawn direction for the next time around
    }

    // Activates Radar for Specific Enemy
    // radar: obj of RadarHandle for the specific Enemy 
    // timeout: how long the notice should last
    IEnumerator RadarSignal(RadarHandle radar, float timeout)
    {
        // Activates Radar for Specific Enemy
        // Start Radar Anim
        radar._occur++;     // increment the number of sets of the same type of enemy
        radar._Animator.SetBool("Active", true);
        // Wait Desired Radar Signal time
        yield return new WaitForSeconds(timeout);
        radar._occur--;     // decrement the number of sets of the same type of enemy
        // Turn off radar: if there is no more of that enemy existing of the game
        if (radar._occur < 0) radar._occur = 0;
        if (radar._occur == 0) radar._Animator.SetBool("Active", false);
    }

    // Obj that holds the radar Animator & the number of enemy sets that exists in the game
    class RadarHandle
    {
        public Animator _Animator;      // Animator of Enemy for specific Radar
        public int _occur               // The number of enemy sets that exists in the game
        {
            get { return _occurance; }
            set { lock (locker) { _occurance = value; } }
        }
        int _occurance;                 // _occurance of this sets of enemies in the game
        object locker = new object();   // empty obj to lock the thread for the _occurance variable
        public RadarHandle(Animator animator)
        {
            _Animator = animator;
        }

    }
}
