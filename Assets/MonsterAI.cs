using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MonsterAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 4.2f;
    public float waitTimeAtPoint = 1f;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    [Header("Chase Settings")]
    public Transform player; 
    public float chaseSpeed = 5.5f;
    public float detectionRadius = 4f;     
    public float loseInterestDistance = 6f; 
    public float loseInterestTime = 2f;
    private bool isChasing = false;
    private Coroutine loseInterestCoroutine;
    [Header("Flashlight Stun")]
    private FlashlightController flashlight;
    public float stunDuration = 2f;         
    public string flashlightTag = "Flashlight"; 
    private bool isStunned = false;
    private float stunTimer = 0f;

    [Header("Damage Settings")]
    public int damageAmount = 1;           
    public float damageInterval = 0.6f;      
    public float damageRange = 1.5f;        
    private float damageTimer = 0f;
    private bool playerInDamageZone = false;

    [Header("Spawn Conditions")]
    [Tooltip("Only spawn during blackout (electricity out)")]
    public bool onlySpawnDuringBlackout = true;
    private bool hasSpawned = false;
    private float spawnDelay = 1f; // Delay before spawning after blackout
    private float spawnTimer = 0f;
    private bool navigationReady = false;

    private Vector2 lastDirect = Vector2.right;
    public static bool HasMonsterAppeared { get; private set; } = false;

    void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        if (agent == null)
        {
            Debug.LogError("[MonsterAI] NavMeshAgent component is missing.", this);
            enabled = false;
            return;
        }
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        if (onlySpawnDuringBlackout)
        {
            SetMonsterVisible(false);
            Debug.Log("[MonsterAI] Monster hidden, waiting for blackout to spawn.", this);
            return;
        }

        StartCoroutine(InitAfterNavMesh());
    }

    IEnumerator InitAfterNavMesh()
    {
        navigationReady = false;

        for (int attempt = 0; attempt < 60; attempt++)
        {
            if (agent != null && agent.isActiveAndEnabled &&
                NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas) &&
                agent.Warp(hit.position))
            {
                navigationReady = true;
                Debug.Log($"[MonsterAI] NavMesh ready at: {hit.position}", this);
                GoToNextPatrol();
                yield break;
            }

            yield return null;
        }

        Debug.LogError("[MonsterAI] Could not place monster on a NavMesh. Bake the walkable floor and place the spawn point within 10 units of it.", this);
    }

    void Update()
    {
        
    
        if (player == null || !player.gameObject.scene.IsValid())
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                player = playerGO.transform;
                flashlight = player.GetComponent<FlashlightController>();
                Debug.Log("[MonsterAI] Player found!");
            }
        }
        
        // Check blackout condition and spawn monster with delay
        if (onlySpawnDuringBlackout)
        {
            if (GameState.TryGet(out GameState state))
            {
                if (state.ElectricityOut && !hasSpawned)
                {
                    // Add delay before spawning for dramatic effect
                    spawnTimer += Time.deltaTime;
                    if (spawnTimer >= spawnDelay)
                    {
                        SpawnMonster();
                        spawnTimer = 0f;
                    }
                }
                else if (!state.ElectricityOut && hasSpawned)
                {
                    DespawnMonster();
                }
            }
        }

        
        if (!navigationReady || !agent.isActiveAndEnabled || !agent.isOnNavMesh || player == null) return;

        
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
                Debug.Log("[MonsterAI] Stun ended, resuming movement.", this);
            }
            else
            {
                // stop agent movement while stunned
                StopMovement();
                UpdateAnimation(); 
                return;
            }
        }

        // check player detection and update chasing state
        CheckPlayerDetection();

        if (isChasing)
        {
            agent.isStopped = false;
            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);
        }
        else if (!isWaiting)
        {
            agent.speed = patrolSpeed;
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                // call WaitThenPatrol() coroutine to wait at the patrol point before moving to the next
                StartCoroutine(WaitThenPatrol()); 
            }
        }

        UpdateAnimation();
        
        
        if (playerInDamageZone && !isStunned)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                DamagePlayer();
                damageTimer = 0f;
            }
        }
    }

    // detect player within detection radius and manage chasing state
    void CheckPlayerDetection()
    {
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

       
        if (distanceToPlayer <= detectionRadius)
        {
            isChasing = true;
            if (loseInterestCoroutine != null)
            {
                StopCoroutine(loseInterestCoroutine);
                loseInterestCoroutine = null;
            }
        }
        //
        else if (isChasing && distanceToPlayer > loseInterestDistance && loseInterestCoroutine == null)
        {
            loseInterestCoroutine = StartCoroutine(LoseInterestTimer());
        }
    }

    // --- PATROL LOGIC ---
    IEnumerator WaitThenPatrol()
    {
        isWaiting = true;
        StopMovement();
        yield return new WaitForSeconds(waitTimeAtPoint);
        if (!navigationReady || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            isWaiting = false;
            yield break;
        }

        agent.isStopped = false;
        GoToNextPatrol();
        isWaiting = false;
    }
    void UpdateAnimation()
    {
        Vector2 velocity = new Vector2(agent.velocity.x, agent.velocity.y);
        float speed = velocity.magnitude;
        anim.SetFloat("Speed", speed);
        
        if (speed > 0.1f)
        {   
            // Debug.Log($"Velocity: X={agent.velocity.x:F2} Y={agent.velocity.y:F2} Z={agent.velocity.z:F2}"); // Removed to stop console spam
            float maxSpeed = isChasing ? chaseSpeed : patrolSpeed;
            
            anim.SetFloat("Speed", Mathf.Clamp01(speed / maxSpeed));
            Vector2 dir = velocity.normalized;
            anim.SetFloat("MoveX", dir.x);
            anim.SetFloat("MoveY", dir.y);
            lastDirect = dir;
        }
        else
        {
            anim.SetFloat("MoveX", lastDirect.x);
            anim.SetFloat("MoveY", lastDirect.y);
        }
    }

    void GoToNextPatrol()
    {
        if (!navigationReady || !agent.isActiveAndEnabled || !agent.isOnNavMesh || patrolPoints == null || patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    IEnumerator LoseInterestTimer()
    {
        yield return new WaitForSeconds(loseInterestTime);
        isChasing = false;
        loseInterestCoroutine = null; 
        GoToNextPatrol();
    }

    // --- TRIGGER DETECTION: FLASHLIGHT STUN & PLAYER DAMAGE ---
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[DEBUG] Monster Trigger Enter: name='{other.name}', tag='{other.tag}', layer={LayerMask.LayerToName(other.gameObject.layer)}, position={other.transform.position}", this);
        
        // Flashlight stun
        if (other.CompareTag(flashlightTag))
        {
            TriggerStun();
        }

        if (other.CompareTag("Player"))
        {
            playerInDamageZone = true;
            damageTimer = 0f; 
            Debug.Log("[MonsterAI] Player entered damage zone!", this);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        
        if (other.CompareTag(flashlightTag) && isStunned)
        {
            stunTimer = stunDuration; 
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        
        if (other.CompareTag("Player"))
        {
            playerInDamageZone = false;
            damageTimer = 0f; // Reset timer khi rời
            Debug.Log("[MonsterAI] Player exited damage zone!", this);
        }
    }

    void TriggerStun()
    {   
        Debug.Log($"[DEBUG] TriggerStun() called. Current isStunned={isStunned}, stunTimer={stunTimer:F2}", this);
        
        if (isStunned)
        {
            Debug.Log("[DEBUG] Already stunned - extending stun timer", this);
            stunTimer = stunDuration; // Extend stun duration instead of ignoring
            return;
        }

        isStunned = true;
        stunTimer = stunDuration;
        StopMovement();

        Debug.Log($"[MonsterAI] Monster stunned by flashlight for {stunDuration}s!", this);

        
        if (loseInterestCoroutine != null)
        {
            StopCoroutine(loseInterestCoroutine);
            loseInterestCoroutine = null;
        }
    }

    // --- MONSTER SPAWN/DESPAWN ---
    
    void SpawnMonster()
    {
        hasSpawned = true;
        HasMonsterAppeared = true;
        SetMonsterVisible(true);
        
        Debug.Log("[MonsterAI] Monster spawned! Blackout has attracted it...", this);
        
        // Play spawn sound if available
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlaySFX(SFXManager.SFXType.MonsterSpawn);
        }
        
        StartCoroutine(InitAfterNavMesh());
    }
    void SetMonsterVisible(bool visible)
    {
        if (!visible) navigationReady = false;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = visible;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = visible;

        if (agent != null) agent.enabled = visible;
    }

    void StopMovement()
    {
        if (!navigationReady || agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }
    void DespawnMonster()
    {
        hasSpawned = false;
        HasMonsterAppeared = false; // Reset for next blackout cycle
        SetMonsterVisible(false);
        Debug.Log("[MonsterAI] Monster despawned (power restored).", this);
    }
    void DamagePlayer()
    {
        if (PlayerHealth.Instance != null)
        {
            if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.MonsterAttack);
            PlayerHealth.Instance.TakeDamage(damageAmount, gameObject);
            Debug.Log($"[MonsterAI] Damaged player for {damageAmount} HP.", this);
        }
        else
        {
            Debug.LogWarning("[MonsterAI] PlayerHealth.Instance not found!", this);
        }
    }
}