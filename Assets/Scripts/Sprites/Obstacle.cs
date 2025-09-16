using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : DynamicSprite {

    public bool passed { get; protected set; }
    public int ID { get; protected set; }

    Vector2 speed;
    float rotationSpeed;
    uint score;

    public bool harmfull=true;

    Vector2 start;

    ObstacleSpawner spawner;

    public void Pass() {
        if (!passed && !Game.main.gameover) {
            passed = true;
            Game.main.IncreaseScore(score);
        }
    }

    public bool CheckPassed(DynamicSprite target) {
        if(!passed && target.transform.position.x - target.size.x / 2 > transform.position.x + size.x / 2) {
            Pass();
            return true;
        }
        else {
            return false;
        }
    }

    protected override void Update() {
        base.Update();

        if (speed != Vector2.zero) {
            if (transform.position.y + size.y / 2 >= spawner.difficulty.upperLimit) speed.y = -Mathf.Abs(speed.y);
            else if (transform.position.y - size.y / 2 <=spawner.difficulty.lowerLimit) speed.y = Mathf.Abs(speed.y);

            if (transform.position.x + size.x / 2 >= start.x + spawner.difficulty.rightLimit) speed.x = -Mathf.Abs(speed.x);
            else if (transform.position.x - size.x / 2 <= start.x + spawner.difficulty.leftLimit) speed.x = Mathf.Abs(speed.x);
        }

        if (CameraManager.main.VisibleByCamera(transform.position, CameraManager.Visibility.X)) {
            if (Game.main.level >= Mathf.FloorToInt(ID / spawner.difficulty.obstaclesPerLevel) && !Game.main.gameover) {
                if (speed != Vector2.zero) Move(speed);
                if (rotationSpeed != 0) Rotate(rotationSpeed);
            }
        }
        else {
            Move(Vector2.zero);
        }
    }

    public void Respawn(Vector2 position, Vector2 size, uint score, int ID, ObstacleSpawner spawner) {
        Respawn(position, size, Vector2.zero, 0f, Color.white, score, ID, spawner);
    }
    public void Respawn(Vector2 position, Vector2 size, Vector2 speed, float rotationSpeed, Color colour, uint score, int ID, ObstacleSpawner spawner, bool harmfull=true) {
        passed = false;
        
        rigidbody.linearVelocity = Vector2.zero;
        rigidbody.angularVelocity = 0;

        transform.position = position;
        transform.rotation = Quaternion.identity;
        start = position;

        this.speed = speed;
        this.speed.x *= (Random.Range(0, 2) * 2 - 1);
        this.speed.y *= (Random.Range(0, 2) * 2 - 1);
        this.rotationSpeed = rotationSpeed * (Random.Range(0, 2) * 2 - 1);
        
        this.ID = ID;

        SetSize(size);
        SetColour(colour);

        this.score = score;
        this.harmfull = harmfull;

        this.spawner = spawner;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.tag == "Player" && harmfull) Game.main.GameOver();
    }
}
