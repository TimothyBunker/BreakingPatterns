using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class DialogueJsonLoader : MonoBehaviour
{
    /* names inside StreamingAssets */
    public string mainFileName = "breaking_patterns.json";
    public string deckFileName = "side_events.json";
    public string introFileName = "breaking_patterns_intro.json";

    public DialogueManager targetManager;

    void Start()
    {
        /* -------- intro sequence -------- */
        string introPath = Path.Combine(Application.streamingAssetsPath, introFileName);
        List<DialogueNode> allNodes = new List<DialogueNode>();
        
        // TEMPORARILY DISABLED - Testing without intro
        /*
        if (File.Exists(introPath))
        {
            IntroFile introData = JsonUtility.FromJson<IntroFile>(File.ReadAllText(introPath));
            if (introData != null && introData.introNodes != null)
            {
                allNodes.AddRange(introData.introNodes);
            }
        }
        */

        /* -------- main storyline -------- */
        string mainPath = Path.Combine(Application.streamingAssetsPath, mainFileName);
        if (!File.Exists(mainPath)) { Debug.LogError($"Main JSON missing: {mainPath}"); return; }

        DialogueFile mainData = JsonUtility.FromJson<DialogueFile>(File.ReadAllText(mainPath));
        if (mainData == null || mainData.nodes == null || mainData.nodes.Count == 0)
        { Debug.LogError("Main JSON parse failed"); return; }
        
        allNodes.AddRange(mainData.nodes);

        /* -------- side-event deck -------- */
        string deckPath = Path.Combine(Application.streamingAssetsPath, deckFileName);
        List<SideEvent> deck = new();

        if (File.Exists(deckPath))
        {
            SideEventFile deckData = JsonUtility.FromJson<SideEventFile>(File.ReadAllText(deckPath));
            if (deckData != null && deckData.events != null)
                deck = new List<SideEvent>(deckData.events);
        }

        if (!targetManager) targetManager = GetComponent<DialogueManager>();
        
        Debug.Log($"Loaded {allNodes.Count} total nodes. IDs: {string.Join(", ", allNodes.Select(n => n.id))}");
        
        targetManager.InitDecks(allNodes, deck);
    }

    /* wrappers for JsonUtility */
    [System.Serializable] class SideEventFile { public SideEvent[] events; }
    [System.Serializable] class IntroFile { public List<DialogueNode> introNodes; }
}
