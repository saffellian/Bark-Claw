using System.Collections;
using UnityEngine;

public class ObjectiveItem : MonoBehaviour
{
    public enum CollectAction {
        TriggerBox,
        OnDestroy
    }

    public CollectAction collectAction = CollectAction.TriggerBox;
    public string objectiveId = string.Empty;


    private void OnTriggerEnter(Collider other) {
        if (collectAction == CollectAction.TriggerBox && other.gameObject.CompareTag("Player"))
        {
            ObjectiveHandler.Instance.IncrementObjective(objectiveId);
            Destroy(gameObject);
        }
    }

    private void OnDestroy() {
        if (collectAction == CollectAction.OnDestroy)
        {
            ObjectiveHandler.Instance.IncrementObjective(objectiveId);
        }
    }
}
