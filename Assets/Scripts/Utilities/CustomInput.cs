using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomInput : MonoBehaviour , IPointerDownHandler, IPointerUpHandler{

    [System.Serializable]
    public class Event : UnityEngine.Events.UnityEvent { }
    [System.Serializable]
    public class IntEvent : UnityEngine.Events.UnityEvent<int> { }
    public IntEvent OnInput = new IntEvent();

    public bool cancelInputWhenPressed;

    static bool useInput=true;

    void Update() {
        if(useInput && !Game.main.paused && (Input.GetMouseButtonDown(0) || Input.GetAxisRaw("Horizontal") != 0)) {
            OnInput.Invoke(GetHorizontalInput());
        }
    }

    public static int GetHorizontalInput() {
        float cameraMid = CameraManager.main.camera.ViewportToScreenPoint(0.5f * Vector3.one).x;
        int touchInput = (Input.mousePosition.x >= cameraMid) ? 1 : -1;
        if (!Input.GetMouseButton(0)) touchInput = 0;

        if (useInput) return (int)Mathf.Clamp(touchInput + Input.GetAxisRaw("Horizontal"), -1, 1);
        else return 0;
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (cancelInputWhenPressed) useInput = false;
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (cancelInputWhenPressed) useInput = true;
    }
}
