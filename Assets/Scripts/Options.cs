using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour {

    public UI UI;
    public Highscores highscores;
    public SceneSwitcher sceneSwitcher;

    [Header("Options")]
    public string defaultSkin;
    public string defaultBackground;

    [Header("Options Screen")]
    public AutoLayoutGroup skinSelector;
    public AutoLayoutGroup backgroundSelector;
    public RuntimeAnimatorController optionButtonAnimatorController;

    public Color selectedColour;
    public List<Image> skinnedImages = new List<Image>();

    [Header("Username Select Screen")]
    public InputField usernameField;
    public Button doneButton;
    public Text mainErrorText;

    enum Option { Skin, Background }

    #region Options
    public static Sprite[] skins { get; protected set; }
    public static Sprite skin { get; protected set; }
    Dictionary<string, Button> skinButtons = new Dictionary<string, Button>();

    public static Sprite[] backgrounds { get; protected set; }
    public static Sprite background { get; protected set; }
    Dictionary<string, Button> backgroundButtons = new Dictionary<string, Button>();

    public static string username { get; protected set; }
    #endregion

    void Awake() {
        UpdateSkins();
        UpdateBackgrounds();

        SetUsername(PlayerPrefs.GetString("Username", ""));
    }

    void Start() {
        StartCoroutine(CheckUsername());
    }

    IEnumerator CheckUsername() {
        yield return null;

        if (string.IsNullOrEmpty(username)) {
            doneButton.interactable = false;
            UI.SetPage("Username", true);
        } else {
            usernameField.readOnly = true;
        }
    }

    [ContextMenu("Update Skins")]
    public void UpdateSkins() {
        UpdateOption(Option.Skin);
    }
    [ContextMenu("Update Backgrounds")]
    public void UpdateBackgrounds() {
        UpdateOption(Option.Background);
    }

    void UpdateOption(Option option) {
        AutoLayoutGroup selector = null;
        Sprite[] list = null;
        Dictionary<string, Button> buttons = new Dictionary<string, Button>();
        string def = "";
        switch (option) {
            case (Option.Skin):
                selector = skinSelector;
                def = defaultSkin;
                break;
            case (Option.Background):
                selector = backgroundSelector;
                def = defaultBackground;
                break;
        }

        list = Resources.LoadAll<Sprite>(option.ToString() + "s");
        selector.ClearChildren();

        foreach (Sprite sprite in list) {
            Image image = new GameObject(sprite.name).AddComponent<Image>();
            image.sprite = sprite;

            image.color = Color.white;
            image.material = null;
            image.raycastTarget = true;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;

            Animator animator = image.gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = optionButtonAnimatorController;
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            Button button = image.gameObject.AddComponent<Button>();
            button.onClick.AddListener(delegate () { ChooseSprite(option, sprite); });
            button.transition = Selectable.Transition.Animation;
            button.image = image;

            UIAudio audio = image.gameObject.AddComponent<UIAudio>();
            audio.element = UIAudio.Element.Button;

            buttons.Add(sprite.name, button);

            if (sprite.name == PlayerPrefs.GetString(option.ToString(), def)) {
                image.color = selectedColour;

                switch (option) {
                    case (Option.Skin):
                        skin = sprite;
                        foreach (Image i in skinnedImages) {
                            i.sprite = sprite;
                        }
                        break;
                    case (Option.Background):
                        background = sprite;
                        Background.UpdateBackgrounds();
                        break;
                }
            }

            image.transform.SetParent(selector.transform);
        }

        switch (option) {
            case (Option.Skin):
                skins = list;
                skinButtons = buttons;
                break;
            case (Option.Background):
                backgrounds = list;
                backgroundButtons = buttons;
                break;
        }

        selector.ReCalculate();
    }

    void ChooseSprite(Option option, Sprite sprite) {
        Dictionary<string, Button> buttons = null;
        switch (option) {
            case (Option.Skin):
                skin = sprite;
                buttons = skinButtons;
                foreach (Image i in skinnedImages) {
                    i.sprite = sprite;
                }
                break;
            case (Option.Background):
                background = sprite;
                buttons = backgroundButtons;
                break;
        }

        if (buttons.ContainsKey(sprite.name)) {
            foreach (KeyValuePair<string, Button> b in buttons) {
                b.Value.image.color = Color.white;
            }
            buttons[sprite.name].image.color = selectedColour;

            if (option == Option.Background) Background.UpdateBackgrounds();

            PlayerPrefs.SetString(option.ToString(), sprite.name);
        }
    }

    #region Username Functions
    public void InputUsername(string newUsername) {
        if (!string.IsNullOrEmpty(newUsername)) {
            Debug.Log("Setting username to " + newUsername);

            SetUsername(newUsername);
            ClearError();
        } else {
            SetUsername("");
            ShowError("");
        }
    }

    public void Logout() {
        ResetUsername();
        sceneSwitcher.MainMenu();
    }

    void SetUsername(string username) {
        PlayerPrefs.SetString("Username", username);
        Options.username = usernameField.text = username;
    }
    void ShowError(string error) {
        doneButton.interactable = false;
        mainErrorText.color = Color.red;
        mainErrorText.text = error;
    }
    void ClearError() {
        doneButton.interactable = true;
        mainErrorText.text = "";
    }
    #endregion

    #region Reset Helpers
    [ContextMenu("Reset Username")]
    public void ResetUsername() {
        PlayerPrefs.DeleteKey("Username");
    }
    [ContextMenu("Reset Skin")]
    public void ResetSkins() {
        PlayerPrefs.DeleteKey("Skin");
    }
    [ContextMenu("Reset Background")]
    public void ResetBackground() {
        PlayerPrefs.DeleteKey("Background");
    }
    #endregion
}
