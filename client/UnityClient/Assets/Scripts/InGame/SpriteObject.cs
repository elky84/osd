using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class SpriteObject : MonoBehaviour
{
    protected SpriteRenderer SpriteRenderer { get; set; }

    [HideInInspector]
    public bool FlipX
    {
        get
        {
            return SpriteRenderer.flipX;
        }
        set
        {
            SpriteRenderer.flipX = value;
        }
    }

    // The name of the sprite sheet to use
    public string SpriteSheetPath;

    // The name of the currently loaded sprite sheet
    private string LoadedSpriteSheetPath;

    // The dictionary containing all the sliced up sprites in the sprite sheet
    private Dictionary<string, Sprite> SpriteSheet;

    protected Animator Animator { get; set; }

    protected void OnAwake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Animator = GetComponent<Animator>();
    }

    protected void OnStart()
    {
        this.LoadSpriteSheet();
    }

    // Runs after the animation has done its work
    private void LateUpdate()
    {
        // Check if the sprite sheet name has changed (possibly manually in the inspector)
        if (this.LoadedSpriteSheetPath != this.SpriteSheetPath)
        {
            // Load the new sprite sheet
            this.LoadSpriteSheet();
        }

        if (this.SpriteRenderer == null)
            return;

        // Swap out the sprite to be rendered by its name
        // Important: The name of the sprite must be the same!
        this.SpriteRenderer.sprite = this.SpriteSheet[this.SpriteRenderer.sprite.name];
    }

    // Loads the sprites from a sprite sheet
    private void LoadSpriteSheet()
    {
        // Load the sprites from a sprite sheet file (png). 
        // Note: The file specified must exist in a folder named Resources
        var sprites = Resources.LoadAll<Sprite>("Sprites/" + this.SpriteSheetPath);
        this.SpriteSheet = sprites.ToDictionary(x => x.name, x => x);

        // Remember the name of the sprite sheet in case it is changed later
        this.LoadedSpriteSheetPath = this.SpriteSheetPath;
    }
}
