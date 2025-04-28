using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class DialogueFile { public List<DialogueNode> nodes; }

[System.Serializable]
public class DialogueNode
{
    public string body;
    public string background;            // file-stem: “bb_hospital”
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
}

