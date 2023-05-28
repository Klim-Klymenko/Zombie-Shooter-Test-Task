using UnityEngine;

public class OnBulletSpawned : MonoBehaviour
{
    [SerializeField] private GameObject bulletHole;
    [SerializeField] private GameObject bloodParticle;

    //Start method once prefab of shell has been spawned
    private void OnEnable() => Shooting();

    private void Shooting()
    {
        //Provide ray via centre of the screen to check whether we got something
        Ray ray = Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f));

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            //If we shoot in zombie
            if (hitInfo.collider.TryGetComponent(out EnemyController enemyController))
            {
                //Subtract zombie health and spawn particle of blood splash if zombie is alive
                if (enemyController.zombieHealthBar > EnemyController.zombieDamage)
                {
                    enemyController.onZombieDamaged?.Invoke();
                    
                    Instantiate(bloodParticle, hitInfo.point, Quaternion.LookRotation(-hitInfo.normal));
                }

                //If zombie is killed, calls the event with death mechanic
                else
                    enemyController.onZombieKilled?.Invoke(hitInfo.collider.gameObject);
            }

            //If we shot in hitable object, we spawn a bullet trace (hole)
            else if (hitInfo.collider.CompareTag("Hitable"))
                Instantiate(bulletHole, hitInfo.point, Quaternion.LookRotation(-hitInfo.normal));
        }
    }
}
