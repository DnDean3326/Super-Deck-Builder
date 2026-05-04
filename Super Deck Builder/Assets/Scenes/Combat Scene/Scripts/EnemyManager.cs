using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private Encounter testEncounter;
    
    [SerializeField] private EnemyInfo[] allEnemies;
    [SerializeField] private List<Enemy> currentEnemies;
    
    private static GameObject _instance;

    private void Awake()
    {
        if (_instance != null) {
            Destroy(gameObject);
        } else {
            _instance = gameObject;
        }
        
        DontDestroyOnLoad(gameObject);
    }

    public void GenerateTestEncounter()
    {
        GenerateEnemiesByEncounter(testEncounter);
    }

    public void GenerateEnemiesByEncounter(Encounter tempEncounter)
    {
        currentEnemies.Clear();
        foreach (EnemyInfo encounterEnemy in tempEncounter.encounterEnemies) {
            // TODO add level scaling so future worlds get tougher
            GenerateEnemyByName(encounterEnemy.enemyName);
        }
    }
    
    private void GenerateEnemyByName(string memberName)
    {
        for (int i = 0; i < allEnemies.Length; i++) {
            if (allEnemies[i].enemyName == memberName) {
                Enemy newEnemy = new Enemy(allEnemies[i]);
                currentEnemies.Add(newEnemy);
            }
        }
    }
    
    public List<Enemy> GetCurrentEnemies()
    {
        return currentEnemies;
    }
}

[System.Serializable]
public class Enemy
{
    public string enemyName;

    public int maxHealth;
    public int currentHealth;
    public int maxArmor;
    public int currentArmor;
    
    public NpcBrain myBrain;
    public GameObject enemyBattleVisualPrefab;

    public Enemy(EnemyInfo enemyInfo)
    {
        enemyName = enemyInfo.enemyName;
        maxHealth = enemyInfo.baseHealth;
        currentHealth = maxHealth;
        maxArmor = enemyInfo.baseArmor;
        currentArmor = maxArmor;
        myBrain = enemyInfo.myBrain;
        enemyBattleVisualPrefab = enemyInfo.enemyBattleVisualPrefab;
    }
}