using UnityEngine;
[RequireComponent(typeof(CircleCollider2D))]
public class DamageZone : MonoBehaviour
{   
    [Header("References")]
    public MonsterAI monster;
    [Header("Visualization")]
     public Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);
     private CircleCollider2D damageCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(monster == null)
        {
            Debug.LogWarning("[DamageZone] Monster reference is not set. Disabling DamageZone.", this);
        }
    
    damageCollider = GetComponent<CircleCollider2D>();
    if (damageCollider != null)
        {
            damageCollider.isTrigger = true;
        }
    }
    void OnDrawGizmos()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            // Vẽ viền tròn màu đỏ nhạt để canh chỉnh tầm cào của quái
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, col.radius);
        }
    }

    // Update is called once per frame
   void OnDrawGizmosSelected()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            // Khi click chọn Object, vẽ mảng màu đỏ đậm hơn cho dễ nhìn
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(transform.position, col.radius);
        }
    }
}
