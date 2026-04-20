using UnityEngine;

public class GameEnums : MonoBehaviour
{
    
}

public enum CardTypes
{
    Attack,
    Skill,
    Power
}

public enum CardCharacter
{
    Immune,
    Ricochet
}

public enum CardValue
{
    Damage,
    Defense,
    Heal,
    CardDraw,
}

public enum ExtraCosts
{
    Health,
    Defense,
    Ammo
}

public enum EnemyIntents
{
    Attack,
    Defend,
    Buff,
    Debuff
}