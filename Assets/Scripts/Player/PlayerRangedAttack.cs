// Dans un script gérant l'attaque à distance du Player, si vous en aviez un.
using UnityEngine;
using System.Collections;

public class PlayerRangedAttack : MonoBehaviour
{
    // C'est ICI que vous glisserez votre Prefab StarProjectile depuis l'éditeur.
    [Header("Projectile")]
    [SerializeField] private StarProjectile projectilePrefab;
    [SerializeField] private Transform firePoint; // L'enfant FirePoint que nous avons discuté

    // Champs pour une attaque en rafale (burst fire)
    [Header("Rafale (Burst Fire)")]
    [Tooltip("Nombre de projectiles à tirer par rafale")]
    [SerializeField] private int burstCount = 3;
    [Tooltip("Délai entre chaque projectile dans la rafale (en secondes)")]
    [SerializeField] private float timeBetweenShots = 0.1f;

    // État pour éviter de lancer une nouvelle rafale pendant que l'ancienne est en cours
    private bool isBursting = false;

    // Déclaration de champ au niveau de la classe
    private WaitForSeconds shotDelay; 

    // Dans Awake, on initialise le WaitForSeconds
    private void Awake()
    {
        // Allocation unique !
        shotDelay = new WaitForSeconds(timeBetweenShots); 
    }


    /// <summary>
    /// Point d'entrée de l'attaque. Démarre la Coroutine de tir en rafale.
    /// </summary>
    public void PerformAttack()
    {
        if (projectilePrefab == null || firePoint == null) return;

        // 1. Instanciation: Crée une COPIE du Prefab dans la scène.
        StarProjectile newProjectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        // 2. Lancement (même logique de direction que les ennemis)
        // La direction est basée sur le flip (localScale.x) du joueur
        float directionX = transform.localScale.x > 0 ? 1f : -1f;
        Vector2 shootDirection = new Vector2(directionX, 0f);

        newProjectile.Launch(shootDirection);

        // Démarre la séquence de tir
        StartCoroutine(BurstFireSequence());
    }

    /// <summary>
    /// Coroutine gérant la séquence de tir en rafale.
    /// </summary>
    private IEnumerator BurstFireSequence()
    {
        isBursting = true; // Le joueur ne peut pas attaquer à nouveau immédiatement

        // Déterminer la direction de tir une seule fois (basée sur le flip du joueur)
        float directionX = transform.localScale.x > 0 ? 1f : -1f;
        Vector2 shootDirection = new Vector2(directionX, 0f);

        // Boucle pour tirer le nombre défini de projectiles
        for (int i = 0; i < burstCount; i++)
        {
            // 1. Instanciation: Crée une COPIE du Prefab
            StarProjectile newProjectile = Instantiate(
                projectilePrefab,
                firePoint.position,
                firePoint.rotation
            );

            // 2. Lancement
            newProjectile.Launch(shootDirection);

            // 3. Délai: On attend un court instant avant le prochain tir
            // Le WaitForSeconds alloue de la mémoire (petite allocation), mais c'est acceptable pour des tirs du joueur.
            yield return new WaitForSeconds(timeBetweenShots);
        }

        isBursting = false; // La rafale est terminée

        yield return shotDelay; // Utilisation de l'objet pré-alloué
    }
}