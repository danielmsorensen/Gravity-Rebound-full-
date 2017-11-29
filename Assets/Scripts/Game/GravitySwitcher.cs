using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitySwitcher : MonoBehaviour {

    public DynamicSprite target;

    public float upper;
    public float lower;

    public LineRenderer upperLine;
    public LineRenderer lowerLine;

    [Header("Audio")]
    public AudioClip upSound;
    public AudioClip downSound;

    bool playingSound;

    float gravity;

    void Start() {
        if (target != null) gravity = Mathf.Abs(target.gravity);
        CameraManager.main.OnCameraTrack += OnCameraTrack;
    }

    [ContextMenu("Update Lines")]
    public void UpdateLines() {
        if (upperLine != null) {
            upperLine.SetPositions(new Vector3[2] {
                new Vector3(CameraManager.main.cameraLeft, upper, 2),
                new Vector3(CameraManager.main.cameraRight, upper, 2)
            });
        }
        if (lowerLine != null) {
            lowerLine.SetPositions(new Vector3[2] {
                new Vector3(CameraManager.main.cameraLeft, lower, 2),
                new Vector3(CameraManager.main.cameraRight, lower, 2)
            });
        }
    }

    void Update() {
        if (target != null && !Game.main.gameover) {
            if (target.transform.position.y >= upper) {
                target.gravity = gravity;
                if (!playingSound) {
                    SoundManager.instance.PlaySound2D(downSound);
                    playingSound = true;
                }
            }
            else if (target.transform.position.y <= lower) {
                target.gravity = -gravity;
                if (!playingSound) {
                    SoundManager.instance.PlaySound2D(upSound);
                    playingSound = true;
                }
            }
            else {
                playingSound = false;
            }
        }
    }

    void OnValidate() {
        if(CameraManager.main != null) {
            UpdateLines();
        }
    }

    void OnCameraTrack(Vector2 position) {
        UpdateLines();
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.white;
        if (upperLine == null) Gizmos.DrawLine(new Vector3(CameraManager.main.cameraLeft, upper), new Vector3(CameraManager.main.cameraRight, upper));
        if (lowerLine == null) Gizmos.DrawLine(new Vector3(CameraManager.main.cameraLeft, lower), new Vector3(CameraManager.main.cameraRight, lower));
        UpdateLines();
    }
}
