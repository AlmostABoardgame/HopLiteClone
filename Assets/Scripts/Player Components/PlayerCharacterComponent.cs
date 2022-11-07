using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// Character component from the player.
/// Many of the base functionality will not be used. I could have created a seperate interret class that does not hold that many of the
/// Enemy type based functionality, but to keep things a bit smipler i went for this.
/// </summary>
public class PlayerCharacterComponent : CharacterComponent
{
    // The image on top of the character to show the currently sellected character image on the player.
    [SerializeField] private Renderer m_renderer;

    /// <summary>
    /// This does NOT destroy the player but registers as a hit, since the player has health instead of getting killed instantly.
    /// Renaming it ObjectGetHit or whatever would maybe make more sence but then i would need an extra destroy object function if its an enemy. So i just went for one function.
    /// </summary>
    public override void DestroyObject()
    {
        GameObject deathFX = Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(deathFX, 5);

        SoundManager.s_insance.PlayerGetHit();
        GameManager.s_instance.PlayerGetsHit();
    }

    /// <summary>
    /// Remove the current player from the game whent he player is destroyed. Might be a bit cofusing in the function naming with the one above
    /// as stated there a rename would have been smart but also would create an extra function in the enemy types.
    /// </summary>
    public void playerDies()
    {
        GameObject deathFX = Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(deathFX, 5);

        Destroy(gameObject);
    }

    // This does nothing, but it needs to be chanceld since we do not want the player component to carry out the base function.
    public override async Task<bool> ActivateEnemy()
    {
        await Task.Delay(1);
        return true;
    }

    /// <summary>
    /// Show the current sellected character image on the player object.
    /// </summary>
    public void SetupPlayerIcon(Sprite playerIcon)
    {
        m_renderer.material.SetTexture("_MainTex", textureFromSprite(playerIcon));
    }

    /// <summary>
    /// Create a texture image from the sprit.
    /// </summary>
    private Texture2D textureFromSprite(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            Texture2D newTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                         (int)sprite.textureRect.y,
                                                         (int)sprite.textureRect.width,
                                                         (int)sprite.textureRect.height);
            newTexture.SetPixels(newColors);
            newTexture.Apply();
            return newTexture;
        }
        else
            return sprite.texture;
    }
}
