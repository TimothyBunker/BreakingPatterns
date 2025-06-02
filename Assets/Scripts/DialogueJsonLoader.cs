using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class DialogueJsonLoader : MonoBehaviour
{
    /* names inside StreamingAssets */
    public string mainFileName = "breaking_patterns.json";
    public string deckFileName = "side_events.json";

    public DialogueManager targetManager;

    void Start()
    {
        /* -------- main storyline -------- */
        string mainPath = Path.Combine(Application.streamingAssetsPath, mainFileName);
        if (!File.Exists(mainPath)) { Debug.LogError($"Main JSON missing: {mainPath}"); return; }

        DialogueFile mainData = JsonUtility.FromJson<DialogueFile>(File.ReadAllText(mainPath));
        if (mainData == null || mainData.nodes == null || mainData.nodes.Count == 0)
        { Debug.LogError("Main JSON parse failed"); return; }

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
        targetManager.InitDecks(mainData.nodes, deck);
    }

    /* wrappers for JsonUtility */
    [System.Serializable] class SideEventFile { public SideEvent[] events; }
}
