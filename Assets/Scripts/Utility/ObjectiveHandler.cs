using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ObjectiveHandler : MonoBehaviour
{
    public enum ObjectiveStatus {
        InProgress,
        Complete,
        ItemNotFound
    }

    [Serializable]
    public struct ObjectiveListItem {
        public Objective objective;
        public UnityEvent completeEvent;
    }

    private struct ObjectiveListItemInstance {
        public int currentAmount;
        public int goalAmount;
        public UnityEvent completeEvent;
    }

    public static ObjectiveHandler Instance;

    public List<ObjectiveListItem> objectives;

    private Dictionary<string, ObjectiveListItemInstance> objectiveDict = new Dictionary<string, ObjectiveListItemInstance>();

    void Start()
    {        
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        ObjectiveListItemInstance inst = new ObjectiveListItemInstance();
        foreach (var obj in objectives)
        {
            inst.currentAmount = obj.objective.currentAmount;
            inst.goalAmount = obj.objective.goalAmount;
            inst.completeEvent = obj.completeEvent;
            objectiveDict.Add(obj.objective.id, inst);
        }
    }

    public ObjectiveStatus IncrementObjective(string id)
    {
        ObjectiveListItemInstance inst;

        if (objectiveDict.TryGetValue(id, out inst))
        {
            if (inst.currentAmount < inst.goalAmount)
            {
                inst.currentAmount++;
                objectiveDict[id] = inst;
            }
            
            if (inst.currentAmount >= inst.goalAmount)
            {
                if (inst.currentAmount == inst.goalAmount)
                    inst.completeEvent.Invoke();
                    
                return ObjectiveStatus.Complete;
            }
            else
                return ObjectiveStatus.InProgress;
        }

        return ObjectiveStatus.ItemNotFound;
    }

}
