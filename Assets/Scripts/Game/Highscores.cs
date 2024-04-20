using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Highscores : MonoBehaviour {

    const string privateCode = "092BYqHln0Wd-OFZO9syrgNDv-K_I9OEauhTp3x2zIOw";
    const string publicCode = "581b9c7c8af60313582463a4";
    const string url = "http://dreamlo.com/lb/";

    [Header("UI")]
    public bool useUI = true;
    public ScrollRect scrollRect;
    public AutoLayoutGroup content;
    public Transform template;
    [Space]
    public float scrollTime;
    public Color first, second, third;
    public Color mine;

    [Header("Debugging")]
    public Highscore[] highscores;

    public void UploadHighscore(Highscore highscore) {
        UploadHighscore(highscore.username, highscore.score, highscore.version);
    }
    public void UploadHighscore(string username, int score, Highscore.Version version) {
        StartCoroutine(AddHighscore(username, score, version));
    }
    IEnumerator AddHighscore(string username, int score, Highscore.Version version) {
        WWW www = new WWW(url + privateCode + "/add/" + WWW.EscapeURL(username) + "/" + score + "/0/" + version.ToString());
        yield return www;

        if (string.IsNullOrEmpty(www.error)) {
            Debug.Log("Highscore uploaded successfully: " + username);
        } else {
            Debug.LogError("Error uploading highscore: " + www.error);
        }
    }

    [ContextMenu("Download Highscores")]
    public void DownloadHighscores() {
        DownloadHighscores(null);
    }
    public void DownloadHighscores(System.Action<Highscore[]> callback) {
        StartCoroutine(GetHighscores(callback));
    }
    IEnumerator GetHighscores(System.Action<Highscore[]> callback) {
        WWW www = new WWW(url + publicCode + "/pipe/");
        yield return www;

        if (string.IsNullOrEmpty(www.error)) {
            Debug.Log("Highscores downloaded successfully");

            string[] entries = www.text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            highscores = new Highscore[entries.Length];

            for (int i = 0; i < entries.Length; i++) {
                highscores[i] = new Highscore(entries[i]);
            }

            if (callback != null) callback.Invoke(highscores);
            if (useUI) UpdateUI();
        } else {
            Debug.LogError("Error downloading highscores: " + www.error);
        }
    }

    public void ChangeUsername(string oldUsername, string newUsername) {
        StartCoroutine(GetUsername(oldUsername, newUsername));
    }
    IEnumerator GetUsername(string oldUsername, string newUsername) {
        WWW www = new WWW(url + publicCode + "/pipe-get/" + WWW.EscapeURL(oldUsername));
        yield return www;

        if (string.IsNullOrEmpty(www.error)) {
            Highscore highscore = new Highscore(www.text);

            www = new WWW(url + privateCode + "/delete/" + oldUsername);
            yield return www;

            if (string.IsNullOrEmpty(www.error)) {
                www = new WWW(url + privateCode + "/add/" + WWW.EscapeURL(newUsername) + "/" + highscore.score + "/0/" + highscore.version.ToString());
                yield return www;

                if (string.IsNullOrEmpty(www.error)) {
                    Debug.Log("Username changed successfully from: " + oldUsername + " to: " + newUsername);
                } else {
                    Debug.LogError("Error changing username - add: " + www.error);
                }
            } else {
                Debug.LogError("Error changing username - delete: " + www.error);
            }
        } else {
            Debug.LogError("Error changing username - get: " + www.error);
        }
    }

    public void DownloadHighscore(string username, System.Action<Highscore, bool> callback) {
        StartCoroutine(GetHighscore(username, callback));
    }
    IEnumerator GetHighscore(string username, System.Action<Highscore, bool> callback) {
        WWW www = new WWW(url + publicCode + "/pipe-get/" + WWW.EscapeURL(username));
        yield return www;

        if (string.IsNullOrEmpty(www.error)) {
            if (string.IsNullOrEmpty(www.text)) {
                Debug.Log("The username: " + username + " does not exist");
                callback.Invoke(Highscore.Null, false);
            } else {
                Debug.Log("Successfully downloaded highscore: " + username);
                callback.Invoke(new Highscore(www.text), true);
            }
        } else {
            Debug.LogError("Error downloading highscore");
        }
    }
    public void RemoveHighscore(string username) {
        StartCoroutine(DeleteHighscore(username));
    }
    IEnumerator DeleteHighscore(string username) {
        WWW www = new WWW(url + privateCode + "/delete/" + username);
        yield return www;

        if (string.IsNullOrEmpty(www.error)) {
            Debug.Log("Highscore successfully removed: " + username);
        } else {
            Debug.LogError("Error removing highscore: " + www.error);
        }
    }

    #region Helpers
    public void DownloadUsernames(System.Action<string[]> callback) {
        DownloadHighscores((Highscore[] highscores) => {
            string[] usernames = highscores.Select((Highscore h) => h.username).ToArray();
            callback.Invoke(usernames);
        });
    }

    public void ContainsUsername(string username, System.Action<bool> callback) {
        DownloadHighscore(username, (Highscore highscore, bool exists) => {
            callback.Invoke(exists);
        });
    }

    public static int GetUsernameHash(string username) {
        return ("#" + username).GetHashCode();
    }
    #endregion

    #region UI
    public void UpdateUI() {
        if (!useUI) return;

        content.ClearChildren(true);

        for (int i = 0; i < highscores.Length; i++) {
            Highscore score = highscores[i];

            Transform item = Instantiate(template, content.transform);
            item.gameObject.SetActive(true);
            item.gameObject.name = score.username;

            Image image = item.GetComponent<Image>();
            if (i == 0) image.color = first;
            else if (i == 1) image.color = second;
            else if (i == 2) image.color = third;
            else if (score.username == Options.username) image.color = mine;

            foreach (Text t in item.GetComponentsInChildren<Text>()) {
                if (t.gameObject.name == "Rank") t.text = (i + 1).ToString("D3");
                else if (t.gameObject.name == "Username") t.text = score.username;
                else if (t.gameObject.name == "Version") {
                    t.text = score.version.ToString();
                    t.color = (score.version == Highscore.Version.Full) ? Color.blue : Color.red;
                } else if (t.gameObject.name == "Score") t.text = score.score.ToString("D3");
            }
        }

        content.ReCalculate();
        ScrollToMe();
    }

    public void ScrollToTop() {
        if (!useUI) return;
        CancelScroll();
        StartCoroutine(ScrollTo(0));
    }
    public void ScrollToMe() {
        if (!useUI) return;

        int index = (highscores.Select((Highscore h) => h.username).ToList().IndexOf(Options.username));
        CancelScroll();
        StartCoroutine(ScrollTo(index));
    }
    public void CancelScroll() {
        StopCoroutine("ScrollTo");
    }

    IEnumerator ScrollTo(int index) {
        yield return null;

        int childCount = content.GetActiveChildCount();

        float scroll = Mathf.InverseLerp(childCount - 1, 0, index);
        scroll = Mathf.Clamp01(scroll);

        float start = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup < start + scrollTime) {
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, scroll, (Mathf.InverseLerp(start, start + scrollTime, Time.realtimeSinceStartup)));
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = scroll;
    }
    #endregion
}

