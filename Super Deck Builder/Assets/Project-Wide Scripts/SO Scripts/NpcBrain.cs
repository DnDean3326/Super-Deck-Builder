using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyBrain", menuName = "Scriptable Objects/EnemyBrain")]
public class NpcBrain : ScriptableObject
{
    public enum TargetFaction
    {
        Party,
        Enemies
    }
    
    public List<NpcAbility> npcAbilities;
}

[System.Serializable]
public class NpcAbility
{
    public string abilityName;
    public List<EnemyIntents> relevantIntents;
    public List<SCardValueInfo> abilityValues;
    public NpcBrain.TargetFaction targetGroup;
}
