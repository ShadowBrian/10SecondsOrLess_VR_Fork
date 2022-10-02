using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ProjectileType { TRANSLATION, PHYSICS };

    public ProjectileType projectileType;

   public Rigidbody body; 

    public float speed;
    public float damage;
    public float duration = 10f; 
    protected float startTime;
    protected Vector3 direction;

    public Collider collider;
    public Renderer renderer;
    public ParticleSystem particle;
    public TrailRenderer trail;

    public virtual void LaunchProjectile(Vector3 direction)
    {
        this.direction = direction; 
        startTime = Time.time;

        gameObject.SetActive(true);

        switch (projectileType)
        {
            case ProjectileType.TRANSLATION:

                break;
            case ProjectileType.PHYSICS:
                body.AddForce(direction.normalized * speed, ForceMode.Impulse);
                break;
        }
    }

    public virtual void Update()
    {
        if (projectileType == ProjectileType.TRANSLATION)
        {
            transform.position += direction.normalized * Time.deltaTime * speed;
        }


        if (Time.time > startTime + duration)
            Destroy(gameObject);
    }


    public void SetDamage(float damage)
    {
        this.damage = damage;
    }
        
    public virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Enemy enemy = other.transform.GetComponent<Enemy>();

            enemy.Kill(); 

        }

        StartCoroutine(DelayedDestroyGameObject());
    }

    public IEnumerator DelayedDestroyGameObject()
    {
        speed = 0;
        renderer.enabled = false; 
        collider.enabled = false;

        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }

    private bool PhysicsBased => projectileType == ProjectileType.PHYSICS;
}
