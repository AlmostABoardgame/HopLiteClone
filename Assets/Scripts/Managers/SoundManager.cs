using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Managing all the in game sound effects.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager s_insance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource m_playerAudio;
    [SerializeField] private AudioSource m_enemyAudio;
    [SerializeField] private AudioSource m_generalAudio;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip m_playerRange;
    [SerializeField] private AudioClip m_playerMelee;
    [SerializeField] private AudioClip m_playerMove;
    [SerializeField] private AudioClip m_playerHit;
    [Space]
    [SerializeField] private AudioClip m_enemyMove;
    [SerializeField] private AudioClip m_enemyJump;
    [SerializeField] private AudioClip m_enemyHit;
    [Space]
    [SerializeField] private AudioClip m_buttonClick;
    [SerializeField] private AudioClip m_startGamem_buttonClick;
    [SerializeField] private AudioClip m_gameOver;

    private void Awake()
    {
        s_insance = this;
    }

    #region Player Sounds

    public void PlayerRangeAttack()
    {
        m_playerAudio.PlayOneShot(m_playerRange);
    }

    public void PlayerMeleeAttack()
    {
        m_playerAudio.PlayOneShot(m_playerMelee);
    }

    public void PlayerMovement()
    {
        m_playerAudio.PlayOneShot(m_playerMove);
    }

    public void PlayerGetHit()
    {
        m_playerAudio.PlayOneShot(m_playerHit);
    }

    #endregion

    #region Enemy Sounds

    public void EnemyAttack(AudioClip soundFx)
    {
        m_enemyAudio.PlayOneShot(soundFx);
    }

    public void EnemyGetHit()
    {
        m_enemyAudio.PlayOneShot(m_enemyHit);
    }

    public void EnemyMovement()
    {
        m_enemyAudio.PlayOneShot(m_enemyMove);
    }

    public void EnemyJump()
    {
        m_enemyAudio.PlayOneShot(m_enemyJump);
    }

    #endregion

    #region General Sounds

    public void UIButtonClick()
    {
        m_generalAudio.PlayOneShot(m_buttonClick);
    }

    public void StartButtonClick()
    {
        m_generalAudio.PlayOneShot(m_startGamem_buttonClick);
    }

    public void GameOver()
    {
        m_generalAudio.PlayOneShot(m_gameOver);
    }

    #endregion
}