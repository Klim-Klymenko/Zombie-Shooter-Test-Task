using Opsive.UltimateCharacterController.Character;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using TMPro;
using System;
using Opsive.Shared.Input;

public class EnemyController : MonoBehaviour
{
    //UI variables
    private Slider healthSlider;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI scoreText;
    private GameObject dropdownMenu;

    //player coordinates which we will obtain in script
    private Transform player;

    //zombie damage per one attack
    public static int zombieDamage = 25;
    //killed amount of zombies for current games session
    public static int killedZombiesAmount;
    //healthbar of zombie
    public int zombieHealthBar = 100;
    //duration of anim of zombie's taking hit
    private int delayTime = 1800;
    //time of zombie hit while attack
    private int swingTime = 1000;
    //full attack time of zombie
    private int fullAttackTime = 2000;
    //how many time zombie falls after it is killed
    private int fallingTime = 2500;

    //checks wheter zombie is killed
    private bool isKilled;
    //checks our anim state of walking
    private bool isWalking = true;
    //checks if zombie attacks
    private bool isExecuting;

    //animator of UI blood on the screen
    private Animator bloodEffectAnim;
    //animator of zombie
    private Animator animator;

    //zombie's NavMeshAgent
    private NavMeshAgent agent;

    //zombie's audio source
    private AudioSource audioSource;
    //camera's audio source
    private AudioSource cameraAudioSource;
    //clip played when the player is killed
    private AudioClip gameOverClip;
    //zombie death clip
    private AudioClip zombieDeathClip;
    //clip sounds once the player is hited
    private AudioClip playerDamageClip;
    //sounds once zombie is hited
    private AudioClip zombieDamageClip;

    //action called when zombie is damaged
    public Action onZombieDamaged;
    //calls when player is killed
    public static Action onPlayerKilled;
    //calls when zombie is killed
    public Action<GameObject> onZombieKilled;
    //calls when player is damaged
    public Action <Slider, Animator, AudioSource, AudioClip, TextMeshProUGUI> onPlayerDamaged;

    private UnityInput unityInput;
    private UltimateCharacterLocomotion ultimateCharacter;

    private void OnEnable()
    {
        //accessing game manager with needed variables which we can acces due to the prefab mode
        GameObject gameManager = GameObject.Find("Game Manager");

        //accessing scripts with needed variables
        PlayerController playerController = gameManager.GetComponent<PlayerController>();
        //accesing variables 
        agent = GetComponent<NavMeshAgent>();
        cameraAudioSource = Camera.main.GetComponent<AudioSource>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        //caching them
        healthSlider = playerController._healthSlider;
        healthText = playerController._healthText;
        scoreText = playerController._scoreText;
        dropdownMenu = playerController._dropdownMenu;
        player = playerController._player;
        bloodEffectAnim = playerController._bloodEffectAnim;

        gameOverClip = playerController._gameOverClip;
        zombieDeathClip = playerController._zombieDeathClip;
        playerDamageClip = playerController._playerDamageClip;
        zombieDamageClip = playerController._zombieDamageClip;

        //subscribing for some events
        onPlayerKilled = null;
        onZombieDamaged += TakingHit;
        onZombieKilled += EnemyDeath;
    }

    //unsubscribing from the events
    private void OnDisable()
    {
        onZombieDamaged -= TakingHit;
        onZombieKilled -= EnemyDeath;
    }

    //calling movement of zombie using NavMeshAgent
    private void Update() => Movement();

