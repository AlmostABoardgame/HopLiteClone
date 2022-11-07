using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Base character class, it holds the base functions so when an enemy is activated the inherret version of that enemy will use its own movement or other functions
/// that are diffrent then the main class. This is also used by the player and level props for basic grid functionality, or in the case of the player also the hit/ destroy function.
/// </summary>
public class CharacterComponent : MonoBehaviour
{
    // Grid and functional
    public GridNode characterLocation;
    public Dictionary<Neighbour, GridNode> neighbours = new Dictionary<Neighbour, GridNode>();

    [Header("Connected Components")]
    public GameObject deathEffect;
    public GameObject activatedHighlight;

    [Header("Settings")]
    public Neighbour attackDirection;

    [Header("-- Only for enemy types --")]
    public Color enemyColor;
    [Space]
    public int scoreValue = 1;
    public int attackDistance = 4;
    public int followPlayerDistance = 1;
    public int movementSteps = 1;
    [Space]
    public AudioClip attackSoundFX;

    // in case the enemy is stuck we need a way to exit the loop, protected so its private but still accesable by the inheret scripts.
    protected int loopCounter = 0;

    private void Awake()
    {
        // set the alpha for the enemy highlights
        enemyColor.a = 0.06f;
    }

    /// <summary>
    /// Copy the neighbors from the node its standing on so to look for the player
    /// </summary>
    public void GetNewNeighbors()
    {
        neighbours = GridManager.GetNeighbors(characterLocation.location);
    }

    /// <summary>
    /// This character (enemy) looks if they can hit the player with there attack, if they can they attack.
    /// If not they will contineu to there moment action.
    /// This is a async task so it needs to be completed first before continuing to the next.
    /// </summary>
    public virtual async Task<bool> ActivateEnemy()
    {
        activatedHighlight.SetActive(true);

        // Look for the player and attack in 4 direction with a max distance of 4 (follor player distance)
        bool foundPlayer = false;

        foreach (var neighbore in neighbours)
        {
            if ((neighbore.Key & attackDirection) != 0)
            {
                if (CheckForPlayer(neighbore.Key))
                {
                    foundPlayer = true;                   
                }
            }
        }

        await Task.Delay(1);
        return foundPlayer;
    }

    public virtual async Task Move()
    {
        // Movement sequence is set in the enemy type inherret script
        await Task.Yield();
    }

    /// <summary>
    /// Check if the player is near this object in the posible movement directions of this character
    /// </summary>
    private bool CheckForPlayer(Neighbour direction)
    {
        GridNode currentNode = neighbours[direction];

        for (int i = 0; i < attackDistance; i++)
        {
            if (currentNode.neighbours.ContainsKey(direction))
            {
                if (currentNode.currentCharacter != null)
                {
                    if (currentNode.currentCharacter.tag == "Player")
                    {
                        Attack(direction);
                        return true;
                    }
                    else if (currentNode.currentCharacter.tag == "Terrain")
                    {
                        return false;
                    }
                }
                if (currentNode.neighbours.ContainsKey(direction))
                {
                    currentNode = currentNode.neighbours[direction];
                }
            }           
        }

        return false;
    }

    /// <summary>
    /// Alle enemy types show there next attack on the grid.
    /// </summary>
    public void HighlightNextMove()
    {
        characterLocation.HighLightNode(true, enemyColor);

        foreach (var direction in neighbours.Keys)
        {
            if ((direction & attackDirection) != 0)
            {
                GridNode currentNode = neighbours[direction];

                for (int i = 0; i < attackDistance; i++)
                {
                    // Exit forloop when bumping into terrain
                    if (currentNode.currentCharacter != null)
                    {
                        if (currentNode.currentCharacter.tag == "Terrain")
                        {
                            break;
                        }
                    }

                    currentNode.HighLightNode(true, enemyColor);

                    if (currentNode.neighbours.ContainsKey(direction))
                    {
                        currentNode = currentNode.neighbours[direction];
                    }
                }
            }
        }
    }

    /// <summary>
    /// Attacks with a range of one to one direction
    /// </summary>
    /// <param name="direction"></param>
    public virtual void Attack(Neighbour direction)
    {
        SoundManager.s_insance.EnemyAttack(attackSoundFX);

        GridManager.HighlightRange(this, direction, attackDistance);
        GridManager.AttackHightlighedArea();
    }

    /// <summary>
    /// When this object gets destroyed show a death effect.
    /// </summary>
    public virtual void DestroyObject()
    {
        SoundManager.s_insance.EnemyGetHit();
        GameManager.s_instance.EnemyDestroyed(scoreValue);

        GameObject deathFX = Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(deathFX, 5);

        gameObject.SetActive(false);
    }
}