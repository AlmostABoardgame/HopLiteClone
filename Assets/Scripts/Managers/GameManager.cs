using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

/// <summary>
/// Manages scoring and main functionality of the game.
/// At first it instances all the objects and charaters on the grid.
/// Then it will move all the components and turns the player on/ off when need to.
/// </summary>
public class GameManager : MonoBehaviour
{
    // For access troughout the whole program.
    public static GameManager s_instance;

    [Header("Settings")]
    [SerializeField] private int m_amountOfLevelProps;
    [Tooltip("Uses async wait timer, so 1f unity seconds is 1000")] [SerializeField] private int m_waitTimeBetweenEnemies = 500;

    [Header("Prefabs")]
    public PlayerCharacterComponent playerPrefab;
    public CharacterComponent enemyAPrefab;
    public CharacterComponent enemyBPrefab;
    public CharacterComponent enemyCPrefab;
    public CharacterComponent enemyDPrefab;
    public CharacterComponent enemyEPrefab;
    public CharacterComponent enemyFPrefab;
    public List<CharacterComponent> propPrefabs = new List<CharacterComponent>();

    [Header("Connected Components")]
    // The move controller for the player that can be turned on/off when need to
    [SerializeField] private Image m_characterImage;
    [SerializeField] private TextMeshProUGUI m_playersHealthText;

    // Data lists
    private List<CharacterComponent> m_allEnemies = new List<CharacterComponent>();
    private List<CharacterComponent> m_allProps = new List<CharacterComponent>();

    [Header("Scoring")]
    [SerializeField] private TextMeshProUGUI m_coinScoreText;
    [SerializeField] private TextMeshProUGUI m_totalEnemiesText;
    [SerializeField] private TextMeshProUGUI m_nextLevelText;

    [Header("Current Enemy UI")]
    [SerializeField] private GameObject m_currentEnemyCanvas;
    [SerializeField] private TextMeshProUGUI m_currentEnemyText;

    [Header("End screens")]
    [SerializeField] private Canvas m_gameOverCanvas;
    [SerializeField] private Canvas m_victoryCanvas;

    // Player Components
    [HideInInspector] public CharacterController thePlayer;

    // Internal values - scoring
    private int m_totalDestroyedEnemies;
    private int m_totalEnemies;
    private int m_totalScore;

    // Internal values - level settings
    private int m_playersHealth;
    private int m_currentDificulty;
    private int m_level;

    // To keep the hierarchy a bit cleaner, place all the props in a holder instead of in the root of the hoagy
    private Transform m_propsHolder;

    private void Awake()
    {
        s_instance = this;

        m_propsHolder = new GameObject().transform;
        m_propsHolder.name = m_propsHolder.ToString();
    }

    /// <summary>
    /// Setup the game using the character and dificulty sellected in the Start Menu.
    /// </summary>
    public void SetupNewCharacter(Sprite newCharacter, int dificulty)
    {
        m_characterImage.sprite = newCharacter;
        m_currentDificulty = dificulty;
        ScorePoints(0);
    }  

    /// <summary>
    /// Start the game, set all the default settings to there start settings and instanciate a new player.
    /// </summary>
    public void StartGame()
    {
        m_totalScore = 0;
        m_playersHealth = 3;
        m_playersHealthText.text = m_playersHealth.ToString();

        // Reset the scoring UI, simply by adding 0 to the 0 score, so it will show 0.
        ScorePoints(0);

        // Instance player
        PlayerCharacterComponent player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        GridManager.PlaceOnTheGrid(player, new Vector2(0, 0));
        thePlayer = player.GetComponent<CharacterController>();
        player.SetupPlayerIcon(m_characterImage.sprite);

        CreateNewm_level();
    }

