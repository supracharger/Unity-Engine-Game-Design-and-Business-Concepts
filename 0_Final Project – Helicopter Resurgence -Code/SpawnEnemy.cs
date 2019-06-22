using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour {

    public float StartTime = 15f, EndTime = 30f;
    public float EnemySpeed;
    public GameObject Radar;
    public float RadarWait = 10f;
    public int SpawnMax = 2;
    public AudioClip RadarWarningSFX;
    public GameObject MainParent;
    public GameObject MapEnd;
    public GameObject[] MapEndNext;
    public GameObject[] Spawns;
    public GameObject[] Enemies;

    [HideInInspector]
    public bool Active = true;
    [HideInInspector]
    public int _spawnDir = 1;
    static System.Random _rand = new System.Random();
    float _time;
    GameObject _player;
    Dictionary<string, RadarHandle> _RadarAnimator;
    AudioSource _radarAudio;
    float _radarNoticeSpan;
    Vector3 prevPlayer;

    // Use this for initialization
    private void Awake()
    {
        _time = NextTimeSpawn() + 6f;
        _radarNoticeSpan = -1;          // Want radar sound to play on 1st run
        // Radar Notice
        _RadarAnimator = new Dictionary<string, RadarHandle>();
        _RadarAnimator["Laser"] = new RadarHandle(Radar.transform.Find("Laser").gameObject.GetComponent<Animator>());
        _RadarAnimator["Missle"] = new RadarHandle(Radar.transform.Find("Missle").gameObject.GetComponent<Animator>());
        _RadarAnimator["Jet"] = new RadarHandle(Radar.transform.Find("Jet").gameObject.GetComponent<Animator>());
        _radarAudio = Radar.GetComponent<AudioSource>();
        //
        if (SpawnMax > Spawns.Length) throw new Exception("ERROR!: SpawnMax > Spawns.Length");
    }

    // Use this for initialization
    void Start ()
    {
        _player = GameManager.gm.Player;
        // Spawn Deviation in X from player
        //_spawnDiff = gameObject.transform.position.x - _player.transform.position.x;
        prevPlayer = _player.transform.position;
    }

    // Update is called once per frame
    void Update ()
    {
        // Time Interval
        _time -= Time.deltaTime;
        _radarNoticeSpan -= Time.deltaTime;
        if (_time > 0f) return;
        _time = NextTimeSpawn();

        // Past End
        bool pastEnd = (MapEnd.transform.position.x - gameObject.transform.position.x) * _spawnDir < 0;
        if (pastEnd || !Active) return;

        // Move Spawn objects in front of player
        var vec = new Vector3(_player.transform.position.x - prevPlayer.x, 0f);
        prevPlayer = _player.transform.position;
        //var vec = new Vector3(_player.transform.position.x + _spawnDiff * _spawnDir, 0);// gameObject.transform.position.y);

        gameObject.transform.Translate(vec, MapEnd.transform);
        // Randomly select Spawn Objs, either 1 object or 2
        var spawns = GetSpawnObj(_rand.Next(SpawnMax) + 1);
        // Choose Enemy Type
        var enemyType = Enemies[_rand.Next(Enemies.Length)];
        string name = enemyType.name.Replace("Enemy_", "");
        // Spawn Selected Enemies
        for (int i = 0; i < spawns.Length; i++)
        {
            var trans = spawns[i].transform;
            // Spawn/ create a new gameObject
            GameObject enemy = Instantiate(enemyType, trans.position, trans.rotation) as GameObject;
            if (MainParent) enemy.transform.SetParent(MainParent.transform);        // Set Parent to move with total rotation
            Enemy script = enemy.GetComponent<Enemy>();                             // Enemy Script
            script.Speed = EnemySpeed;                                              // Enemy Speed
            if (_spawnDir < 0) script.RotateEnemy(180f);                            // Spawn Direction
            script.DestroyTimeOut(60f);                                             // 1min. Destroy Timeout
        }
        // Radar
        StartCoroutine(RadarSignal(_RadarAnimator[name], 7));
        // Radar Notice SFX
        if (_radarNoticeSpan <= 0)
        { if (RadarWarningSFX) _radarAudio.PlayOneShot(RadarWarningSFX); _radarNoticeSpan = RadarWait; }
	}

    GameObject[] GetSpawnObj(int nSpawn)
    {
        if (nSpawn < 1) throw new Exception();
        // Randomly select Spawn Objs, either 1 object or 2
        var spawns = new GameObject[nSpawn];
        List<int> indexs = new List<int>();
        for (int i = 0; i < nSpawn; i++)
        {
            // Get a unique index for each spawn
            int idx;
            do { idx = _rand.Next(Spawns.Length); }
            while (indexs.Contains(idx));
            // Save unique
            indexs.Add(idx);
            spawns[i] = Spawns[idx];
        }
        return spawns;
    }

    float NextTimeSpawn() { return StartTime + (float)_rand.NextDouble() * (EndTime - StartTime); }

    int nextEndI = 0;
    public void FlipWorld(int direction)
    {
        int prevDir = _spawnDir;
        gameObject.transform.Translate(new Vector3(_player.transform.position.x - prevPlayer.x, 0f), MapEnd.transform);
        var next = MapEndNext[nextEndI++].transform.position;
        var prev = MapEnd.transform.position;
        MapEnd.transform.position = next;
        MapEnd.transform.rotation = new Quaternion();
        // Reset Enemy Spawner
        float distance = Math.Abs(gameObject.transform.position.x - _player.transform.position.x);
        prevPlayer = _player.transform.position;
        gameObject.transform.position = next;
        //gameObject.transform.position += Vector3.up * (1 + next.y - prev.y);
        float diff = (_player.transform.position.x - next.x) - distance * prevDir;
        gameObject.transform.Translate(new Vector3(diff, 0), MapEnd.transform);
        // Set Direction
        _spawnDir = direction;
    }

    IEnumerator RadarSignal(RadarHandle radar, float timeout)
    {
        // Start Radar Anim
        radar._occur++;
        radar._Animator.SetBool("Active", true);
        // Wait Desired Radar Signal time
        yield return new WaitForSeconds(timeout);
        // Turn of radar if there is no more of that enemy existing of the game
        radar._occur--;
        if (radar._occur < 0) radar._occur = 0;
        if (radar._occur == 0) radar._Animator.SetBool("Active", false);
    }

    class RadarHandle
    {
        public Animator _Animator;
        public int _occur
        {
            get { return _occurance; }
            set { lock (locker) { _occurance = value; } }
        }
        int _occurance;
        object locker = new object();
        public RadarHandle(Animator animator)
        {
            _Animator = animator;
        }

    }
}
