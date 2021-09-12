using UnityEngine;

[CreateAssetMenu(fileName = "Objective", menuName = "ScriptableObjects/Objective", order = 1)]
public class Objective : ScriptableObject
{
    public string id = "Objective Unique ID";
    public int goalAmount = 5;
    public int currentAmount = 0;
}
