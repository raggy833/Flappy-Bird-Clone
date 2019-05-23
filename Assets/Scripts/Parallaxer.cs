﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallaxer : MonoBehaviour {

    // We are not going to be instantiating or destroying
    // so we determine the number of objects and
    // see if they are in use or not

	class PoolObject {
        public Transform transform;
        public bool inUse;
        public PoolObject(Transform t) { transform = t; }
        public void Use() { inUse = true; }
        public void Dispose() { inUse = false; }

    }
    [System.Serializable]
    public struct YSpawnRange {
        public float min;
        public float max;
    }

    public GameObject Prefab;
    public int poolSize;
    public float shiftSpeed;
    public float spawnRate;

    public YSpawnRange ySpawnRange;
    public Vector3 defaultSpawnPos;
    public bool spawnImmediate; // particle prewarm
    public Vector3 immediateSpawnPos;
    public Vector2 targetAspectRatio;

    float spawnTimer;
    float targetAspect;
    PoolObject[] poolObjects;
    GameManager gameManager;

    private void Awake() {
        Configure();
    }

    private void Start() {
        gameManager = GameManager.Instance;

    }

    private void OnEnable() {
        GameManager.OnGameOverConfirmed += OnGameOverConfirmed;
    }

    private void OnDisable() {
        GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;
    }

    void OnGameOverConfirmed() {
        for (int i = 0; i < poolObjects.Length; i++) {
            poolObjects[i].Dispose();
            poolObjects[i].transform.position = Vector3.one * 1000;
        }
        if (spawnImmediate) {
            SpawnImmediate();
        }
    }

    private void Update() {
        if (gameManager.GameOver) return;
        Shift();
        spawnTimer += Time.deltaTime;
        if(spawnTimer > spawnRate) {
            Spawn();
            spawnTimer = 0;
        }
    }

    void Configure() {
        targetAspect = targetAspectRatio.x / targetAspectRatio.y;
        poolObjects = new PoolObject[poolSize];
        for (int i = 0; i < poolObjects.Length; i++) {
            GameObject go = Instantiate(Prefab) as GameObject;
            Transform t = go.transform;
            t.SetParent(transform);
            t.position = Vector3.one * 1000;
            poolObjects[i] = new PoolObject(t);
        }
        if (spawnImmediate) {
            SpawnImmediate();
        }
    }

    void Spawn() {
        Transform t = GetPoolObject();
        if (t == null) return; // if true, this indicated that poolSize is too small
        Vector3 pos = Vector3.zero;
        pos.x = (defaultSpawnPos.x * Camera.main.aspect) / targetAspect;
        pos.y = Random.Range(ySpawnRange.min, ySpawnRange.max);
        t.position = pos;
    }

    void SpawnImmediate() {
        Transform t = GetPoolObject();
        if (t == null) return; // if true, this indicated that poolSize is too small
        Vector3 pos = Vector3.zero;
        pos.x = (immediateSpawnPos.x * Camera.main.aspect) / targetAspect;
        pos.y = Random.Range(ySpawnRange.min, ySpawnRange.max);
        t.position = pos;
        Spawn();
    }

    void Shift() {
        for (int i = 0; i < poolObjects.Length; i++) {
            poolObjects[i].transform.position += Vector3.left * shiftSpeed * Time.deltaTime;
            CheckDisposeObject(poolObjects[i]);
        }
    }

    void CheckDisposeObject(PoolObject poolObject) {
        if(poolObject.transform.position.x < (-defaultSpawnPos.x * Camera.main.aspect) / targetAspect) {
            System.Console.WriteLine(poolObject.transform.position.x);
            System.Console.WriteLine(-defaultSpawnPos.x);
            poolObject.Dispose();
            poolObject.transform.position = Vector3.one * 1000;
        }
    }

    Transform GetPoolObject(){
        for (int i = 0; i < poolObjects.Length; i++) {
            if (!poolObjects[i].inUse) {
                poolObjects[i].Use();
                return poolObjects[i].transform;
            }
        }
        return null;
    }

}