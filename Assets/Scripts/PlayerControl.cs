using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControl : MonoBehaviour {

    public delegate void PlayerDelegate();
    public static event PlayerDelegate OnPlayerDied;
    public static event PlayerDelegate OnPlayerScored;


    public float tapForce = 10f;
    public float tiltSmooth = 5f;
    public Vector3 startPos;

    Rigidbody2D rb;
    Quaternion downRotation;
    Quaternion forwardRotation;

    GameManager gameManager;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        downRotation = Quaternion.Euler(0, 0, -90);
        forwardRotation = Quaternion.Euler(0, 0, 35);
        gameManager = GameManager.Instance;
        rb.simulated = false;
    }

    private void OnEnable() {
        GameManager.OnGameStarted += OnGameStarted;
        GameManager.OnGameOverConfirmed += OnGameOverConfirmed;
    }

    private void OnDisable() {
        GameManager.OnGameStarted -= OnGameStarted;
        GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;
    }

    void OnGameStarted() {
        rb.velocity = Vector3.zero;
        rb.simulated = true;
    }
    void OnGameOverConfirmed() {
        transform.localPosition = startPos;
        transform.rotation = Quaternion.identity;
    }

    private void Update() {
        if (gameManager.GameOver) return;

        if (Input.GetMouseButtonDown(0)) {
            transform.rotation = forwardRotation;
            // To restart the velocity
            rb.velocity = Vector3.zero;
            rb.AddForce(Vector2.up * tapForce, ForceMode2D.Force);

        }
        transform.rotation = Quaternion.Lerp(transform.rotation, downRotation, tiltSmooth * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if(col.gameObject.tag == "ScoreZone") {
            // Add score
            OnPlayerScored(); // event sent to gameManager
            // Play sound
        }

        if(col.gameObject.tag == "DeadZone") {
            rb.simulated = false;
            // dead event
            OnPlayerDied(); // event sent to gameManager
            // play sound

        }
    }

}
