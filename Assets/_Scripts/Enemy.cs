using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Renderer renderer;
    public Collider collider;
    public ParticleSystem particles;
    public AudioSource source;
    public AudioClip gibSfx; 

    public bool killed = false;

    public void Kill()
    {
        particles.Play();
        killed = true; 

        source.Stop();  
        source.volume = .5f;
        source.pitch = Random.Range(0.95f, 1.05f);
        source.PlayOneShot(gibSfx);

        renderer.enabled = false; 
        collider.enabled = false;
        CameraShake.instance.shakeDuration = 0.5f;
    }

    public void OnTriggerEnter(Collider other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();

        if (controller)
        { 
            Kill();
        }
    }
}
