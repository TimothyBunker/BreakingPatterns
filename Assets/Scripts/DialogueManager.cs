using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    /* ────── Inspector ────── */
    [Header("UI")]
    public TMP_Text bodyLabel;
    public Image bgImage;
    public Image charLeftImage;
    public Image charRightImage;     // Walt remains here

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

    /* ────── Public API called by loader ────── */
    public void InitDecks(List<DialogueNode> main, List<SideEvent> side)
    {
        nodes = main;
        deck = side ?? new List<SideEvent>();
        nodeIdx = optionIdx = 0;
        LoadWaltPortrait();
        ShowNode();
    }

    /* ────── Update ────── */
    void Update()
    {
        if (nodes == null) return;

        if (Input.GetKeyDown(KeyCode.UpArrow)) optionIdx = (optionIdx - 1 + currentOpts.Count) % currentOpts.Count;
        if (Input.GetKeyDown(KeyCode.DownArrow)) optionIdx = (optionIdx + 1) % currentOpts.Count;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Alpha1 + optionIdx))
            OnOptionSelected(optionIdx);

        // Re-draw only when arrow keys pressed (keeps perf fine)
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            Redraw();
    }

    /* ────── Drawing helpers ────── */
    void LoadWaltPortrait()
    {
        Sprite w = Resources.Load<Sprite>(charactersPath + waltSpriteName);
        if (charRightImage) { charRightImage.sprite = w; charRightImage.enabled = w; }
    }

    void ShowNode()
    {
        showingCard = false;
        optionIdx = 0;
        var n = nodes[nodeIdx];
        DrawScreen(n.body, n.background, n.charLeft, n.options, false);
    }

    void ShowSideEvent(SideEvent ev)
    {
        showingCard = true;
        optionIdx = 0;
        DrawScreen(ev.body, ev.background, ev.charLeft, ev.options, true);
    }

    void Redraw()
    {
        if (showingCard)
            DrawScreen(currentCard.body, currentCard.background, currentCard.charLeft, currentOpts, true);
        else
        {
            var n = nodes[nodeIdx];
            DrawScreen(n.body, n.background, n.charLeft, currentOpts, false);
        }
    }

    void DrawScreen(string body, string bg, string charL, List<DialogueOption> opts, bool isEvent)
    {
        currentOpts = opts;

        if (bgImage && !string.IsNullOrEmpty(bg))
        {
            Sprite s = Resources.Load<Sprite>(backgroundsPath + bg);
            if (s) bgImage.sprite = s;
        }

        if (charLeftImage)
        {
            if (!string.IsNullOrEmpty(charL))
            {
                Sprite p = Resources.Load<Sprite>(charactersPath + charL);
                charLeftImage.sprite = p;
                charLeftImage.enabled = (p != null);
            }
            else charLeftImage.enabled = false;
        }

        var sb = new System.Text.StringBuilder();
        if (isEvent) sb.AppendLine("<b>[EVENT]</b>\n");

        sb.AppendLine(body).AppendLine();
        for (int i = 0; i < opts.Count; i++)
        {
            DialogueOption o = opts[i];
            string cursor = (i == optionIdx) ? "<color=#FFD700>> </color>" : "  ";
            sb.Append(cursor).Append(i + 1).Append(". ").Append(o.text).Append(" [")
              .Append(ColorNum(o.profit)).Append("|")
              .Append(ColorNum(o.relationships)).Append("|")
              .Append(ColorNum(o.suspicion)).Append("]\n");
        }

        bodyLabel.text = sb.ToString().TrimEnd('\n');   // avoid extra blank
    }

    string ColorNum(int n)
    {
        if (n == 0) return "<color=#CCCCCC>0</color>";
        string c = n > 0 ? "#4CAF50" : "#E53935";
        return $"<color={c}>{(n > 0 ? "+" : "")}{n}</color>";
    }

    /* ────── Choice handler ────── */
    void OnOptionSelected(int pick)
    {
        var opt = currentOpts[pick];
        GameManager.Instance.ApplyChoice(opt.profit, opt.relationships, opt.suspicion);

        if (showingCard) { showingCard = false; ShowNode(); return; }
        if (TrySideEvent()) return;

        if (opt.nextNode >= 0 && opt.nextNode < nodes.Count)
        {
            nodeIdx = opt.nextNode;
            ShowNode();
        }
        else SceneManager.LoadScene("EndScene");
    }

    /* ────── Deck logic ────── */
    SideEvent currentCard;
    bool TrySideEvent()
    {
        if (deck == null || deck.Count == 0) return false;
        if (rng.NextDouble() > 0.35) return false;      // 35 % chance

        int start = rng.Next(deck.Count);
        for (int i = 0; i < deck.Count; i++)
        {
            SideEvent ev = deck[(start + i) % deck.Count];

            if (ev.tag == "rare" && rng.NextDouble() > 0.1) continue;   // 10 % for rare
            if (GameManager.Instance.Suspicion >= ev.minSuspicion && nodeIdx <= ev.maxScene)
            {
                deck.Remove(ev);
                currentCard = ev;
                ShowSideEvent(ev);
                return true;
            }
        }
        return false;
    }
}