    /// <summary>
    /// Create a new m_level, this at the start of during the game.
    /// It removes all props and resets the m_level and enemy settings to 0
    /// It then creates all Enemy's using the base amount + caluclations so the higher the m_level and dificulty the more dificult the m_levels will become.
    /// </summary>
    public void CreateNewm_level()
    {
        RemoveAllObjects();
        m_totalEnemies = 0;
        m_totalDestroyedEnemies = 0;

        // Instance enemies
        for (int i = 0; i < Mathf.FloorToInt((m_currentDificulty + m_level) * 0.40f) + 3; i++)
        {
            m_allEnemies.Add(AddObject(enemyAPrefab));
        }
        for (int i = 0; i < Mathf.FloorToInt((m_currentDificulty + m_level) * 0.15f) + 1; i++)
        {
            m_allEnemies.Add(AddObject(enemyBPrefab));
        }
        for (int i = 0; i < Mathf.FloorToInt((m_currentDificulty + m_level) * 0.15f) + 2; i++)
        {
            m_allEnemies.Add(AddObject(enemyCPrefab));
        }
        for (int i = 0; i < Mathf.FloorToInt((m_currentDificulty + m_level) * 0.25f); i++)
        {
            m_allEnemies.Add(AddObject(enemyDPrefab));
        }
        for (int i = 0; i < Mathf.FloorToInt((m_currentDificulty + m_level) * 0.20f); i++)
        {
            m_allEnemies.Add(AddObject(enemyEPrefab));
        }
        for (int i = 0; i < Mathf.FloorToInt((m_currentDificulty + m_level) * 0.35f); i++)
        {
            m_allEnemies.Add(AddObject(enemyFPrefab));
        }

        m_totalEnemies = m_allEnemies.Count;

        // Instance m_level props, a prop also has a character component but all its functions are deactivated
        // This is an easy way for other systems to check if a node is full (witch it will be) and when removeing this prop the node will become free again.
        // So in this demo there is never the need to rebuild the grid, just remove this prop and its done.
        for (int i = 0; i < m_amountOfLevelProps; i++)
        {
            CharacterComponent newProp = AddObject(propPrefabs[Random.Range(0, propPrefabs.Count - 1)]);
            newProp.transform.SetParent(m_propsHolder);
            newProp.characterLocation.accesableTerrain = false;
            m_allProps.Add(newProp);
        }

        UpdateCurrentEnemyStatus();

        // Set first highlight
        foreach (var enemy in m_allEnemies) enemy.HighlightNextMove();
    }

    /// <summary>
    /// Instance a new character onto the grid.
    /// </summary>
    /// <returns>Newly created character</returns>
    private CharacterComponent AddObject(CharacterComponent createObject)
    {
        CharacterComponent newObject = Instantiate(createObject, Vector3.zero, Quaternion.identity);
        GridManager.PlaceOnTheGrid(newObject, GridManager.s_instance.GetRandomEmptyLocation());

        return newObject;
    }

    /// <summary>
    /// Start the enemy movement phase, but only if the player is still alive. And remove all highlighed nodes
    /// </summary>
    public void MoveTheEnemies()
    {
        m_currentEnemyText.text = "";
        m_currentEnemyCanvas.SetActive(true);

        if (thePlayer != null) thePlayer.enabled = false;
        GridManager.TurnOffAllHightlights();

        if (m_playersHealth > 0)
        {
           EnemyMovement();
        }        
    }

    /// <summary>
    /// The enmey movement loop
    /// Show the current active enemy in the UI and activate it. This enemy will check if the location of the player is near by or in its shooting direction/ distance.
    /// To check all these things needs some time, so we use a Async task system to finish one enemies activation completly before moving to the next one.
    /// </summary>
    public async void EnemyMovement()
    {
        await Task.Delay(m_waitTimeBetweenEnemies);

        int currentEnemyNumber = 0;

        if (m_allEnemies.Count > 0)
        {
            foreach (var enemy in m_allEnemies)
            {
                // Check if the enemy is still pressent in the scene, if so activate it
                if (enemy.gameObject.activeInHierarchy)
                {
                    currentEnemyNumber++;
                    m_currentEnemyText.text = "Enemy " + currentEnemyNumber;

                    if (!await enemy.ActivateEnemy())
                    {
                        Debug.Log("no??");
                        await enemy.Move();
                    }

                    // In case the enemy in question got itself destroyed (Enemy C jumping on the player or by another enemies attack)
                    if (enemy.gameObject.activeInHierarchy) enemy.HighlightNextMove();
                }

                await Task.Delay(m_waitTimeBetweenEnemies);
                enemy.activatedHighlight.SetActive(false);
            }
        }       

        if (thePlayer != null) thePlayer.enabled = true;
        m_currentEnemyCanvas.SetActive(false);
    }

