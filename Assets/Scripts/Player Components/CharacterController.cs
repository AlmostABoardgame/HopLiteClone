using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlls the player character on the screen. The player can only move one space in the up/down/left/right direction, or use an attack.
/// After this 1 activation the players turn is over. 
/// </summary>
public class CharacterController : MonoBehaviour
{
    [HideInInspector] public PlayerCharacterComponent characterComponent;

    [Header("Settings")]
    [SerializeField] private int m_rangeAttackDistance;

    private bool m_rangeAttack = false;

    private void Awake()
    {
        characterComponent = GetComponent<PlayerCharacterComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        // NOTE: I could have used the Input.GetButtonDown / Input.GetAxis() for movement. But the downside is the GetAxis gets triggerd again after the attack and would need something like a bool so the player can't give any input anymore
        // The downsize of hard code keypresses like I have done do make the program less flexible but for the sake of keeping things simple and no settings menu i went for this.

        // Movement
        if (!Input.GetKey(KeyCode.Space))
        {
            // Up
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                if (characterComponent.neighbours.ContainsKey(Neighbour.above))
                {
                    if (characterComponent.neighbours[Neighbour.above].IsAvalible())
                    {
                        GridManager.PlaceOnTheGrid(characterComponent, characterComponent.neighbours[Neighbour.above].location);
                        SoundManager.s_insance.PlayerMovement();
                        GameManager.s_instance.MoveTheEnemies();
                    }
                }
            }

            // Down
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                if (characterComponent.neighbours.ContainsKey(Neighbour.below))
                {
                    if (characterComponent.neighbours[Neighbour.below].IsAvalible())
                    {
                        GridManager.PlaceOnTheGrid(characterComponent, characterComponent.neighbours[Neighbour.below].location);
                        SoundManager.s_insance.PlayerMovement();
                        GameManager.s_instance.MoveTheEnemies();
                    }
                }
            }

            // Left
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                if (characterComponent.neighbours.ContainsKey(Neighbour.left))
                {
                    if (characterComponent.neighbours[Neighbour.left].IsAvalible())
                    {
                        GridManager.PlaceOnTheGrid(characterComponent, characterComponent.neighbours[Neighbour.left].location);
                        SoundManager.s_insance.PlayerMovement();
                        GameManager.s_instance.MoveTheEnemies();
                    }
                }
            }

            // Right
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                if (characterComponent.neighbours.ContainsKey(Neighbour.right))
                {
                    if (characterComponent.neighbours[Neighbour.right].IsAvalible())
                    {
                        GridManager.PlaceOnTheGrid(characterComponent, characterComponent.neighbours[Neighbour.right].location);
                        SoundManager.s_insance.PlayerMovement();
                        GameManager.s_instance.MoveTheEnemies();
                    }
                }
            }
        }

        // Attack 1
        if (Input.GetKey(KeyCode.Space))
        {
            GridManager.HighlightAllNeighbors(characterComponent);
            m_rangeAttack = false;
        }
        // Range attacks
        // Up
        if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            GridManager.HighlightRange(characterComponent, Neighbour.above, m_rangeAttackDistance);
            m_rangeAttack = true;
        }
        // Down
        else if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            GridManager.HighlightRange(characterComponent, Neighbour.below, m_rangeAttackDistance);
            m_rangeAttack = true;
        }
        // Left
        else if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            GridManager.HighlightRange(characterComponent, Neighbour.left, m_rangeAttackDistance);
            m_rangeAttack = true;
        }
        // Right
        else if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            GridManager.HighlightRange(characterComponent, Neighbour.right, m_rangeAttackDistance);
            m_rangeAttack = true;
        }
        // Release hightlights and attack on all highlighed spaces
        if (Input.GetKeyUp(KeyCode.Space))
        {
            GridManager.AttackHightlighedArea();
            GameManager.s_instance.MoveTheEnemies();

            if (m_rangeAttack)
            {
                SoundManager.s_insance.PlayerRangeAttack();
            }
            else
            {
                SoundManager.s_insance.PlayerMeleeAttack();
            }
        }
    }
}