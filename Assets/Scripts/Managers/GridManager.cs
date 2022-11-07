using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

/// <summary>
/// Building and managing the grid.
/// The grid request for the test was 10x10, In the scene there is a fixed grid outside border. I could instance this border dynamicly regardless of the size of the grid.
/// But to keep things more simple i just added the whole border in the scene.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager s_instance;

    [SerializeField] private GridNode basicGroundPrefab;
    [SerializeField] private Transform groundHolder;

    private Color m_playerAttackHightlightColor = Color.red;

    [Header("Level Settings")]
    // since its the test needs to be 10x10 this setting should not be anything but that since the fixed border i did to keep things a bit more easy.
    public Vector2 levelGridSize;

    // All the grid nodes orderd by there location.
    public static Dictionary<Vector2, GridNode> groundObjects = new Dictionary<Vector2, GridNode>();

    /// <summary>
    /// Currently highlighed ground objects that indicate the current to be attacked node's or can be turned off when needed in case the attack changes.
    /// </summary>
    public static List<GridNode> currentlyHightlighted = new List<GridNode>();

    // Path finding
    private List<GridNode> openList = new List<GridNode>();
    private List<GridNode> closedList = new List<GridNode>();
    private const int moveStraightCost = 11;
    private const int moveDiagonalCost = 14;

    private void Start()
    {
        s_instance = this;
        m_playerAttackHightlightColor.a = 0.5f;
        SetupTheGrid();
    }

    /// <summary>
    /// Setup and instance the grid using a nested forloop that goes over the x/y axis creating basicly a rows of nodes
    /// </summary>
    private void SetupTheGrid()
    {
        for (int x = 0; x < levelGridSize.x; x++)
        {
            for (int y = 0; y < levelGridSize.y; y++)
            {
                GridNode newNode = Instantiate(basicGroundPrefab, new Vector3(x, 0, y), Quaternion.identity);
                newNode.transform.SetParent(groundHolder);
                newNode.location = new Vector2(x, y);

                groundObjects.Add(newNode.location, newNode);
            }
        }

        SetupStraightNeighbors();
        SetupDiagonalNeighbors();
    }

    /// <summary>
    /// Look for all the neighbors in a up/down/left/right position on the grid using a nested forloop just as when building the grid itself.
    /// </summary>
   private void SetupStraightNeighbors()
    {
        for (int x = 0; x < levelGridSize.x; x++)
        {
            for (int y = 0; y < levelGridSize.y; y++)
            {
                // horizontal until 1 away from the side
                if (x < levelGridSize.x - 1)
                {
                    groundObjects[new Vector2(x, y)].AddNeighbore(Neighbour.right, groundObjects[new Vector2(x + 1, y)]);
                }
                // when on the left side
                if (x > 0)
                {
                    groundObjects[new Vector2(x, y)].AddNeighbore(Neighbour.left, groundObjects[new Vector2(x - 1, y)]);
                }

                // vertical untile 1 away from the bottom
                if (y < levelGridSize.y - 1)
                {
                    groundObjects[new Vector2(x, y)].AddNeighbore(Neighbour.above, groundObjects[new Vector2(x, y + 1)]);
                }
                if (y > 0)
                {
                    groundObjects[new Vector2(x, y)].AddNeighbore(Neighbour.below, groundObjects[new Vector2(x, y - 1)]);
                }
            }
        }
    }

    /// <summary>
    /// This looks for the Diagonaly niehgbors, this could have been mixed with the function above to find all neighbors.
    /// But the function gets long and visualy messy, think its more clean to slipt them up.
    /// </summary>
    void SetupDiagonalNeighbors()
    {
        for (int x = 0; x < levelGridSize.x; x++)
        {
            for (int y = 0; y < levelGridSize.y; y++)
            {
                // Above right
                if (x < levelGridSize.x - 1 && y < levelGridSize.y - 1)
                {
                    groundObjects[new Vector2(x, y)].AddNeighbore(Neighbour.aboveRight, groundObjects[new Vector2(x + 1, y + 1)]);
                }
                // Above left
                if (x > 0 && y < levelGridSize.y - 1)
                {
                    groundObjects[new Vector2(x, y)].AddNeighbore(Neighbour.aboveLeft, groundObjects[new Vector2(x - 1, y + 1)]);
                }

                // Below right
                if (x < levelGridSize.x - 1 && y > 0)
                {
                    groundObjects[new Vector2(x, y)].AddNeighbore(Neighbour.belowRight, groundObjects[new Vector2(x + 1, y - 1)]);
                }
                // Below left
                if (x > 0 && y > 0)
                {
                    groundObjects[new Vector2(x, y)].AddNeighbore(Neighbour.belowLeft, groundObjects[new Vector2(x - 1, y - 1)]);
                }

            }
        }
    }

    /// <summary>
    /// Place an object at a possible location on the grid if its posible to be placed there.
    /// </summary>
    public static void PlaceOnTheGrid(CharacterComponent character, Vector2 location)
    {
        if (character.characterLocation != null)
        {
            character.characterLocation.currentCharacter = null;
        }

        // The jumping enemy also tries to jump on the player, if this happens they both get a hit
        if (groundObjects[location].currentCharacter != null)
        {
            groundObjects[location].currentCharacter.DestroyObject();
            character.DestroyObject();
        }

        groundObjects[location].currentCharacter = character;
        character.transform.position = new Vector3(location.x, 1.75f, location.y);
        character.characterLocation = groundObjects[location];
        character.GetNewNeighbors();
    }

    /// <summary>
    /// Range attack in a straight line, this shows upto the range all the tile's that can be part of the attack
    /// </summary>
    /// <param name="character">the attack character</param>
    /// <param name="direction">any 6 directions from the character location</param>
    /// <param name="attackRange"></param>
    public static void HighlightRange(CharacterComponent character, Neighbour direction, int attackRange)
    {
        // First turn off all active hightlights
        TurnOffAllHightlights();

        // Pick the needed neighbore from tile the character is on and keep picking the neightbore of the neighbore in that same direction
        if (character.neighbours.ContainsKey(direction))
        {
            GridNode currentNode = character.neighbours[direction];
            currentlyHightlighted.Clear();

            for (int i = 0; i < attackRange; i++)
            {
                // Stop if there is terrain
                if (currentNode.currentCharacter != null)
                {
                    if (currentNode.currentCharacter.tag == "Terrain")
                    {
                        return;
                    }
                }

                currentlyHightlighted.Add(currentNode);
                currentNode.HighLightNode(true, s_instance.m_playerAttackHightlightColor);
                if (currentNode.neighbours.ContainsKey(direction))
                {
                    currentNode = currentNode.neighbours[direction];
                }
            }
        }
    }

    /// <summary>
    /// Attacks the current highlighed area.
    /// </summary>
    public static void AttackHightlighedArea()
    {
        foreach (var highlightedArea in currentlyHightlighted)
        {
            if (highlightedArea.currentCharacter != null)
            {
                if (highlightedArea.currentCharacter.tag != "Terrain")
                {
                    highlightedArea.currentCharacter.DestroyObject();
                    highlightedArea.currentCharacter = null;
                }
            }
            highlightedArea.AttackVisual();
        }

        TurnOffAllHightlights();
    }

    /// <summary>
    /// Highlights all neighbors of the character and stores them in a list in case it will the highlighted tiles will be part of the upcomming attack.
    /// </summary>
    /// <param name="character"></param>
    public static void HighlightAllNeighbors(CharacterComponent character)
    {
        currentlyHightlighted.Clear();
        TurnOffAllHightlights();

        foreach (var neighbore in character.neighbours)
        {
            neighbore.Value.HighLightNode(true, s_instance.m_playerAttackHightlightColor);
            currentlyHightlighted.Add(neighbore.Value);

            // For the way this foreach loop is setup we first set all neirborse before turning off the ones with terrain on them.
            if (neighbore.Value.currentCharacter != null)
            {
                if (neighbore.Value.currentCharacter.tag == "Terrain")
                {
                    neighbore.Value.HighLightNode(false, s_instance.m_playerAttackHightlightColor);
                    currentlyHightlighted.Remove(neighbore.Value);
                }
            }
        }
    }

    /// <summary>
    /// Return all neighbors for the node on this location.
    /// </summary>
    public static Dictionary<Neighbour, GridNode> GetNeighbors(Vector2 location)
    {
        return groundObjects[location].neighbours;
    }

    /// <summary>
    /// Attacks and future attacks show a hightlight on the grid nodes, turn off all those hightlighs.
    /// </summary>
    public static void TurnOffAllHightlights()
    {
        foreach (var groundObject in groundObjects)
        {
            groundObject.Value.HighLightNode(false, Color.white);
        }
    }

    /// <summary>
    /// Look for a random avalible node on the grid so an item or enemy can be freely places there.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetRandomEmptyLocation()
    {
        Vector2 location = Vector2.zero;
        bool whileLoop = true;

        while (whileLoop)
        {
            location = new Vector2((int)Random.Range(0, levelGridSize.x), (int)Random.Range(0, levelGridSize.y));
            if (groundObjects.ContainsKey(location))
            {
                if (groundObjects[location].IsAvalible() && groundObjects[location].currentCharacter == null)
                {
                    whileLoop = false;
                }
            }
        }

        return location;
    }

    /// <summary>
    /// When restarting or starting a new level turn all the hightlighs off and clean the grid from all its removed character connections.
    /// </summary>
    public static void ResetTheGrid()
    {
        TurnOffAllHightlights();

        foreach (var item in groundObjects)
        {
            // Except for the player (if still alive)
            if (item.Value.currentCharacter != null)
            {
                if (item.Value.currentCharacter.tag != "Player")
                {
                    item.Value.currentCharacter = null;
                }
            }
           
            item.Value.accesableTerrain = true;
        }
    }

    /// <summary>
    /// PATH FINDING system, loop trough the grid and fint the shortes route to the end posstion using only the supplied directions
    /// This creates a path that only uses the direction nodes the enemy or player can use so it matches that enemies movement posability.
    /// 
    /// This is a async task so the enemy movement system has to wait for it te return before going on to the next enemey to prevent this function from
    /// rutting more then ones at a time. What would not be posible since it changes values in the nodes that will then be over writen by a second run of this system.
    /// 
    /// NOTE: tutorial used: https://youtu.be/alU04hvz6L4
    /// </summary>
    /// <param name="startNode">Start point (enemy itself)</param>
    /// <param name="endNode">always the player node in this case, but for flexablility kept it open to any location.</param>
    /// <param name="avalibleNeighborse">the direction this enemy can walk</param>
    /// <returns></returns>
    public async Task<List<GridNode>> PathFinding(GridNode startNode, GridNode endNode, Neighbour avalibleNeighborse)
    {
        // Set starging node.
        openList.Clear();
        closedList.Clear();

        // Set all default values
        foreach (var item in groundObjects)
        {
            item.Value.gCost = int.MaxValue;
            item.Value.CalculateFCost();
            item.Value.cameFromNode = null;
        }

        // Set the calculated values of the first node
        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        openList.Add(startNode);

        // look rought the whole grid and find the shortest route from start to end node.
        while (openList.Count > 0)
        {
            // If end node has been found
            GridNode currentNode = GetLowestFCostNode(openList);

            if (currentNode == endNode)
            {
                await Task.Delay(1);
                return await CalculatedPath(endNode);
            }

            // remove the checked node
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // Check all the neighbour nodes and check there distance to the end node to compair what node is the most close to the end node
            foreach (var neighbour in currentNode.neighbours)
            {
                if ((neighbour.Key & avalibleNeighborse) != 0)
                {
                    // Check if the neighbore exsists and if its avalible.
                    if (closedList.Contains(neighbour.Value)) continue;
                    if (!neighbour.Value.accesableTerrain)
                    {
                        closedList.Add(neighbour.Value);
                        continue;
                    }

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbour.Value);
                    if (tentativeGCost < neighbour.Value.gCost)
                    {
                        neighbour.Value.cameFromNode = currentNode;
                        neighbour.Value.gCost = tentativeGCost;
                        neighbour.Value.hCost = CalculateDistanceCost(neighbour.Value, endNode);
                        neighbour.Value.CalculateFCost();

                        if (!openList.Contains(neighbour.Value))
                        {
                            openList.Add(neighbour.Value);
                        }
                    }
                }
            }
        }

        await Task.Delay(1);
        return null;
    }

    /// <summary>
    /// PATH FINDING, Look at the end node and what its last neighbore was and this way walk back to the starting postion.
    /// This creates a list that is the wrong way around. But after inversing it it will be the path towards the end possition.
    /// </summary>
    private async Task<List<GridNode>> CalculatedPath(GridNode endNode)
    {
        List<GridNode> path = new List<GridNode>();
        path.Add(endNode);

        GridNode currentNode = endNode;

        // Loop until there are no more came before nodes, meawning where at the start
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        // so the list goes from start to finish instead of finesh to start.
        path.Reverse();
        await Task.Delay(1);
        return path;
    }


    /// <summary>
    /// PATH FINDING, Calculate the total dinstance betuween the start and end node
    /// </summary>
    private int CalculateDistanceCost(GridNode a, GridNode b)
    {
        int xDistance = Mathf.Abs((int)a.location.x - (int)b.location.x);
        int yDistance = Mathf.Abs((int)a.location.y - (int)b.location.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return moveDiagonalCost * Math.Min(xDistance, yDistance) + moveStraightCost * remaining;
    }

    /// <summary>
    /// PATH FINDING, look for the node that has the lowest cost towards its end goal.
    /// </summary>
    private GridNode GetLowestFCostNode(List<GridNode> pathNodeList)
    {
        GridNode lowestFCostNode = pathNodeList[0];

        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }
}
