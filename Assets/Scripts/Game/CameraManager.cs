using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour {

    public static CameraManager main { get; private set; }

    public new Camera camera { get; private set; }
    public float cameraWidth { get; protected set; }
    public float cameraHeight { get; protected set; }

    public float cameraLeft { get { return transform.position.x - cameraWidth / 2; } }
    public float cameraRight { get { return transform.position.x + cameraWidth / 2; } }
    public float cameraTop { get { return transform.position.y + cameraHeight / 2; } }
    public float cameraBottom { get { return transform.position.y - cameraHeight / 2; } }

    public Transform target;
    [Space]
    public Vector2 offset;
    [Space]
    public bool trackX;
    public bool trackY;
    public bool trackRotation;

    bool focus;

    public System.Action<Vector2> OnCameraTrack;
    Vector2 lastPosition;

    void Awake() {
        InitializeComponents();
        InitializeValues();
    }

    void InitializeComponents() {
        if (camera == null) camera = GetComponent<Camera>();
    }
    void InitializeValues() {
        main = this;
        if (camera != null) {
            cameraHeight = camera.orthographicSize * 2;
            cameraWidth = camera.aspect * cameraHeight;
        }
    }

    public enum Visibility { Both, X, Y, Either };
    public bool VisibleByCamera(Vector2 point) {
        return VisibleByCamera(point, Visibility.Both);
    }
    public bool VisibleByCamera(Vector2 point, Visibility visibility) {
        bool x = point.x <= transform.position.x + cameraWidth / 2 && point.x >= transform.position.x - cameraWidth / 2;
        bool y = point.y <= transform.position.y + cameraHeight / 2 && point.y >= transform.position.y - cameraHeight / 2;

        switch(visibility) {
            default:
            case (Visibility.Both): return x && y;
            case (Visibility.X): return x;
            case (Visibility.Y): return y;
            case (Visibility.Either): return x || y;
        }
    }

    void LateUpdate() {
        TrackTarget();
    }

    public void TrackTarget() {
        if (target != null && !focus) {
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;

            if (trackX) position.x = target.transform.position.x + offset.x;
            if (trackY) position.y = target.transform.position.y + offset.y;
            if (trackRotation) rotation = target.transform.rotation;

            transform.position = position;
            transform.rotation = rotation;

            if((Vector2)position != lastPosition) {
                if (OnCameraTrack != null) OnCameraTrack.Invoke(position);
            }

            lastPosition = position;
        }
    }

    public void Focus(float zoom, float time) {
        focus = true;

        trackX = true;
        trackY = true;
        trackRotation = true;
        offset = Vector2.zero;

        StartCoroutine(FocusTarget(zoom, time));
    }
    IEnumerator FocusTarget(float zoom, float time) {
        float start = Time.realtimeSinceStartup;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float startZoom = camera.orthographicSize;

        Vector3 targetPos = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);

        while(Time.realtimeSinceStartup <= start + time) {
            targetPos = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);

            Vector3 position = Vector3.Lerp(startPos, targetPos, Mathf.InverseLerp(start, start + time, Time.realtimeSinceStartup));
            Quaternion rotation = Quaternion.Lerp(startRot, target.transform.rotation, Mathf.InverseLerp(start, start + time, Time.realtimeSinceStartup));
            float camZoom = Mathf.Lerp(startZoom, zoom, Mathf.InverseLerp(start, start + time, Time.realtimeSinceStartup));

            transform.position = position;
            transform.rotation = rotation;
            camera.orthographicSize = camZoom;

            yield return null;
        }

        transform.rotation = target.rotation;
        transform.position = targetPos;
        camera.orthographicSize = zoom;

        focus = false;
    }

    void OnValidate() {
        InitializeComponents();
        InitializeValues();
        TrackTarget();
    }
}
