using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This script helps set up the EndScene UI in Unity Editor
public class EndSceneUISetup : MonoBehaviour
{
    [Header("Instructions")]
    [TextArea(10, 20)]
    public string setupInstructions = @"
END SCENE UI SETUP GUIDE:

1. CREATE UI STRUCTURE:
   - Create Canvas (if not exists)
   - Set Canvas Scaler to 'Scale With Screen Size' (1920x1080)
   
2. ADD BACKGROUND:
   - Add Image as child of Canvas, name it 'Background'
   - Anchor: Stretch to fill entire canvas
   - Color: Dark (20, 20, 20) or add background sprite
   
3. CREATE MAIN CONTENT PANEL:
   - Add Empty GameObject, name it 'ContentPanel'
   - Position: Center, Width: 1600, Height: 900
   
4. ADD UI ELEMENTS TO CONTENT PANEL:
   
   a) Ending Title (top):
      - Add TextMeshPro, name it 'EndingTitle'
      - Font Size: 72, Bold, Center aligned
      - Position: (0, 300)
      
   b) Ending Description (below title):
      - Add TextMeshPro, name it 'EndingDescription'
      - Font Size: 36, Center aligned
      - Position: (0, 200)
      
   c) Character Image (left side):
      - Add Image, name it 'CharacterImage'
      - Size: 400x600
      - Position: (-500, 0)
      
   d) Epilogue Text (center):
      - Add TextMeshPro, name it 'EpilogueText'
      - Font Size: 24, Width: 800
      - Position: (100, 0)
      
   e) Stats Text (right side):
      - Add TextMeshPro, name it 'StatsText'
      - Font Size: 28, Left aligned
      - Position: (600, 0)
      
5. ADD BUTTONS PANEL:
   - Add Horizontal Layout Group, name it 'ButtonPanel'
   - Position: (0, -350)
   - Spacing: 50
   
   a) Restart Button:
      - Add Button-TextMeshPro, name it 'RestartButton'
      - Text: 'New Game'
      - Size: 200x60
      
   b) Quit Button:
      - Add Button-TextMeshPro, name it 'QuitButton'
      - Text: 'Quit'
      - Size: 200x60
      
6. ATTACH ENDSCENECONTROLLER:
   - Add EndSceneController to Canvas or a GameObject
   - Drag all UI elements to their respective slots
   - Ensure background/character paths are correct
   
7. OPTIONAL ENHANCEMENTS:
   - Add fade-in animation
   - Add particle effects for certain endings
   - Add background music continuation
";

    void Start()
    {
        Debug.Log("EndSceneUISetup loaded. Check Inspector for setup instructions.");
    }
}