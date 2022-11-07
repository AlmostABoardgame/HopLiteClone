using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[Flags]public enum Neighbour
{
    none = 0,
    above = 1 << 1,
    left = 1 << 2,
    right = 1 << 3,
    below = 1 << 4,
    aboveRight = 1 << 5,
    aboveLeft = 1 << 6,
    belowRight = 1 << 7,
    belowLeft = 1 << 8,
}

/// <summary>
/// A node on the grid in the world. This object holds all the basics for this gridnode + the pathfinding and location requirments for it.
/// </summary>
public class GridNode : MonoBehaviour
{
    [Header("Connected Components")]
    public CharacterComponent currentCharacter;
    public GameObject attackEffect;
    [SerializeField] private Renderer m_highLight;

    // All the 8 serounding directions
    public Dictionary<Neighbour, GridNode> neighbours = new Dictionary<Neighbour, GridNode>();
   
    // The grid location of this object.
    public Vector2 location;

    // In case this terrain block would be something else then normal ground. In the current game there is just one terrain type so this would never happen
    // but in case i would like to extent things in a posible future i could add walls, lava or whatever thats not accesable.
    public bool accesableTerrain = true;

    // PathFinding
    [HideInInspector] public int gCost;
    [HideInInspector] public int hCost;
    [HideInInspector] public int fCost;
    [HideInInspector] public GridNode cameFromNode;

    /// <summary>
    /// Add one specific neighboring node to the given neighbor direction
    /// </summary>
    /// <param name="Neighbour"></param>
    /// <param name="node"></param>
    public void AddNeighbore(Neighbour Neighbour, GridNode node)
    {
        neighbours.Add(Neighbour, node);
    }

    /// <summary>
    /// Check of the space is empty so the player or enemy can move towards is.
    /// Enemies however don't always see the player as blocking terrain. So we check if node is avalible except for when the player is there.
    /// Enemy type C for instance can jump onto the player instead of shooting it if falls off its directions but withing range.
    /// </summary>
    /// <returns>if true = terain is concidert empty</returns>
    public bool IsAvalible()
    {
        if (currentCharacter != null && currentCharacter.gameObject.tag != "Player")
        {
            return false;
        }
        return accesableTerrain;
    }

    /// <summary>
    /// Highlight this ground node or turn the hightlight off.
    /// </summary>
    public void HighLightNode(bool isTrue, Color color)
    {
        m_highLight.gameObject.SetActive(isTrue);
        m_highLight.material.color = color;    }


    /// <summary>
    /// Show the attack visual when an attack happens on this node.
    /// To keep things simple I have added the attack directly on the node, instead of having a transform location
    /// and this function having a (GameObject newEffect) to instance on that transform location, to keep things a bit simpler.
    /// </summary>
    public void AttackVisual()
    {
        attackEffect.SetActive(false);
        attackEffect.SetActive(true);
    }

    /// <summary>
    /// PATHFINDING
    /// </summary>
    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
}