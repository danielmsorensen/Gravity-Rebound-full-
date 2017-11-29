using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
public class Tiler : MonoBehaviour {

    public new SpriteRenderer renderer { get; protected set; }

    [Range(0, 1)]
    public float paralax;
    public new Camera camera;
    [Space]
    public bool tileInverted;
    public float padding;
    [Space]
    public string ID = "tile";

    float spriteWidth;
    Vector3 start;
    Vector3 offset;
    
    protected Tiler left, right;

    protected static Dictionary<string, LinkedList<Tiler>> tiles = new Dictionary<string, LinkedList<Tiler>>();

    protected void Awake() {
        if(renderer == null) renderer = GetComponent<SpriteRenderer>();
        if (camera == null) Debug.LogError("A camera must be specified");
        else {
            if(!camera.orthographic) {
                camera.orthographic = true;
                Debug.LogWarning("The Camera has been set to orthographic mode");
            }
        }

        if (renderer.sprite == null) Debug.LogError("The Sprite Renderer must have a sprite");
        else spriteWidth = renderer.sprite.bounds.size.x;

        left = right = null;

        start = transform.position;
        offset = start - camera.transform.position;
    }

    void Start() {
        if (!tiles.ContainsKey(ID)) {
            tiles.Add(ID, new LinkedList<Tiler>());
            tiles[ID].AddFirst(this);

            CheckEdges(false);

            while (tiles[ID].Count < 3) {
                CreateTile(1, false);
            }
        }
    }

    void Update() {
        if (paralax != 0) {
            transform.position = Vector3.Lerp(start, camera.transform.position + offset, paralax);
        }
    }

    void LateUpdate() {
        CheckEdges(true);
    }

    void CheckEdges(bool pool) {
        if (left == null || right == null) {
            float camWidth = camera.orthographicSize * camera.aspect;

            float edgeRight = transform.position.x + (spriteWidth / 2f);
            float edgeLeft  = transform.position.x - (spriteWidth / 2f);

            if (camera.transform.position.x + camWidth >= edgeRight + padding && right == null) {
                right = CreateTile(1, pool);
                right.CheckEdges(pool);
            }
            if (camera.transform.position.x - camWidth <= edgeLeft - padding && left == null) {
                left = CreateTile(-1, pool);
                left.CheckEdges(pool);
            }
        }
    }

    Tiler CreateTile(int side, bool pool) {
        side = (int)Mathf.Sign(side);

        Vector3 pos = new Vector3(transform.position.x + ((spriteWidth + padding) * side), transform.position.y, transform.position.z);
        Tiler tile;

        if (pool) {
            tile = (side == 1) ? tiles[ID].First.Value : tiles[ID].Last.Value;
            tiles[ID].Remove(tile);
            
            tile.transform.position = pos;
            tile.transform.rotation = transform.rotation;

            tile.ID = ID;
            tile.Awake();
        }
        else {
            tile = Instantiate(this, pos, transform.rotation) as Tiler;
            tile.ID = ID;
        }

        if (side == 1) {
            tiles[ID].AddLast(tile);
            tiles[ID].First.Value.left = null;
        }
        else {
            tiles[ID].AddFirst(tile);
            tiles[ID].Last.Value.right = null;
        }

        tile.transform.SetParent(transform.parent, true);

        if (tileInverted) {
            tile.renderer.flipX = !renderer.flipX;
        }

        if (side == 1) {
            tile.left = this;
        }
        else {
            tile.right = this;
        }

        return tile;
    }

    void OnDrawGizmosSelected() {
        if (renderer == null) renderer = GetComponent<SpriteRenderer>();
        if (renderer.sprite == null) return;

        spriteWidth = renderer.sprite.bounds.size.x;
        float spriteHeight = renderer.sprite.bounds.size.y;

        float edgeRight = transform.position.x + (spriteWidth / 2f);
        float edgeLeft = transform.position.x - (spriteWidth / 2f);

        Gizmos.color = Color.blue;

        Gizmos.DrawLine(new Vector3(edgeLeft - padding, (-spriteHeight / 1.5f) + transform.position.y), new Vector3(edgeLeft - padding, (spriteHeight / 1.5f) + transform.position.y));
        Gizmos.DrawLine(new Vector3(edgeRight + padding, (-spriteHeight / 1.5f) + transform.position.y), new Vector3(edgeRight + padding, (spriteHeight / 1.5f) + transform.position.y));
    }

    void OnValidate() {
        if (renderer == null) renderer = GetComponent<SpriteRenderer>();
        if (renderer.sprite == null) return;

        spriteWidth = renderer.sprite.bounds.size.x;

        if (padding < -spriteWidth / 2f) padding = -spriteWidth / 2f;
    }

    #region Clean Up
    void OnEnable() {
        SceneManager.sceneLoaded += OnLevelLoaded;
    }
    void OnDisable() {
        SceneManager.sceneLoaded -= OnLevelLoaded;
    }

    void OnLevelLoaded(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) tiles.Remove(ID);
    }
    #endregion
}
