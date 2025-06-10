using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class DialogueFile { public List<DialogueNode> nodes; }

[System.Serializable]
public class StatChanges
{
    public int profit;
    public int relationships;
    public int suspicion;
}

[System.Serializable]
public class DialogueNode
{
    public int id;                       // Node ID for navigation
    public string body;
    public string background;            // file-stem: �bb_hospital�
    public string charLeft;
    public List<DialogueOption> options; // 1-4 entries

}

[System.Serializable]
public class DialogueOption
{
    public string text;
    public int profit;
    public int relationships;
    public int suspicion;
    public int nextNode;              // -1 ends story
    
    // New fields for enhanced mechanics
    public StatChanges statChanges => new StatChanges { profit = profit, relationships = relationships, suspicion = suspicion };
    public int minRelationship = 0;   // Minimum relationship required
    public int maxRelationship = 100; // Maximum relationship allowed
    public bool isHidden = false;     // Hide option unless conditions met
}

[System.Serializable]
public class SideEvent
{
    public string body;
    public string background;
    public string charLeft;
    public List<DialogueOption> options;  // was DialogueOption[]
    public int minSuspicion = 0;
    public int maxScene = 10;
    public string tag;
    
    // New relationship-based conditions
    public int minRelationship = 0;
    public int maxRelationship = 100;
    public float rareChance = 0.1f;    // Chance for rare events (default 10%)
}