    /// <summary>
    /// When an enemy gets destroyed this tells the game manager to change points and the total enemy amount in game.
    /// If all enemies are destroyed the player won and we contineu to the next one.
    /// </summary>
    public void EnemyDestroyed(int scoreValue)
    {
        m_totalDestroyedEnemies++;
        UpdateCurrentEnemyStatus();

        ScorePoints(scoreValue);

        if (m_totalEnemies == m_totalDestroyedEnemies)
        {
            GameWon();
        }
    }

    /// <summary>
    /// Update the current amount of active enemies in the UI.
    /// </summary>
    public void UpdateCurrentEnemyStatus()
    {
        m_totalEnemiesText.text = m_totalDestroyedEnemies + "<#c5c8d0>/" + m_totalEnemies;
    }

    /// <summary>
    /// Add an amount of points to the total score of the player.
    /// </summary>
    void ScorePoints(int amount)
    {
        m_totalScore += amount;
        m_coinScoreText.text = m_totalScore.ToString();
    }

    /// <summary>
    /// Clean all objects (props and enemies) in the scene connected to the game manager.
    /// </summary>
    void RemoveAllObjects()
    {
        if (m_allEnemies.Count > 0)
        {
            foreach (var item in m_allEnemies)
            {
                Destroy(item.gameObject);
            }
        }

        if (m_allProps.Count > 0)
        {
            foreach (var item in m_allProps)
            {
                item.characterLocation.accesableTerrain = true;
                Destroy(item.gameObject);
            }
        }

        m_allEnemies.Clear();
        m_allProps.Clear();
    }

    /// <summary>
    /// The player gets hit, if it's health reaches 0 the player has lost. 
    /// </summary>
    public void PlayerGetsHit()
    {
        m_playersHealth--;
        m_playersHealthText.text = m_playersHealth.ToString();

        if (m_playersHealth <= 0)
        {
            GameOver();
        }
    }

    /// <summary>
    /// Show the completion of this m_level.
    /// </summary>
    public void GameWon()
    {
        m_level++;
        m_nextLevelText.text = (m_level + 1).ToString();

        m_victoryCanvas.enabled = true;
        thePlayer.enabled = false;

        StartCoroutine(Nextm_levelWait());
    }

    /// <summary>
    /// Continueing to the next m_level after a short wait.
    /// </summary>
    public IEnumerator Nextm_levelWait()
    {
        yield return new WaitForSeconds(1f);

        GridManager.ResetTheGrid();
        CreateNewm_level();

        m_victoryCanvas.enabled = false;
        thePlayer.enabled = true;
    }

    /// <summary>
    /// The player has lost, reset the m_level and get ready to reset the game.
    /// </summary>
    private void GameOver()
    {
        SoundManager.s_insance.GameOver();
        m_level = 0;
        thePlayer.characterComponent.playerDies();
        StartCoroutine(ShowGameOverUI());
    }

    /// <summary>
    /// After a short wait show the Game Over UI since the player has lost
    /// </summary>
    private IEnumerator ShowGameOverUI()
    {
        yield return new WaitForSeconds(1f);
        m_gameOverCanvas.enabled = true;
    }

    /// <summary>
    /// Restart the game from m_level 1 using the same character and dificulty
    /// </summary>
    public void RetryButton()
    {
        GridManager.ResetTheGrid();

        SoundManager.s_insance.StartButtonClick();
        m_gameOverCanvas.enabled = false;

        s_instance.StartGame();
    }

    /// <summary>
    /// Go back to the character sellection and fully restart the game.
    /// </summary>
    public void MainMenuButton()
    {
        GridManager.ResetTheGrid();

        SoundManager.s_insance.UIButtonClick();
        StartScreenManager.ShowCanvas();
        m_gameOverCanvas.enabled = false;
    }
}