using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Health Settings")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP = 100;
    
    [Header("Damage Settings")]
    [SerializeField] private float invincibilityDuration = 1f; // Vô địch tạm thời sau khi bị damage
    [SerializeField] private float damageFlashDuration = 0.1f;
    private bool isInvincible = false;

    [Header("UI References")]
    public Image healthBar; // Đã đổi sang Image cho dạng Fill
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Image damageVignette; // Màn hình đỏ khi nhận damage

    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private GameObject deathScreen; // UI hiện khi chết

    [Header("Audio")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    // Đã xóa healSound vì game không có cơ chế hồi máu
    private AudioSource audioSource;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Load HP từ GameState nếu có
        if (GameState.TryGet(out GameState state))
        {
            int savedHP = state.PlayerHP;
            if (savedHP > 0)
            {
                currentHP = savedHP;
                if (verboseLogs) Debug.Log($"[PlayerHealth] Loaded HP from GameState: {currentHP}/{maxHP}", this);
            }
        }
        else
        {
            currentHP = maxHP;
        }

        UpdateUI();

        if (deathScreen != null) deathScreen.SetActive(false);
        if (damageVignette != null)
        {
            Color c = damageVignette.color;
            c.a = 0f;
            damageVignette.color = c;
        }
    }

    /// <summary>
    /// Gây damage cho player. Gọi từ Monster hoặc trap.
    /// </summary>
    public void TakeDamage(int damage, GameObject source = null)
    {
        if (isInvincible)
        {
            if (verboseLogs) Debug.Log("[PlayerHealth] Invincible, damage ignored.", this);
            return;
        }

        if (currentHP <= 0)
        {
            return; // Đã chết rồi
        }

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        if (verboseLogs)
        {
            string sourceName = source != null ? source.name : "Unknown";
            Debug.Log($"[PlayerHealth] Took {damage} damage from {sourceName}. HP: {currentHP}/{maxHP}", this);
        }

        // Save HP vào GameState
        if (GameState.TryGet(out GameState state))
        {
            state.SetHP(currentHP);
        }

        UpdateUI();
        PlaySound(damageSound);
        StartCoroutine(DamageFlashEffect());
        StartCoroutine(InvincibilityCoroutine());

        if (currentHP <= 0)
        {
            Die();
        }
    }


    void Die()
    {
        if (verboseLogs) Debug.Log("[PlayerHealth] Player died!", this);

        if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.PlayerDie);
        PlaySound(deathSound);

        // Hiện death screen
        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
        }

        // Disable player movement
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (verboseLogs) Debug.Log("[PlayerHealth] Respawning from checkpoint...", this);

        // Respawn từ checkpoint
        if (GameState.TryGet(out GameState state))
        {
            state.RespawnFromCheckpoint(); // GameState sẽ tự gọi RestoreFullHealth() trong luồng của nó
        }
        else
        {
            Debug.LogError("[PlayerHealth] Cannot respawn: GameState is NULL!");
        }
    }

    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    IEnumerator DamageFlashEffect()
    {
        if (damageVignette == null) yield break;

        // Flash đỏ màn hình
        Color c = damageVignette.color;
        c.a = 0.5f;
        damageVignette.color = c;

        yield return new WaitForSeconds(damageFlashDuration);

        c.a = 0f;
        damageVignette.color = c;
    }

    void UpdateUI()
    {
        if (healthBar != null)
        {
            // Cập nhật giá trị UI Image Fill (từ 0 đến 1)
            healthBar.fillAmount = (float)currentHP / maxHP; 
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHP} / {maxHP}";
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // --- GETTERS ---
    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public bool IsAlive => currentHP > 0;
    public bool IsInvincible => isInvincible;
    public float HealthPercent => (float)currentHP / maxHP;

    // --- DEBUG ---
    [ContextMenu("Take 10 Damage (Test)")]
    void DebugTakeDamage()
    {
        TakeDamage(10);
    }

    [ContextMenu("Kill Player (Test)")]
    void DebugKill()
    {
        TakeDamage(currentHP);
    }
}