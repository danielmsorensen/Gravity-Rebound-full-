using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : DynamicSprite {

    [Header("Movement")]
    public float moveSpeed;
    public float behindLimit;
    float activeLimit;

    int input;
    
    protected override void Awake() {
        base.Awake();

        activeLimit = behindLimit;
    }

    protected override void Update() {
        base.Update();
        
        input = CustomInput.GetHorizontalInput();
        if (!Game.main.gameover) Move(input * moveSpeed * Vector2.right);
        else gravity = 0;

        if (transform.position.x + behindLimit > activeLimit) activeLimit = transform.position.x + behindLimit;
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

        Vector2 position = rigidbody.position;
        position.x = Mathf.Clamp(position.x, activeLimit, position.x);
        rigidbody.position = position;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        float x = ((Application.isPlaying) ? activeLimit : transform.position.x + behindLimit);
        Gizmos.DrawLine(new Vector3(x, CameraManager.main.cameraTop), new Vector3(x, CameraManager.main.cameraBottom));
    }
}
