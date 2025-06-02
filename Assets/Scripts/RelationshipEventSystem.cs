using UnityEngine;
using System.Collections.Generic;

public class RelationshipEventSystem : MonoBehaviour
{
    [System.Serializable]
    public class RelationshipEvent
    {
        public string eventName;
        public int relationshipThreshold;
        public bool requiresHighRelationship;
        public string triggerDialogueNode;
        public string message;
        public StatChanges bonusRewards;
    }
    
    [SerializeField] private List<RelationshipEvent> relationshipEvents = new List<RelationshipEvent>();
    private HashSet<string> triggeredEvents = new HashSet<string>();
    private DialogueManager dialogueManager;
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = GameManager.Instance;
        dialogueManager = FindObjectOfType<DialogueManager>();
        InitializeDefaultEvents();
    }
    
    void InitializeDefaultEvents()
    {
        // High relationship events
        relationshipEvents.Add(new RelationshipEvent
        {
            eventName = "jesse_loyalty",
            relationshipThreshold = 75,
            requiresHighRelationship = true,
            message = "Jesse's loyalty reduces heat from the DEA!",
            bonusRewards = new StatChanges { suspicion = -10 }
        });
        
        relationshipEvents.Add(new RelationshipEvent
        {
            eventName = "partner_bonus",
            relationshipThreshold = 80,
            requiresHighRelationship = true,
            message = "Your trusted partners bring in extra profit!",
            bonusRewards = new StatChanges { profit = 20 }
        });
        
        // Low relationship events
        relationshipEvents.Add(new RelationshipEvent
        {
            eventName = "betrayal_risk",
            relationshipThreshold = 20,
            requiresHighRelationship = false,
            message = "Someone ratted you out to the DEA!",
            bonusRewards = new StatChanges { suspicion = 15 }
        });
        
        relationshipEvents.Add(new RelationshipEvent
        {
            eventName = "partner_abandonment",
            relationshipThreshold = 15,
            requiresHighRelationship = false,
            message = "Your partners are abandoning the operation!",
            bonusRewards = new StatChanges { profit = -25 }
        });
    }
    
    public void CheckRelationshipEvents()
    {
        foreach (var evt in relationshipEvents)
        {
            if (triggeredEvents.Contains(evt.eventName)) continue;
            
            bool shouldTrigger = evt.requiresHighRelationship ? 
                gameManager.Relationships >= evt.relationshipThreshold :
                gameManager.Relationships <= evt.relationshipThreshold;
                
            if (shouldTrigger && Random.value < 0.5f) // 50% chance when conditions are met
            {
                TriggerRelationshipEvent(evt);
            }
        }
    }
    
    void TriggerRelationshipEvent(RelationshipEvent evt)
    {
        triggeredEvents.Add(evt.eventName);
        
        // Show feedback
        var feedbackSystem = gameManager.GetComponent<FeedbackSystem>();
        if (feedbackSystem != null)
        {
            // Create a fake stat result to show the event
            var result = new StatChangeResult
            {
                profitChange = evt.bonusRewards.profit,
                relationshipChange = evt.bonusRewards.relationships,
                suspicionChange = evt.bonusRewards.suspicion,
                criticalType = evt.requiresHighRelationship ? CriticalType.Success : CriticalType.Failure
            };
            
            feedbackSystem.ShowStatChangeFeedback(result, new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }
        
        // Apply the rewards/penalties
        gameManager.ApplyChoice(evt.bonusRewards.profit, evt.bonusRewards.relationships, evt.bonusRewards.suspicion);
        
        // Show message (you might want to integrate this with your dialogue system)
        Debug.Log($"Relationship Event: {evt.message}");
    }
    
    public void ResetEvents()
    {
        triggeredEvents.Clear();
    }
}