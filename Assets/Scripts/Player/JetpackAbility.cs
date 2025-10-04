using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class JetpackAbility : MonoBehaviour
{
    [Header("Jetpack Settings")]
    [SerializeField] private float jetPackForce = 7f;
    [SerializeField] private float fuelMax = 1f;
    [SerializeField] private ParticleSystem jetpackParticles;
    [SerializeField] private ParticleSystem jetpackSmoke;
    [SerializeField] private AudioSource jetPackAudio;


    private Rigidbody2D rb;
    private bool isUsingJetpack;
    private float currentFuel;

     // Propriété publique pour vérifier si le joueur utilise le jetpack
    public bool IsUsingJetpack => isUsingJetpack;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentFuel = fuelMax;
        if (jetpackParticles != null)
        {
            jetpackParticles.Stop();
        }
    }

    public void HandleJetPack(bool isPressed)
    {
        // Si le carburant est épuisé, arrêter le jetpack
        if (currentFuel <= 0)
        {
            StopJetpack();
            return;
        }

        // Activer ou désactiver le jetpack en fonction de l'entrée
        if (isPressed)
        {
            UseJetPack();
        }
        else
        {
            StopJetpack();
        }
    }

    private void UseJetPack()
    {
        isUsingJetpack = true;
        rb.AddForce(Vector2.up * jetPackForce, ForceMode2D.Force);

        // Consommer le carburant
        currentFuel -= Time.deltaTime;

        // Jouer les particules du jetpack
        if (jetpackParticles != null && !jetpackParticles.isPlaying)
        {
            jetpackParticles.Play();
        }

        // Ajouter de la fumée lorsque le jetpack est en marche
        if (jetpackSmoke != null && !jetpackSmoke.isPlaying)
        {
            jetpackSmoke.Play();
        }

        // Jouer le son du jetpack
        if (jetPackAudio != null && !jetPackAudio.isPlaying)
        {
            jetPackAudio.Play();
        }
    }

    private void StopJetpack()
    {
        isUsingJetpack = false;

        if (jetpackParticles != null && jetpackParticles.isPlaying)
        {
            jetpackParticles.Stop();
        }

        if (jetpackSmoke != null && jetpackSmoke.isPlaying)
        {
            jetpackSmoke.Stop();
        }

        if (jetPackAudio != null && jetPackAudio.isPlaying)
        {
            jetPackAudio.Stop();
        }
    }

    /// <summary>
    /// Retourne le ratio de carburant actuel (entre 0.0 et 1.0).
    /// </summary>
    public float CurrentFuelRatio
    {
        get
        {
            // Évite la division par zéro si fuelMax est accidentellement à 0
            if (fuelMax <= 0) return 0f; 
            return currentFuel / fuelMax;
        }
    }

    public void RefillFuel()
    {
        currentFuel = fuelMax;
    }
}
