using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// This enemy walks and shoots for one space in the up, down, left and right direction when activated.
/// This enemy Type walks in blocks until they walk past the player then they will start to attack and follow him.
/// </summary>
public class EnemyTypeA : CharacterComponent
{
    public int walkingDistance = 3;

    // Internaly used values
    private bool m_hasMoved = false;
    private int  m_currentWalkingDistance = 0;
    private int  m_currentWalkingPosition = 0;

    /// <summary>
    /// This enemy can walk 4 directions after seeing if its able to reach the player using the pathfinding
    /// if not it will walk in a block with the lengt of "walkingDistance" using its posible movement directions.
    /// If a direction is blocked or not posible it will move on to its next direction. In the case the enmey is stuck
    /// a loop protection keeps it from going on forever. Meaning it will just stand still.
    ///
    /// Its a Async Task so it will need to be completed first before moving on to the next enemy.
    /// </summary>
    public override async Task Move()
    {
        // Try to find a path to the player if it is close, if posible move into the dirction of the player
        List<GridNode> pathToPlayer = await GridManager.s_instance.PathFinding(characterLocation, GameManager.s_instance.thePlayer.characterComponent.characterLocation, attackDirection);
        if (pathToPlayer != null)
        {
            if (pathToPlayer.Count <= followPlayerDistance && pathToPlayer[movementSteps].IsAvalible())
            {
                GridManager.PlaceOnTheGrid(this, pathToPlayer[movementSteps].location);
                SoundManager.s_insance.EnemyMovement();
                await Task.Yield(); // complete the task
                return; // to prevent this function from running anything else.
            }
        }
        else
        {
            Debug.LogWarning(name + "'s path empty");
        }

        m_hasMoved = false;

        loopCounter = 0;
        while (!m_hasMoved)
        {
            loopCounter++;
            if (loopCounter > 25) m_hasMoved = true;

            // One up
            if (m_currentWalkingPosition == 0)
            {
                if (neighbours.ContainsKey(Neighbour.above))
                {
                    if (neighbours[Neighbour.above].IsAvalible())
                    {
                        GridManager.PlaceOnTheGrid(this, neighbours[Neighbour.above].location);
                        m_hasMoved = true;
                    }
                }
            }
            // One right
            else if (m_currentWalkingPosition == 1)
            {
                if (neighbours.ContainsKey(Neighbour.right))
                {
                    if (neighbours[Neighbour.right].IsAvalible())
                    {
                        GridManager.PlaceOnTheGrid(this, neighbours[Neighbour.right].location);
                        m_hasMoved = true;
                    }
                }
            }
            // One down
            else if (m_currentWalkingPosition == 2)
            {
                if (neighbours.ContainsKey(Neighbour.below))
                {
                    if (neighbours[Neighbour.below].IsAvalible())
                    {
                        GridManager.PlaceOnTheGrid(this, neighbours[Neighbour.below].location);
                        m_hasMoved = true;
                    }
                }
            }
            // One left
            else if (m_currentWalkingPosition == 3)
            {
                if (neighbours.ContainsKey(Neighbour.left))
                {
                    if (neighbours[Neighbour.left].IsAvalible())
                    {
                        GridManager.PlaceOnTheGrid(this, neighbours[Neighbour.left].location);
                        m_hasMoved = true;
                    }
                }
            }

            // If the side is not avalible, this can be becouse of other enemies, edge of the level or blocking terrain ajust the movement.
            if (!m_hasMoved)
            {
                m_currentWalkingPosition++;
                if (m_currentWalkingPosition > 3) m_currentWalkingPosition = 0;
            }
        }

        SoundManager.s_insance.EnemyMovement();

        // Increase the distance check, if over the distance move to the next direction.
        m_currentWalkingDistance++;
        if (m_currentWalkingDistance >= walkingDistance)
        {
            m_currentWalkingDistance = 0;
            m_currentWalkingPosition++;
            if (m_currentWalkingPosition > 3) m_currentWalkingPosition = 0;
        }

        // complete the taks
        await Task.Yield();
    }
}