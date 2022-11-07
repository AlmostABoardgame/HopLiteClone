using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// This enemy walks and shoots for one space diagonaly when activated.
/// This enemy Type walks in diagonal blocks until they walk past the player then they will start to attack and follow him.
///
/// NOTE: the diagonaly enemy can only get to the player with pathfinding if the player is on any of the nodes this enemy can reach
/// meaning it will lose the player 50% of the players movements. I could fix this by having the full pathfinding and using the path and sellect
/// the tile that is closes to the player this enemy could walk on. But i wanted to keep it clean and simple. So it now just cant always get to the player
/// </summary>
public class EnemyTypeB : CharacterComponent
{
    public int walkingDistance = 3;

    // Internaly used values
    private bool m_hasMoved = false;
    private int m_currentWalkingDistance = 0;  
    private int m_currentWalkingPosition = 0;


    /// <summary>
    /// This enemy can walk 4 directions after seeing if its able to reach the player using the pathfinding, but since this object can't cover all the nodes due to its movement
    /// it will not always folow the player and then just continue its movement patern.
    /// It will walk in a block with the lengt of "walkingDistance" using its posible movement directions.
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
                if (neighbours.ContainsKey(Neighbour.aboveLeft))
                {
                    if (neighbours[Neighbour.aboveLeft].IsAvalible())
                    {
                        GridManager.PlaceOnTheGrid(this, neighbours[Neighbour.aboveLeft].location);
                        m_hasMoved = true;
                    }
                }
            }
            // One right
            else if (m_currentWalkingPosition == 1)
            {
                if (neighbours.ContainsKey(Neighbour.belowLeft))
                {
                    if (neighbours[Neighbour.belowLeft].IsAvalible())
                    {
                        GridManager.PlaceOnTheGrid(this, neighbours[Neighbour.belowLeft].location);
                        m_hasMoved = true;
                    }
                }
            }
            // One down
            else if (m_currentWalkingPosition == 2)
            {
                if (neighbours.ContainsKey(Neighbour.belowRight))
                {
                    if (neighbours[Neighbour.belowRight].IsAvalible())
                    {
                        GridManager.PlaceOnTheGrid(this, neighbours[Neighbour.belowRight].location);
                        m_hasMoved = true;
                    }
                }
            }
            // One left
            else if (m_currentWalkingPosition == 3)
            {
                if (neighbours.ContainsKey(Neighbour.aboveRight))
                {
                    if (neighbours[Neighbour.aboveRight].IsAvalible())
                    { 
                        GridManager.PlaceOnTheGrid(this, neighbours[Neighbour.aboveRight].location);
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