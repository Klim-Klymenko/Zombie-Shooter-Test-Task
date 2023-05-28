using TMPro;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //variables for EnemyController access
    public Slider _healthSlider;
    public TextMeshProUGUI _healthText;
    public TextMeshProUGUI _scoreText;
    public GameObject _dropdownMenu;

    public Transform _player;

    public Animator _bloodEffectAnim;

    public AudioClip _gameOverClip;
    public AudioClip _zombieDeathClip;
    public AudioClip _playerDamageClip;
    public AudioClip _zombieDamageClip;

    public static int attackTime = 1500;

    //If we restart the game, we need to continue setting the scale on 1 unit
    private void Start() => Time.timeScale = 1.0f;

    //method calls if player takes hit
    public static async void PlayerTakingHit(Slider healthSlider, Animator animator, AudioSource audioSource, AudioClip hurtClip, TextMeshProUGUI text)
    {
        healthSlider.value -= EnemyController.zombieDamage;
        audioSource.PlayOneShot(hurtClip);
        animator.SetBool("Hit", true);
        text.text = $"{healthSlider.value} / 100";

        await Task.Delay(attackTime);

        animator.SetBool("Hit", false);
    }
}
