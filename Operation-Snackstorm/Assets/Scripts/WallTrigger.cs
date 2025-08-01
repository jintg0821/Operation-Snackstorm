using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallTrigger : MonoBehaviour
{
    public GameObject[] fireExtinguishers;
    public float searchRadius;
    public float explodeChance;

    void Start()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius);
        List<GameObject> result = new List<GameObject>();

        foreach (var hit in hits)
        {
            FireExtinguisher extinguisher = hit.GetComponent<FireExtinguisher>();
            if (extinguisher != null)
                result.Add(hit.gameObject);
        }
        fireExtinguishers = result.ToArray();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (fireExtinguishers.Length == 0) return;

            if (Random.value < explodeChance)
            {
                int randomNum = Random.Range(0, fireExtinguishers.Length);
                FireExtinguisher extinguisher = fireExtinguishers[randomNum].GetComponent<FireExtinguisher>();
                extinguisher.FireExtinguisherExplode();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}