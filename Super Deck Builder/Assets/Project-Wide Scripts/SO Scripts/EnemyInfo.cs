using UnityEngine;

[CreateAssetMenu(fileName = "EnemyInfo", menuName = "Scriptable Objects/EnemyInfo")]
public class EnemyInfo : ScriptableObject
{
    public string enemyName;

    public int baseHealth;
    public int baseArmor;
    
    [TextArea(15,20)]
    public string enemyDescription;
}
