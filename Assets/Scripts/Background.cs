using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Background : MonoBehaviour {

    public new SpriteRenderer renderer { get; private set; }

	public Sprite background { get; protected set; }
    static List<Background> instances = new List<Background>();

    void Awake() {
        renderer = GetComponent<SpriteRenderer>();
        if (!instances.Contains(this)) instances.Add(this);
    }

    void Start() {
        UpdateBackgrounds();
    }

    public static void UpdateBackgrounds() {
        foreach (Background background in instances) {
            background.SetBackground(Options.background);
        }
    }

    public void SetBackground(Sprite background) {
        this.background = background;
        if(renderer != null) renderer.sprite = background;
    }
}
