using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transformation : MonoBehaviour {

    public Canvas canvas;
    [Space]
    public Rect limits;
    public Vector2 speed;
    public float rotationSpeed;
    public bool randomSpeed;
    public bool randomRotation;
    public float minRandomTime;

    Vector2 activeSpeed;
    float activeRotationSpeed;
    float lastMoveTime;
    float lastRotTime;

    float lastTime;
    float deltaTime;

    Vector2 pos;

    void Awake() {
        activeSpeed = speed;
        activeRotationSpeed = rotationSpeed;
        lastMoveTime = lastRotTime = -minRandomTime;

        lastTime = Time.realtimeSinceStartup - deltaTime;
    }

    void Start() {
        Vector2 scale = new Vector2(canvas.pixelRect.size.x / 1280, canvas.pixelRect.size.y / 800);
        Vector2 centre = limits.center;
        limits.size = new Vector2(limits.size.x * scale.x, limits.size.y * scale.y);
        limits.center = new Vector2(centre.x * scale.x, centre.y * scale.y);

        speed = new Vector2(speed.x * scale.x, speed.y * scale.y);
    }

    void Update() {
        deltaTime = Time.realtimeSinceStartup - lastTime;

        if (pos.x >= limits.xMax) {
            activeSpeed.x = -Mathf.Abs(speed.x);
            Move(limits.xMax - pos.x, 0);
        }
        if (pos.x <= limits.xMin) {
            activeSpeed.x = Mathf.Abs(speed.x);
            Move(limits.xMin - pos.x, 0);
        }
        if (pos.y >= limits.yMax) {
            activeSpeed.y = -Mathf.Abs(speed.y);
            Move(0, limits.yMax - pos.y);
        }
        if (pos.y <= limits.yMin) {
            activeSpeed.y = Mathf.Abs(speed.y);
            Move(0, limits.yMin - pos.y);
        }

        if(randomSpeed && Time.realtimeSinceStartup > lastMoveTime + minRandomTime) {
            activeSpeed.x *= Random.Range(0, 2) * 2 - 1;
            activeSpeed.y *= Random.Range(0, 2) * 2 - 1;

            lastMoveTime = Time.realtimeSinceStartup;
        }
        if(randomRotation && Time.realtimeSinceStartup > lastRotTime + minRandomTime) {
            activeRotationSpeed *= Random.Range(0, 2) * 2 - 1;

            lastRotTime = Time.realtimeSinceStartup;
        }

        if (activeSpeed != Vector2.zero) Move(activeSpeed * deltaTime);
        if(activeRotationSpeed != 0) transform.Rotate(0, 0, activeRotationSpeed * deltaTime, Space.Self);

        lastTime = Time.realtimeSinceStartup;
    }

    public void Move(float x, float y) {
        Move(new Vector2(x, y));
    }
    public void Move(Vector2 translation) {
        transform.Translate(translation, Space.World);
        pos += translation;
    }

    void OnDrawGizmosSelected() {
        if (!Application.isPlaying) {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube((Vector3)limits.center + transform.position, limits.size);
            if (limits.size.x == 0 || limits.size.y == 0) Gizmos.DrawLine((Vector3)limits.min + transform.position, (Vector3)limits.max + transform.position);
        }
    }
}
