using UnityEngine;

public class HazardFeedback : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 1;
    public float knockbackForce = 5f;

    [Header("Feedback")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.2f;
    public AudioClip hitSound;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Récupérer la vie du joueur
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            // Appliquer knockback
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 knockDir = (collision.transform.position - transform.position).normalized;
                rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
            }

            // Feedback visuel
            SpriteRenderer sr = collision.gameObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                StartCoroutine(Flash(sr));
            }

            // Jouer son
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
            }
        }
    }

    private System.Collections.IEnumerator Flash(SpriteRenderer sr)
    {
        Color original = sr.color;
        sr.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        sr.color = original;
    }
}
