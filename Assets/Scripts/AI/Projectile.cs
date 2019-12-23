using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private List<string> _noCollideTags = new List<string>();
    private List<GameObject> _noCollideObjects = new List<GameObject>();

    public void RegisterNoCollideObject(GameObject obj)
    {
        _noCollideObjects.Add(obj);
    }

    public void RemoveNoCollideTag(GameObject obj)
    {
        _noCollideObjects.RemoveAll(o => o == obj);
    }

    public void RegisterNoCollideTag(string tagName)
    {
        _noCollideTags.Add(tagName);
    }

    public void RemoveNoCollideTag(string tagName)
    {
        _noCollideTags.RemoveAll(t => t == tagName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_noCollideTags.Contains(other.tag) || _noCollideObjects.Contains(other.gameObject))
            return;

        if (other.transform.CompareTag("Player"))
        {
            PlayerHealth.Instance.ApplyDamage(damage);
            damage = 0;
            Destroy(gameObject);
        }
        else
        {
            // explode
            Destroy(gameObject);
        }
        Debug.Log(other.name);
    }
}
