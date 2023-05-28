using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    //variables we need to drag to allow sound to be produced
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;

    //if we start new game, we reset killed amount of zombies as it is static, run game scene and call button sound
    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        EnemyController.killedZombiesAmount = 0;
        UIClipPlaying();
    }

    //qits the game
    public void Quit() => Application.Quit();

    //comming back to menu with called button sound
    public void MenuRunning()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        UIClipPlaying();
    }

    //if we restart new game, we reset killed amount of zombies as it is static, run game scene and call button sound
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        EnemyController.killedZombiesAmount = 0;
        UIClipPlaying();
    }

    //method for calling the button sound
    private void UIClipPlaying() => audioSource.PlayOneShot(clip);
}
