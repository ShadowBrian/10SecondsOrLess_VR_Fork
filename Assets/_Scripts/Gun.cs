using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Animator animator;

    private float lastFireTime;

    public Transform firepoint;
    public GameObject projectilePrefab;

    public bool CanFire => Time.time > lastFireTime + 0.5f;

    public AudioSource source;
    public AudioClip fireClip;

    public void Fire()
    {
        if (CanFire)
        {
            //animator.SetTrigger("Fire");
            lastFireTime = Time.time;

            source.pitch = Random.Range(0.95f, 1.05f);
            source.volume = 0.7f;
            source.PlayOneShot(fireClip);

            GameObject projectile = Instantiate(projectilePrefab, firepoint.transform.position, firepoint.transform.rotation);

            Projectile projectileComponent = projectile.GetComponent<Projectile>();
            projectileComponent.damage = 1;

            Vector3 direction = firepoint.forward;

            Enemy closestEnemy = null;
            float closestAngle = float.MaxValue;

            Vector3 forwardDirection = firepoint.forward;//(Camera.main.transform.position + (Camera.main.transform.forward * 40f)) - projectileComponent.transform.position;

            foreach (Enemy enemy in GameManager.Instance.allEnemies)
            {
                if (!enemy.killed)
                {
                    Vector3 directionToEnemy = enemy.transform.position - firepoint.position;

                    if (Vector3.Angle(firepoint.forward, directionToEnemy) < closestAngle)
                    {
                        closestAngle = Vector3.Angle(firepoint.forward, directionToEnemy);
                        closestEnemy = enemy;

                        Debug.Log(closestAngle);
                    }
                }
            }


            if (Mathf.Abs(closestAngle) > 8f || closestEnemy == null)
                direction = forwardDirection;
            else
                direction = closestEnemy.transform.position - firepoint.position;


            projectileComponent.LaunchProjectile(direction);
        }
    }
}
