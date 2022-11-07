using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Handels the start UI of the game + the character selection process.
/// </summary>
public class StartScreenManager : MonoBehaviour
{
    /// <summary>
    /// All the base character components and settings, made serializable so it shows up in the inspector.
    /// </summary>
    [Serializable]
    public class characterClass
    {
        public string name;
        public Sprite image;
        public int dificulty;
    }

    private static Canvas m_canvas;

    [Header("Connected Components")]
    [SerializeField] private Image[] stars;
    [Space]
    [SerializeField] private Image m_characterLeft;
    [SerializeField] private Image m_characterRight;
    [SerializeField] private Image m_characterCenter;
    [SerializeField] private TextMeshProUGUI m_nameText;

    [Header("All Avalible Characters")]
    [SerializeField] private characterClass[] m_characterSprites;

    // Position in the character selection UI
    private int m_currentCharacter = 1;

    // Block key inputs when in game.
    private static bool m_currentlyInGame = false;

    private void Start()
    {
        m_canvas = GetComponent<Canvas>();
        m_canvas.enabled = true;

        Setupm_currentCharacters();
    }

    /// <summary>
    /// Show the Start canvas and turn on the UI input buttons.
    /// </summary>
    public static void ShowCanvas()
    {
        m_canvas.enabled = true;
        m_currentlyInGame = false;
    }

    // There are multiple ways of getting the direction keys, as an example I use the axis here instead of hard key presses. 
    private void Update()
    {
        if (!m_currentlyInGame)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                RightArrowButton();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                LeftArrowButton();
            }
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                StartButton();
            }
        }
    }

    /// <summary>
    /// Move the character selection carousel to the right.
    /// </summary>
    public void RightArrowButton()
    {
        SoundManager.s_insance.UIButtonClick();

        if (m_currentCharacter < m_characterSprites.Length -1)
        {
            m_currentCharacter++;
            Setupm_currentCharacters();
        }       
    }

    /// <summary>
    /// Move the character selection carousel to the left.
    /// </summary>
    public void LeftArrowButton()
    {
        SoundManager.s_insance.UIButtonClick();

        if (m_currentCharacter > 0)
        {
            m_currentCharacter--;
            Setupm_currentCharacters();
        }
    }

    /// <summary>
    /// Show the current character and the ones before and after the current one to create a selection menu.
    /// If there is no posible character to the left or right in the list (+1 or -1) turn that image off since there is nothing to show.
    /// </summary>
    private void Setupm_currentCharacters()
    {
        m_characterCenter.sprite = m_characterSprites[m_currentCharacter].image;
        m_nameText.text = m_characterSprites[m_currentCharacter].name;

        if (m_currentCharacter > 0)
        {
            m_characterLeft.enabled = true;
            m_characterLeft.sprite = m_characterSprites[m_currentCharacter - 1].image;
        }
        else
        {
            m_characterLeft.enabled = false;
        }

        if (m_currentCharacter < m_characterSprites.Length - 1)
        {
            m_characterRight.enabled = true;
            m_characterRight.sprite = m_characterSprites[m_currentCharacter + 1].image;
        }
        else
        {
            m_characterRight.enabled = false;
        }

        for (int i = 0; i < stars.Length; i++)
        {
            if (i < m_characterSprites[m_currentCharacter].dificulty)
            {
                stars[i].enabled = true;
            }
            else
            {
                stars[i].enabled = false;
            }
        }
    }

    /// <summary>
    /// BUTTON: Set everything ready and start a new game.
    /// </summary>
    public void StartButton()
    {
        m_currentlyInGame = true;
        m_canvas.enabled = false;

        SoundManager.s_insance.StartButtonClick();
        GameManager.s_instance.SetupNewCharacter(m_characterSprites[m_currentCharacter].image, m_characterSprites[m_currentCharacter].dificulty);

        GameManager.s_instance.StartGame();
    }
}
