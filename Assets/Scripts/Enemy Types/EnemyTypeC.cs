using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// This enemy type is difrent since it jumps intead of moving.
/// The enmey will move randomly over the grid until the player gets to close, then it will start folowing the player.
/// Normaly it will jump in 8 directions but when it sees the enemy it can also jump ontop of the enemy as a special attack ignoring its direction but still have it be at the correct movement range.
/// </summary>
public class EnemyTypeC : CharacterComponent
{
    public bool canJumpOnThePlayer = false;

    // Internaly used values
    private bool m_hasMoved = false;
    private int m_currentWalkingPosition = 0;


    /// <summary>
    /// The movement sequence as explaned above, its an async task and will need to be completed before the next enemey (if any) can be activated.
    /// It first looks if its posible to folor or jump ontop the player. If not it picks a random location in its range and movement direcction.
    /// </summary>
    public override async Task Move()
    {
        m_hasMoved = false;

        List<GridNode> jumpPathToPlayer = await GridManager.s_instance.PathFinding(characterLocation, GameManager.s_instance.thePlayer.characterComponent.characterLocation, attackDirection);

        // Special attack movement, where it can jump on the player if the player is in range but not in a direct straight or diagonal line.
        if (canJumpOnThePlayer)
        {
            // with this movement the enemy can jump on your if its just outside of its direction
            // Try to find a path to the player if it is close, if posible move into the dirction of the player
            
            if (jumpPathToPlayer != null)
            {
                if (jumpPathToPlayer.Count <= followPlayerDistance && jumpPathToPlayer[movementSteps].IsAvalible())
                {
                    GridManager.PlaceOnTheGrid(this, jumpPathToPlayer[movementSteps].location);
                    await Task.Yield(); // complete the task
                    m_hasMoved = true;
                }
            }
        }

        // This enmey uses 2 directions to move in, but should not jump to places in betuween especial when the settings are set to a higher max jump distance.
        // if it could check all the move direcitons it could make an illiegal move. We just use the 2 movement options seperatly so it can only jump in straight lines.

        // straigh movement
        List<GridNode> pathToPlayer = await GridManager.s_instance.PathFinding(characterLocation, GameManager.s_instance.thePlayer.characterComponent.characterLocation, Neighbour.above | Neighbour.left | Neighbour.right | Neighbour.below);
        if (pathToPlayer != null && !m_hasMoved)
        {
            if (pathToPlayer.Count <= followPlayerDistance && pathToPlayer.Count > 1 && pathToPlayer.Count < movementSteps && pathToPlayer[movementSteps].IsAvalible())
            {
                GridManager.PlaceOnTheGrid(this, pathToPlayer[movementSteps].location);
                await Task.Yield(); // complete the task
                m_hasMoved = true;
            }
        }

        // diagonaly movement
        pathToPlayer = await GridManager.s_instance.PathFinding(characterLocation, GameManager.s_instance.thePlayer.characterComponent.characterLocation, Neighbour.aboveLeft | Neighbour.aboveRight | Neighbour.belowLeft | Neighbour.belowRight);
        if (pathToPlayer != null && !m_hasMoved)
        {
            if (pathToPlayer.Count <= followPlayerDistance && pathToPlayer.Count > 1 && pathToPlayer.Count < movementSteps && pathToPlayer[movementSteps].IsAvalible())
            {
                GridManager.PlaceOnTheGrid(this, pathToPlayer[movementSteps].location);
                await Task.Yield(); // complete the task
                m_hasMoved = true;
            }
        }

       
        loopCounter = 0;
        while (!m_hasMoved)
        {
            loopCounter++;
            if (loopCounter > 25) m_hasMoved = true;

            m_currentWalkingPosition = Random.Range(0, 7);

            switch (m_currentWalkingPosition)
            {
                case 0:
                    m_hasMoved = Jump(Neighbour.above);
                    break;
                case 1:
                    m_hasMoved = Jump(Neighbour.aboveLeft);
                    break;
                case 2:
                    m_hasMoved = Jump(Neighbour.aboveRight);
                    break;
                case 3:
                    m_hasMoved = Jump(Neighbour.below);
                    break;
                case 4:
                    m_hasMoved = Jump(Neighbour.belowLeft);
                    break;
                case 5:
                    m_hasMoved = Jump(Neighbour.belowRight);
                    break;
                case 6:
                    m_hasMoved = Jump(Neighbour.right);
                    break;
                case 7:
                    m_hasMoved = Jump(Neighbour.left);
                    break;
            }
        }

        SoundManager.s_insance.EnemyMovement();

        // complete the taks
        await Task.Yield();
    }

    /// <summary>
    /// A movement style where the character jumps 2 tiles away in the given direction.
    /// </summary>
    private bool Jump(Neighbour direction)
    {
        SoundManager.s_insance.EnemyJump();

        if (neighbours.ContainsKey(direction))
        {
            GridNode currentNode = neighbours[direction];

            for (int i = 1; i <= movementSteps; i++)
            {
                if (currentNode.neighbours.ContainsKey(direction))
                {
                    currentNode = currentNode.neighbours[direction];
                }
            }

            if (currentNode.IsAvalible())
            {
                // Make the move
                GridManager.PlaceOnTheGrid(this, currentNode.location);
                return true;
            }
        }

        return false;
    }
}