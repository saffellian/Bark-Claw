using System.Collections;
using UnityEngine;

interface IEnemy
{
    IEnumerator BrainLogic();
    IEnumerator Patrol();
    IEnumerator Attack();
    IEnumerator Death();
    void ApplyDamage(int amount);
}
