using UnityEngine;
using UnityEngine.UI; // Required for ScrollRect, LayoutRebuilder if you add scrolling
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

public class DialogueManager : MonoBehaviour
{
    /* ────── Inspector ────── */
    [Header("UI")]
    public TMP_Text bodyLabel;
    public Image bgImage;
    public Image charLeftImage;
    public Image charRightImage;     // Walt remains here
    public ScrollRect bodyLabelScrollRect; // Assign if you implement scrolling for bodyLabel

    [Header("Resources")]
    public string backgroundsPath = "Sprites/Backgrounds/";
    public string charactersPath = "Sprites/Characters/";
    public string waltSpriteName = "WalterWhite_front";

    /* ────── Runtime ────── */
    List<DialogueNode> nodes;
    List<SideEvent> deck;
    List<DialogueOption> currentOpts;

    int nodeIdx, optionIdx;
    bool showingCard;
    readonly System.Random rng = new();
    
    RelationshipEventSystem relationshipEvents;

    /* ────── Public API called by loader ────── */
    public void InitDecks(List<DialogueNode> main, List<SideEvent> side)
    {
        nodes = main;
        deck = side ?? new List<SideEvent>();
        
        Debug.Log($"InitDecks: Received {nodes.Count} nodes");
        
        // Start with intro if it exists, otherwise start at node 0
        nodeIdx = -1;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].id == -10)
            {
                nodeIdx = i;
                Debug.Log($"Found intro node -10 at index {i}");
                break;
            }
        }
        if (nodeIdx == -1)
        {
            Debug.Log("No intro node found, starting at index 0");
            nodeIdx = 0; // Fallback to node 0 if no intro
        }
        
        optionIdx = 0;
        
        // Initialize relationship event system
        relationshipEvents = GetComponent<RelationshipEventSystem>() ?? gameObject.AddComponent<RelationshipEventSystem>();
        
        LoadWaltPortrait();
        ShowNode();
    }

    /* ────── Update ────── */
    void Update()
    {
        if (nodes == null || currentOpts == null || currentOpts.Count == 0) return; // Added check for currentOpts

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            optionIdx = (optionIdx - 1 + currentOpts.Count) % currentOpts.Count;
            AudioManager.Instance?.PlayUINavigationSound(); // Play navigation sound
            Redraw(); // Redraw to show selection
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            optionIdx = (optionIdx + 1) % currentOpts.Count;
            AudioManager.Instance?.PlayUINavigationSound(); // Play navigation sound
            Redraw(); // Redraw to show selection
        }

        // Check if optionIdx is valid before using it for Alpha keys
        if (optionIdx >= 0 && optionIdx < currentOpts.Count)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Alpha1 + optionIdx)) // Make sure Alpha1 + optionIdx is valid
            {
                OnOptionSelected(optionIdx);
            }
        }
        // Fallback for Return/Space if Alpha key was out of bounds (e.g. if currentOpts changed rapidly)
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            // Potentially select the first option or handle error, for now, let's assume optionIdx gets corrected
            // Or, if currentOpts is not empty, select the current (possibly wrapped) optionIdx
            if (currentOpts.Count > 0)
            {
                OnOptionSelected(optionIdx % currentOpts.Count);
            }
        }
    }

    /* ────── Drawing helpers ────── */
    void LoadWaltPortrait()
    {
        if (charRightImage == null) { Debug.LogError("charRightImage is not assigned!"); return; }
        Sprite w = Resources.Load<Sprite>(charactersPath + waltSpriteName);
        if (w != null)
        {
            charRightImage.sprite = w;
            charRightImage.enabled = true;
        }
        else
        {
            charRightImage.enabled = false;
            Debug.LogWarning($"Failed to load Walt portrait: {waltSpriteName}");
        }
    }

    void ShowNode()
    {
        if (nodes == null || nodeIdx < 0 || nodeIdx >= nodes.Count)
        {
            Debug.LogError($"ShowNode: Invalid node data or index. nodeIdx={nodeIdx}, nodes.Count={nodes?.Count ?? 0}");
            return;
        }
        showingCard = false;
        optionIdx = 0;
        var n = nodes[nodeIdx];
        Debug.Log($"Showing node at index {nodeIdx} with ID {n.id}: {n.body.Substring(0, Mathf.Min(50, n.body.Length))}...");
        DrawScreen(n.body, n.background, n.charLeft, n.options, false);
    }

    void ShowSideEvent(SideEvent ev)
    {
        if (ev == null)
        {
            Debug.LogError("ShowSideEvent: SideEvent is null.");
            return;
        }
        showingCard = true;
        optionIdx = 0;
        DrawScreen(ev.body, ev.background, ev.charLeft, ev.options, true);
    }

    void Redraw()
    {
        // Ensure currentOpts is not null and has items before redrawing
        if (currentOpts == null || currentOpts.Count == 0)
        {
            // It's possible we are in a state where there are no options (e.g. end of dialogue before scene change)
            // Or if currentCard/nodes[nodeIdx] is somehow invalid at this point.
            // For now, let's prevent error if currentOpts is bad.
            Debug.LogWarning("Redraw called with no current options available.");
            return;
        }

        if (showingCard)
        {
            if (currentCard == null) { Debug.LogError("Redraw: currentCard is null while showingCard is true."); return; }
            DrawScreen(currentCard.body, currentCard.background, currentCard.charLeft, currentOpts, true);
        }
        else
        {
            if (nodes == null || nodeIdx < 0 || nodeIdx >= nodes.Count) { Debug.LogError("Redraw: Invalid node data or index while not showing card."); return; }
            var n = nodes[nodeIdx];
            DrawScreen(n.body, n.background, n.charLeft, currentOpts, false);
        }
    }

    void DrawScreen(string body, string bg, string charL, List<DialogueOption> opts, bool isEvent)
    {
        // Ensure UI elements are assigned
        if (bodyLabel == null) { Debug.LogError("bodyLabel is not assigned in DialogueManager!"); return; }
        if (bgImage == null) { Debug.LogError("bgImage is not assigned in DialogueManager!"); } // Can be optional
        if (charLeftImage == null) { Debug.LogError("charLeftImage is not assigned in DialogueManager!"); } // Can be optional
        
        // Filter options based on relationship requirements
        var filteredOpts = new List<DialogueOption>();
        foreach (var opt in opts)
        {
            if (opt == null) continue;
            
            // Check relationship requirements
            bool meetsReqs = true;
            if (GameManager.Instance != null)
            {
                meetsReqs = GameManager.Instance.Relationships >= opt.minRelationship &&
                           GameManager.Instance.Relationships <= opt.maxRelationship;
            }
                           
            // Add visible options or hidden options that meet requirements
            if (!opt.isHidden || meetsReqs)
            {
                filteredOpts.Add(opt);
            }
        }
        
        // Ensure we have at least one option
        if (filteredOpts.Count == 0)
        {
            Debug.LogWarning("No valid options available! Adding a default option.");
            filteredOpts.Add(new DialogueOption 
            { 
                text = "Continue...", 
                nextNode = isEvent ? nodeIdx : -1 
            });
        }
        
        currentOpts = filteredOpts;
        if (optionIdx >= currentOpts.Count) optionIdx = 0;
        
        if (currentOpts == null)
        {
            Debug.LogError("DrawScreen called with null options list.");
            currentOpts = new List<DialogueOption>(); // Prevent further null errors
        }
        // Ensure optionIdx is valid for the new options
        if (optionIdx >= currentOpts.Count && currentOpts.Count > 0)
        {
            optionIdx = currentOpts.Count - 1;
        }
        else if (currentOpts.Count == 0)
        {
            optionIdx = 0; // Or handle no options state
        }


        if (bgImage != null && !string.IsNullOrEmpty(bg))
        {
            Sprite s = Resources.Load<Sprite>(backgroundsPath + bg);
            if (s) bgImage.sprite = s;
            else Debug.LogWarning($"Background sprite not found: {bg}");
        }

        if (charLeftImage != null)
        {
            if (!string.IsNullOrEmpty(charL))
            {
                Sprite p = Resources.Load<Sprite>(charactersPath + charL);
                charLeftImage.sprite = p;
                charLeftImage.enabled = (p != null);
                if (p == null) Debug.LogWarning($"Character sprite not found: {charL}");
            }
            else charLeftImage.enabled = false;
        }

        var sb = new System.Text.StringBuilder();
        if (isEvent) sb.AppendLine("<b>[EVENT]</b>\n");

        sb.AppendLine(body).AppendLine();

        if (opts != null)
        {
            for (int i = 0; i < opts.Count; i++)
            {
                DialogueOption o = opts[i];
                if (o == null) continue; // Skip null options
                string cursor = (i == optionIdx) ? "<color=#FFD700>> </color>" : "  "; // Non-breaking space for alignment
                sb.Append(cursor).Append(i + 1).Append(". ").Append(o.text).Append(" [")
                  .Append(ColorNumRange(o.profit)).Append("|")
                  .Append(ColorNumRange(o.relationships)).Append("|")
                  .Append(ColorNumRange(o.suspicion)).Append("]\n");
            }
        }


        bodyLabel.text = sb.ToString().TrimEnd('\n');   // avoid extra blank

        // Reset scroll position if ScrollRect is assigned
        if (bodyLabelScrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(bodyLabelScrollRect.content);
            bodyLabelScrollRect.verticalNormalizedPosition = 1f; // 1 is top, 0 is bottom
        }
    }

    string ColorNum(int n)
    {
        if (n == 0) return "<color=#CCCCCC>0</color>";
        string c = n > 0 ? "#4CAF50" : "#E53935"; // Green for positive, Red for negative
        return $"<color={c}>{(n > 0 ? "+" : "")}{n}</color>";
    }
    
    string ColorNumRange(int baseValue)
    {
        if (baseValue == 0) return "<color=#CCCCCC>0</color>";
        
        // Calculate variance (roughly 1/3 of base value)
        int variance = Mathf.Abs(baseValue) / 3;
        if (variance < 1) variance = 1;
        
        int min = baseValue - variance;
        int max = baseValue + variance;
        
        string c = baseValue > 0 ? "#4CAF50" : "#E53935";
        
        // Show as range if there's variance
        if (variance > 0)
        {
            string minStr = min > 0 ? "+" + min : min.ToString();
            string maxStr = max > 0 ? "+" + max : max.ToString();
            return $"<color={c}>{minStr}~{maxStr}</color>";
        }
        else
        {
            return $"<color={c}>{(baseValue > 0 ? "+" : "")}{baseValue}</color>";
        }
    }

    /* ────── Choice handler ────── */
    void OnOptionSelected(int pick)
    {
        if (currentOpts == null || pick < 0 || pick >= currentOpts.Count)
        {
            Debug.LogError($"OnOptionSelected: Invalid pick index {pick} or currentOpts is null/empty.");
            return;
        }

        AudioManager.Instance?.PlayUISelectSound(); // Play selection sound

        var opt = currentOpts[pick];
        if (opt == null)
        {
            Debug.LogError($"OnOptionSelected: Option at index {pick} is null.");
            return;
        }

        // Use the new RNG-based system
        GameManager.Instance?.ApplyChoiceWithRNG(opt);
        
        // Check for relationship-triggered events
        relationshipEvents?.CheckRelationshipEvents();

        if (showingCard)
        {
            showingCard = false;
            ShowNode();
            return;
        }

        // Attempt to trigger a side event *after* processing the main choice consequences,
        // but *before* moving to the next main node.
        if (TrySideEvent()) return;

        // Find node by ID
        int nextIndex = -1;
        Debug.Log($"Looking for node with ID: {opt.nextNode}");
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].id == opt.nextNode)
            {
                nextIndex = i;
                Debug.Log($"Found node {opt.nextNode} at index {i}");
                break;
            }
        }

        if (nextIndex >= 0)
        {
            nodeIdx = nextIndex;
            ShowNode();
        }
        else if (opt.nextNode == -1)
        {
            // -1 explicitly means end story
            Debug.Log("Ending game - nextNode is -1");
            SceneManager.LoadScene("EndScene");
        }
        else
        {
            // Node not found - this is an error
            Debug.LogError($"Could not find node with ID: {opt.nextNode}! Available node IDs: {string.Join(", ", nodes.Select(n => n.id))}");
            // For intro nodes, try to find node 0 as fallback
            if (opt.nextNode == 0)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].id == 0)
                    {
                        nodeIdx = i;
                        ShowNode();
                        return;
                    }
                }
            }
            // Last resort - go to first node
            nodeIdx = 0;
            ShowNode();
        }
    }

    /* ────── Deck logic ────── */
    SideEvent currentCard; // Store the current side event card being shown

    // Revised TrySideEvent logic
    bool TrySideEvent()
    {
        // Don't trigger side events during intro sequence (negative node IDs)
        if (nodes != null && nodeIdx >= 0 && nodeIdx < nodes.Count && nodes[nodeIdx].id < 0)
        {
            Debug.Log("Skipping side events during intro sequence");
            return false;
        }
        
        if (deck == null || deck.Count == 0) return false;
        if (rng.NextDouble() > 0.35) return false; // 35% chance to even attempt a side event

        List<SideEvent> eligibleEvents = new List<SideEvent>();
        for (int i = 0; i < deck.Count; i++) // Iterate using index
        {
            SideEvent currentEventInDeck = deck[i];
            if (currentEventInDeck == null) continue;

            // Check general eligibility first
            if (GameManager.Instance != null &&
                GameManager.Instance.Suspicion >= currentEventInDeck.minSuspicion &&
                GameManager.Instance.Relationships >= currentEventInDeck.minRelationship &&
                GameManager.Instance.Relationships <= currentEventInDeck.maxRelationship &&
                nodeIdx <= currentEventInDeck.maxScene) // nodeIdx refers to the current main story progression
            {
                // If it's a rare event, apply the 10% chance of it being considered
                if (currentEventInDeck.tag == "rare")
                {
                    if (rng.NextDouble() <= 0.1) // 10% chance to pass this check for rare events
                    {
                        eligibleEvents.Add(currentEventInDeck);
                    }
                }
                else // Not a rare event
                {
                    eligibleEvents.Add(currentEventInDeck);
                }
            }
        }

        if (eligibleEvents.Count > 0)
        {
            // Pick a random event from the eligible ones
            currentCard = eligibleEvents[rng.Next(eligibleEvents.Count)];
            deck.Remove(currentCard); // Remove from the original deck

            // Debug.Log($"Side Event Triggered: {currentCard.body}");
            ShowSideEvent(currentCard);
            return true;
        }

        return false;
    }
}
