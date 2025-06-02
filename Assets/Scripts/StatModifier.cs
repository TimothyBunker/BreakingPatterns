using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StatRange
{
    public int baseValue;
    public int variance;
    
    public StatRange(int baseValue, int variance = 0)
    {
        this.baseValue = baseValue;
        this.variance = variance;
    }
    
    public int Roll()
    {
        if (variance == 0) return baseValue;
        return baseValue + Random.Range(-variance, variance + 1);
    }
}

public enum CriticalType
{
    None,
    Success,
    Failure
}

public class StatModifier : MonoBehaviour
{
    [SerializeField] private float criticalSuccessChance = 0.1f;
    [SerializeField] private float criticalFailureChance = 0.05f;
    [SerializeField] private float criticalMultiplier = 2f;
    
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = GameManager.Instance;
    }
    
    public StatChangeResult ApplyStatChanges(DialogueOption option)
    {
        var result = new StatChangeResult();
        var critical = RollCritical();
        result.criticalType = critical;
        
        // Calculate relationship modifier
        float relationshipModifier = CalculateRelationshipModifier();
        
        // Apply profit changes
        if (option.statChanges.profit != 0)
        {
            var profitRange = new StatRange(option.statChanges.profit, Mathf.Abs(option.statChanges.profit) / 3);
            int profitChange = profitRange.Roll();
            
            // Apply critical modifier
            if (critical == CriticalType.Success && profitChange > 0)
                profitChange = Mathf.RoundToInt(profitChange * criticalMultiplier);
            else if (critical == CriticalType.Failure && profitChange > 0)
                profitChange = Mathf.RoundToInt(profitChange * 0.5f);
            
            result.profitChange = profitChange;
            result.profitExpected = option.statChanges.profit;
        }
        
        // Apply relationship changes
        if (option.statChanges.relationships != 0)
        {
            var relRange = new StatRange(option.statChanges.relationships, Mathf.Abs(option.statChanges.relationships) / 4);
            int relChange = relRange.Roll();
            
            if (critical == CriticalType.Success && relChange > 0)
                relChange = Mathf.RoundToInt(relChange * criticalMultiplier);
            else if (critical == CriticalType.Failure)
                relChange = Mathf.RoundToInt(relChange * 0.5f);
            
            result.relationshipChange = relChange;
            result.relationshipExpected = option.statChanges.relationships;
        }
        
        // Apply suspicion changes with relationship modifier
        if (option.statChanges.suspicion != 0)
        {
            var susRange = new StatRange(option.statChanges.suspicion, Mathf.Abs(option.statChanges.suspicion) / 4);
            int susChange = susRange.Roll();
            
            // High relationships reduce suspicion gains
            if (susChange > 0 && relationshipModifier > 0)
            {
                susChange = Mathf.RoundToInt(susChange * (1f - relationshipModifier * 0.3f));
            }
            
            if (critical == CriticalType.Failure && susChange > 0)
                susChange = Mathf.RoundToInt(susChange * criticalMultiplier);
            
            result.suspicionChange = susChange;
            result.suspicionExpected = option.statChanges.suspicion;
        }
        
        return result;
    }
    
    private CriticalType RollCritical()
    {
        float roll = Random.value;
        if (roll < criticalFailureChance)
            return CriticalType.Failure;
        else if (roll < criticalFailureChance + criticalSuccessChance)
            return CriticalType.Success;
        return CriticalType.None;
    }
    
    private float CalculateRelationshipModifier()
    {
        // Returns 0-1 based on relationship level
        return gameManager.Relationships / 100f;
    }
    
    public bool CheckRelationshipGate(int threshold)
    {
        return gameManager.Relationships >= threshold;
    }
    
    public float GetSuspicionReductionFromRelationships()
    {
        // At 100 relationships, reduce suspicion gain by 30%
        return gameManager.Relationships / 100f * 0.3f;
    }
}

[System.Serializable]
public class StatChangeResult
{
    public int profitChange;
    public int profitExpected;
    public int relationshipChange;
    public int relationshipExpected;
    public int suspicionChange;
    public int suspicionExpected;
    public CriticalType criticalType;
    
    public bool HasVariance()
    {
        return profitChange != profitExpected || 
               relationshipChange != relationshipExpected || 
               suspicionChange != suspicionExpected;
    }
}