using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class DynamicSprite : MonoBehaviour {

    public new Rigidbody2D rigidbody { get; private set; }
    public new Rigidbody2D rigidbody2D { get; private set; }
    public new Collider2D collider { get; private set; }
    public new Collider2D collider2D { get; private set; }
    public new SpriteRenderer renderer { get; private set; }

    [Header("Physics")]
    public float gravity;
    public float maxYSpeed;

    Vector2 targetVelocity;
    bool move;

    float targetRotationSpeed;
    bool rotate;

    public Vector2 size { get; protected set; }
    public Color colour { get; private set; }

    protected virtual void Awake() {
        InitializeComponents();
    }

    void Start() {
        if(Options.skin != null) renderer.sprite = Options.skin;
    }

    protected virtual void InitializeComponents() {
        if(rigidbody == null) rigidbody = GetComponent<Rigidbody2D>();
        if(rigidbody2D == null) rigidbody2D = rigidbody;
        if(collider == null) collider = GetComponent<Collider2D>();
        if(collider2D == null) collider2D = collider;
        if (renderer == null) renderer = GetComponent<SpriteRenderer>();
    }


    protected virtual void Update() {
        Vector3 velocity = rigidbody.velocity;
        if (maxYSpeed != 0) velocity.y = Mathf.Clamp(velocity.y, -maxYSpeed, maxYSpeed);
        rigidbody.velocity = velocity;
    }

    protected virtual void FixedUpdate() {
        if (move) rigidbody.AddForce(Vector2.right * (targetVelocity.x - rigidbody.velocity.x) * rigidbody.mass, ForceMode2D.Impulse);
        if (move && targetVelocity.y != 0) rigidbody.AddForce(Vector2.up * (targetVelocity.y - rigidbody.velocity.y) * rigidbody.mass, ForceMode2D.Impulse);

        if (rotate) rigidbody.angularVelocity = targetRotationSpeed;

        if (gravity != 0) rigidbody.AddForce(Vector2.down * gravity * rigidbody.mass, ForceMode2D.Force);

        move = false;
        rotate = false;
    }

    public void Move(Vector2 velocity) {
        targetVelocity = velocity;
        move = true;
    }
    public void Rotate(float rotationSpeed) {
        targetRotationSpeed = rotationSpeed;
        rotate = true;
    }

    public void SetSize(Vector2 size) {
        this.size = size;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = size;
        if (collider is BoxCollider2D) ((BoxCollider2D)collider).size = size;
        else if (collider is CircleCollider2D) ((CircleCollider2D)collider).radius = size.magnitude;
        else if (collider is CapsuleCollider2D) ((CapsuleCollider2D)collider).size = size;
    }
    public void SetColour(Color colour) {
        this.colour = colour;
        renderer.color = colour;
    }

    void OnValidate() {
        InitializeComponents();
        if(maxYSpeed < 0) maxYSpeed = 0;
        rigidbody.gravityScale = 0;
    }
}
