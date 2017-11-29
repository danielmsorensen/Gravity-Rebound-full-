using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour {

    public Animator faderAnimator;

    public virtual void MainMenu() {
        faderAnimator.SetInteger("Scene ID", 0);
        Fade();
    }
    public virtual void SinglePlayer() {
        faderAnimator.SetInteger("Scene ID", 1);
        Fade();
    }
    public virtual void Multiplayer() {
        faderAnimator.SetInteger("Scene ID", 2);
        Fade();
    }

    public virtual void Quit() {
        faderAnimator.SetInteger("Scene ID", -1);
        Fade();
    }

    protected void Fade() {
        faderAnimator.ResetTrigger("Fade To White");
        faderAnimator.ResetTrigger("Fade To Black");
        faderAnimator.SetTrigger("Fade To Black");
    }
}
