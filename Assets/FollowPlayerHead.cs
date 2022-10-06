using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerHead : MonoBehaviour
{
    public Transform head;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = head.position + head.forward * 1f;
        transform.rotation = Quaternion.LookRotation(transform.position - head.position);
    }
}