    //if zombie's trigger collides with player, we start subscribe on player's taking hit event, hence, zombie
    //doesn't move as it starts attacking
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out CapsuleColliderPositioner playerController))
        {
           if (onPlayerDamaged == null) onPlayerDamaged += PlayerController.PlayerTakingHit;

           isWalking = false;
        }
    }

    //while player is in trigger of zonbie, we zombie attack him
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent(out CapsuleColliderPositioner playerController) && !isKilled)
        {
            EnemyAttack();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if we quit from the player we start walking and stops taking the player's damage
        if (other.gameObject.TryGetComponent(out CapsuleColliderPositioner playerController))
        {
            animator.SetBool("Walk", true);
            animator.SetBool("Attack", false);
            animator.SetBool("Take Hit", false);

            if (agent.enabled) agent.isStopped = false;
            isWalking = true;
            onPlayerDamaged = null;
        }
    }

    //zonbie's movements using NavMesh
    private void Movement() => agent.SetDestination(player.position);

    //Method of getting hited by player
    private async void TakingHit()
    {
        //stops hitting player if we didn't unsubscribe before
        onPlayerDamaged = null;

        //zombie takes damage
        TakingDamage(ref zombieHealthBar, zombieDamage, audioSource, zombieDamageClip);

        //animator starts playing from begginig if we call it when it taking hit animation is not finished
        animator.Rebind();

        //satrts animation of taking the hit
        animator.SetBool("Take Hit", true);
        animator.SetBool("Walk", false);
        animator.SetBool("Attack", false);
         
        //wait until animation finished
        await Task.Delay(delayTime);

        //turning it off
        if (animator != null) animator.SetBool("Take Hit", false);

        //if we are supposed to walk, we continue walking
        if (isWalking && !isExecuting)
        {
            if (animator != null) animator.SetBool("Attack", false);
            if (animator != null) animator.SetBool("Walk", true);
        }

        //if player is in zombie's trigger, we continue attacking
        else if (!isWalking)
        {
            if (animator != null) animator.SetBool("Attack", true);
            if (animator != null) animator.SetBool("Walk", false);
        }

        //wait 2 sec to prevent bug of taking an extra hit and then giving an opportunity to player to be hited 
        //if zombie hits him
        await Task.Delay(fullAttackTime);

        if (onPlayerDamaged == null) onPlayerDamaged += PlayerController.PlayerTakingHit;
    }

    //method for subtracting health and playing certain clip
    private void TakingDamage(ref int healthbar, int damage, AudioSource audioSource, AudioClip damageClip)
    {
        healthbar -= damage;
        audioSource.PlayOneShot(damageClip);
    }
    //zombie's attack
    public async void EnemyAttack()
    {
        //calling it one time per one full execution
        if (!isExecuting)
        {
            //zombie does't walk
            isWalking = false;

            //stop calling it until zombie finished one attack
            isExecuting = true;

            //stop player
            if (agent.enabled) agent.isStopped = true;

            //turnin on attack anim
            animator.SetBool("Attack", true);
            animator.SetBool("Walk", false);
            animator.SetBool("Take Hit", false);

            //wait until zombie swings and kinda hits player
            await Task.Delay(swingTime);

            //check whether player is alive and if yes, taking damage is called
            if (healthSlider.value > 25)
                onPlayerDamaged?.Invoke(healthSlider, bloodEffectAnim, audioSource, playerDamageClip, healthText);

            //if not already and if player is in zombie's trugger, dropdown menu is shown and game is finoshed
            else
            {
                if (!isWalking) PlayerDied();
            }

            //waiting for full end of attack animation
            await Task.Delay(fullAttackTime);

            //stop executing attack effect
            isExecuting = false;
        }
    }

    private void PlayerDied()
    {
        //getting and switching off built in components of TPC
        if (player != null) unityInput = player.GetComponent<UnityInput>();
        if (player != null) ultimateCharacter = player.GetComponent<UltimateCharacterLocomotion>();
        
        unityInput.enabled = false;
        ultimateCharacter.enabled = false;

        //displaying UI correctly with killed amount of zombies
        healthSlider.value = 0;
        scoreText.text = $"Your score is: {killedZombiesAmount}";
        healthText.text = "0 / 100";

        //truning on dropdown menu
        dropdownMenu.SetActive(true);

        //playing "game over" sound
        cameraAudioSource.Stop();
        cameraAudioSource.PlayOneShot(gameOverClip);

        //unlocking a cursor and freezing the game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0.0f;
    }

    public async void EnemyDeath(GameObject enemy)
    {
        //getting rigidbody of all ragdoll's bons and colliders
        Rigidbody[] bones = enemy.GetComponentsInChildren<Rigidbody>();
        CapsuleCollider[] bonesCapsuleCollider = enemy.GetComponentsInChildren<CapsuleCollider>();
        BoxCollider[] bonesBoxCollider = enemy.GetComponentsInChildren<BoxCollider>();

        //getting rigidbody of zombie and other zombie's components for managing them
        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
        BoxCollider boxCollider = enemy.GetComponent<BoxCollider>();
        CapsuleCollider capsuleCollider = enemy.GetComponent<CapsuleCollider>();
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        Animator animator = enemy.GetComponent<Animator>();
        EnemyController enemyController = enemy.GetComponent<EnemyController>();

        //sound of death sounds and counter of killed zombies ammount increases as well and player isn't already damaged
        audioSource.PlayOneShot(zombieDeathClip);
        killedZombiesAmount++;
        onPlayerDamaged = null;

        //turning on ragdoll
        foreach (Rigidbody i in bones) i.isKinematic = false;
        foreach (CapsuleCollider i in bonesCapsuleCollider) i.enabled = true;
        foreach (BoxCollider i in bonesBoxCollider) i.enabled = true;

        //finishing death of zombie turning off other needed components
        enemyRb.constraints = RigidbodyConstraints.FreezeAll;
        capsuleCollider.enabled = false;
        boxCollider.enabled = false;
        enemyController.isKilled = true;
        enemyController.enabled = false;
        agent.enabled = false;
        animator.enabled = false;

        //wait until zonbie falls
        await Task.Delay(fallingTime);

        //switching off colliders to allow any objects go through killed zombie
        for (int i = 0; i < bonesCapsuleCollider.Length; i++)
        {
            if (bonesCapsuleCollider[i] == null) break;
            bonesCapsuleCollider[i].enabled = false;
        }

        for (int i = 0; i < bonesBoxCollider.Length; i++)
        {
            if (bonesBoxCollider[i] == null) break;
            bonesBoxCollider[i].enabled = false;
        }

        //freezing ragdoll
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] == null) break;
            bones[i].isKinematic = true;
        }
    }
}
