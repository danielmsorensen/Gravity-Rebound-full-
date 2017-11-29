using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : SceneSwitcher {

    public UI UI;
    public Highscores highscores;
    [Header("Level Bar")]
    public CanvasGroup levelBar;
    public float inTime;
    public float duration;
    public float outTime;
    [Header("Death Animation")]
    public float slowTime;
    [Header("Audio")]
    public AudioClip hitSound;

	public int score { get; protected set; }
    public int level { get; protected set; }
    public bool gameover { get; protected set; }

    public static Game main { get; private set; }

    public bool paused { get; protected set; }

    protected virtual void Awake() {
        main = this;
        gameover = false;

        Time.timeScale = 1;
    }

    protected virtual void Start() {
        score = 0;
        UI.SetText("Score", "Score: 000");
        levelBar.alpha = 0;
    }

    public void IncreaseScore(uint ammount) {
        score += (int)ammount;
        UI.SetText("Score", "Score: " + score.ToString("D3"));
    }
    
    public void GameOver() {
        gameover = true;

        UI.SetText("ScoreGO", "Score: " + score.ToString("D3"));
        
        int highscore = PlayerPrefs.GetInt("Highscore", 0);
        if (score > highscore) {
            UI.SetText("Highscore", "New Highscore");
            PlayerPrefs.SetInt("Highscore", score);
            highscore = score;
        }
        else {
            UI.SetText("Highscore", "Highscore: " + highscore.ToString("D3"));
        }

        highscores.UploadHighscore(Options.username, score, Highscore.Version.Full);
        SoundManager.instance.PlaySound2D(hitSound);

        StartCoroutine(SlowTime(slowTime));
    }

    IEnumerator SlowTime(float duration) {
        float start = Time.realtimeSinceStartup;
        float startScale = Time.timeScale;

        while (Time.realtimeSinceStartup < start + duration) {
            Time.timeScale = Mathf.Lerp(startScale, 0, Mathf.InverseLerp(start, start + duration, Time.realtimeSinceStartup));
            yield return null;
        }

        Time.timeScale = 0;
        UI.SetPage("GameOver");
    }

    public void ChangeLevel(int level) {
        this.level = level;
        UI.SetText("Level Bar", "Level: " + level);
        StopCoroutine("FadeLevelBar");
        StartCoroutine(FadeLevelBar(inTime, duration, outTime));
    }

    IEnumerator FadeLevelBar(float inTime, float duration, float outTime) {
        float start = Time.time;
        float end = start + inTime + duration + outTime;

        while (Time.time <= end) {
            if (Time.time <= start + inTime) levelBar.alpha = Mathf.Lerp(0, 1, Mathf.InverseLerp(start, start + inTime, Time.time));
            else if(Time.time <= end && Time.time >= end - outTime) levelBar.alpha = Mathf.Lerp(1, 0, Mathf.InverseLerp(start, end, Time.time));
            yield return null;
        }

        levelBar.alpha = 0;
    }

    public void Pause() {
        if (!gameover) {
            Time.timeScale = 0;
            paused = true;
            UI.SetPage("Pause");
        }
    }
    public void Resume() {
        if (!gameover) {
            Time.timeScale = 1;
            paused = false;
            UI.SetPage("InGame");
        }
    }

    public void Highscores() {
        UI.SetPage("Highscores");
        highscores.DownloadHighscores();
    }

    void OnValidate() {
        main = this;
    }
}