[System.Serializable]
public struct Highscore {
    public string username;
    public int score;
    public enum Version { Full, Lite };
    public Version version;

    public int hash;

    public Highscore(string username, int score, Version version) {
        this.username = username;
        this.score = score;
        this.version = version;
        hash = Highscores.GetUsernameHash(username);
    }

    public Highscore(string pipe) {
        string[] parts = pipe.Split(new char[] { '|' });

        if (parts.Length == 1) {
            this = Null;
        } else {
            string username = WWW.UnEscapeURL(parts[0]);
            int score = int.Parse(parts[1]);
            Version version = (parts[3] == "Full") ? Version.Full : Version.Lite;

            this.username = username;
            this.score = score;
            this.version = version;

            hash = Highscores.GetUsernameHash(username);
        }
    }

    public static Highscore Null = new Highscore("", 0, Version.Lite);

    public static bool operator ==(Highscore h1, Highscore h2) {
        return h1.username == h2.username && h1.score == h2.score && h1.version == h2.version;
    }
    public static bool operator !=(Highscore h1, Highscore h2) {
        return !(h1 == h2);
    }
    public override bool Equals(object obj) {
        return base.Equals(obj);
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }

    public override string ToString() {
        return "<Highscore> " + username + ": " + score + " - " + version.ToString();
    }
}
