using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObstacleSpawner : MonoBehaviour {

    public Obstacle prefab;
    [Space]
    public DynamicSprite target;
    [Space]
    public float obstacleSpacing;
    public uint obstacleCount;
    public float headStart;
    [Space]
    public Difficulty difficulty;
    int currObstacle;
    [Space]
    public List<Block> blocks = new List<Block>();
    [Space]
    public AudioClip passSound;

    Queue<Obstacle> obstacles = new Queue<Obstacle>();

    public int level { get; protected set; }

    void Start() {
        for (int i = 0; i < obstacleCount; i++) {
            SpawnObstacle(Instantiate(prefab, Vector3.zero, Quaternion.identity, transform));
        }
    }

    void LateUpdate() {
        if (!Game.main.gameover) {
            Obstacle lastObstacle = obstacles.ToArray()[obstacles.Count - 1];

            if (CameraManager.main.VisibleByCamera(lastObstacle.transform.position, CameraManager.Visibility.X)) {
                SpawnNextObstacle();
            }

            foreach (Obstacle obstacle in obstacles.ToArray()) {
                if (obstacle.CheckPassed(target)) {
                    if (difficulty.obstaclesPerLevel != 0) {
                        int level = Mathf.FloorToInt((obstacle.ID + 1) / difficulty.obstaclesPerLevel);
                        if (level > this.level) {
                            print("Next Level: " + level);
                            this.level = level;
                            Game.main.ChangeLevel(level);
                        }
                    }

                    SoundManager.instance.PlaySound2D(passSound);
                }
            }
        }
    }

    public void SpawnNextObstacle() {
        SpawnObstacle(obstacles.Dequeue());
    }

    void SpawnObstacle(Obstacle obstacle) {
        int level = (difficulty.obstaclesPerLevel == 0) ? -1 : Mathf.FloorToInt(currObstacle / difficulty.obstaclesPerLevel);

        Vector2 speed = Mathf.Clamp(level - difficulty.moveStartLevel + 1, 0, float.MaxValue) * difficulty.speedIncrease;
        if(speed != Vector2.zero) speed += Random.Range(-difficulty.speedRandomness / 2, difficulty.speedRandomness / 2) * Vector2.one;
        float rotationSpeed = Mathf.Clamp(level - difficulty.rotateStartLevel + 1, 0, float.MaxValue) * difficulty.rotationIncrease;
        if(rotationSpeed != 0) rotationSpeed += Random.Range(-difficulty.rotationRandomness / 2, difficulty.rotationRandomness / 2);

        Obstacle lastObstacle = (obstacles.Count == 0) ? null : obstacles.ToArray()[obstacles.Count - 1];
        float lastX = (lastObstacle == null) ? headStart : lastObstacle.transform.position.x + lastObstacle.size.x / 2;

        Vector2 size = new Vector2(Random.Range(difficulty.minSize, difficulty.maxSize), Random.Range(difficulty.minSize, difficulty.maxSize));
        Vector2 nextPosition = new Vector2(lastX + obstacleSpacing + size.x / 2, Random.Range(difficulty.lowerLimit, difficulty.upperLimit));

        Color colour = Color.white;
        uint score = 1;

        foreach (Block b in blocks) {
            float area = 0;

            if (obstacle.collider is BoxCollider2D) {
                area = size.x * size.y;
            }

            if (area <= b.area) {
                colour = b.color;
                score = b.score;

                break;
            }
        }

        obstacle.Respawn(nextPosition, size, speed, rotationSpeed, colour, score, currObstacle, this, prefab.harmfull);
        obstacles.Enqueue(obstacle);

        currObstacle += 1;
    }

    public enum Preview { None, Spacing, Size, Limits, Blocks }
    [Space] public Preview preview;
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        switch (preview) {
            case (Preview.Spacing):
                Gizmos.DrawLine(new Vector3(headStart, CameraManager.main.cameraTop), new Vector3(headStart, CameraManager.main.cameraBottom));
                Gizmos.DrawLine(new Vector3(headStart + obstacleSpacing, CameraManager.main.cameraTop), new Vector3(headStart + obstacleSpacing, CameraManager.main.cameraBottom));
                break;
            case (Preview.Size):
                Gizmos.DrawCube(Vector3.right * headStart, difficulty.maxSize * Vector2.one);
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(Vector3.right * headStart, difficulty.minSize * Vector2.one);
                break;
            case (Preview.Limits):
                Rect r = new Rect();
                r.min = new Vector2(difficulty.leftLimit + headStart, difficulty.lowerLimit);
                r.max = new Vector2(difficulty.rightLimit + headStart, difficulty.upperLimit);
                Gizmos.DrawCube(r.center, r.size);
                break;
            case (Preview.Blocks):
                List<Block> blocks = new List<Block>(this.blocks);
                blocks.Reverse();
                for (int i = 0; i < blocks.Count; i++) {
                    Gizmos.color = blocks[i].color;
                    Gizmos.DrawCube(transform.position + Vector3.right * headStart, Vector3.one * Mathf.Sqrt(blocks[i].area));
                }
                break;
        }
    }

    void OnValidate() {
        float maxArea = Mathf.Pow(difficulty.maxSize, 2);
        for (int i = 0; i < blocks.Count; i++) {
            Block b = blocks[i];
            if (i == blocks.Count - 1) b.area = maxArea;
            else {
                b.area = Mathf.Clamp(b.area, 0, maxArea);
                if(i != 0) {
                    if (b.area < blocks[i - 1].area) b.area = blocks[i - 1].area;
                }
            }
            blocks[i] = b;
        }
    }

    [System.Serializable]
    public struct Difficulty {
        [Header("Size")]
        public float maxSize;
        public float minSize;
        [Header("Levels")]
        public uint obstaclesPerLevel;
        [Space]
        public uint moveStartLevel;
        public Vector2 speedIncrease;
        public float speedRandomness;
        [Space]
        public uint rotateStartLevel;
        public float rotationIncrease;
        public float rotationRandomness;
        [Header("Limits")]
        public float upperLimit;
        public float lowerLimit;
        public float leftLimit;
        public float rightLimit;
    }

    [System.Serializable]
    public struct Block {
        public float area;

        public Color color;
        public uint score;
    }
}
