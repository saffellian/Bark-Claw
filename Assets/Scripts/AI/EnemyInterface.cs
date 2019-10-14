using System;
using System.Collections;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected abstract IEnumerator BrainLogic();
    protected abstract IEnumerator Patrol();
    protected abstract IEnumerator Attack();
    protected abstract IEnumerator Death();
    public abstract void ApplyDamage(int amount);
    protected abstract void PlayerDied();

    public abstract void InstantDeath(bool explode);
}
