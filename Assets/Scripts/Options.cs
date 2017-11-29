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

    public InputField passwordField;

    [Header("Username Select Screen")]
    public InputField usernameField;
    public Button doneButton;
    public Text mainErrorText;
    [Space]
    public GameObject loginPanel;
    public InputField usernameLoginField;
    public InputField passwordLoginField;
    public Text loginErrorText;
    [Space]
    public Button logoutButton;
    public Button deleteAccoundButton;

    [Header("Other Screens")]
    public Button multiplayerButton;
    public GameObject passwordPanel;
    public InputField passwordPanelField;

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
            if (Application.internetReachability == NetworkReachability.NotReachable) {
                multiplayerButton.interactable = logoutButton.interactable = deleteAccoundButton.interactable = false;
                ShowError("No internet connection");
            }
            else {
                doneButton.interactable = false;
                UI.SetPage("Username", true);
            }
        }
        else {
            usernameField.readOnly = true;
            passwordField.gameObject.SetActive(true);
            passwordField.text = Highscores.GetUsernameHash(username).ToString();
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

                switch(option) {
                    case (Option.Skin):
                        skin = sprite;
                        foreach(Image i in skinnedImages) {
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
        doneButton.interactable = false;

        if (!string.IsNullOrEmpty(newUsername)) {
            Debug.Log("Checking username");
            highscores.ContainsUsername(newUsername, (bool containsNew) => {
                if (!containsNew) {
                    Debug.Log("Setting username to " + newUsername);

                    SetUsername(newUsername);
                    ClearError();

                    passwordPanel.SetActive(true);
                }
                else {
                    Debug.Log("Username taken");
                    ShowError("Username Taken");
                    usernameLoginField.text = newUsername;
                }
            });
        }
        else {
            SetUsername("");
            ShowError("");
            usernameLoginField.text = "";
        }
    }

    public void Login() {
        if (Highscores.GetUsernameHash(usernameLoginField.text) == float.Parse(passwordLoginField.text)) {
            highscores.ContainsUsername(usernameLoginField.text, (bool contains) => {
                if (contains) {
                    SetUsername(usernameLoginField.text);
                    Debug.Log("Loged in as " + username + " with the password " + passwordField.text);

                    ClearError();
                    mainErrorText.color = loginErrorText.color = Color.green;
                    mainErrorText.text = loginErrorText.text = "Logged in successfully";

                    loginPanel.SetActive(false);
                }
                else {
                    Debug.Log("Username not recognised");
                    loginErrorText.text = "Username or password incorrect";
                }
            });
        }
        else {
            Debug.Log("Password incorrect");
            loginErrorText.text = "Username or password incorrect";
        }
    }

    public void Logout() {
        ResetUsername();
        sceneSwitcher.MainMenu();
    }
    public void DeleteAccount() {
        highscores.ContainsUsername(username, (bool contatins) => {
            if(contatins) highscores.RemoveHighscore(username);
            ResetUsername();
            ResetSkins();
            ResetBackground();
            sceneSwitcher.MainMenu();
        });
    }

    void SetUsername(string username) {
        PlayerPrefs.SetString("Username", username);
        Options.username = usernameField.text = username;
        passwordField.text = passwordPanelField.text = Highscores.GetUsernameHash(username).ToString();
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
