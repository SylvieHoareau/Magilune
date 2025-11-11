using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class JetpackAbility : MonoBehaviour
{
    [Header("Jetpack Settings")]
    [SerializeField] private float jetPackForce = 7f;
    [SerializeField] private float fuelMax = 1f;
    [SerializeField] private float fuelConsumptionRate = 1f;
    [SerializeField] private float fuelRechargeRate = 0.5f;

    [Header("Effets visuels et audio")]
    [SerializeField] private ParticleSystem jetpackParticles;
    [SerializeField] private ParticleSystem jetpackSmoke;
    [SerializeField] private AudioSource jetPackAudio;

    private Rigidbody2D rb;
    private bool isUsingJetpack = false;
    private float currentFuel;

    /// <summary>
    /// Permet de savoir si le joueur utilise le jetpack actuellement.
    /// </summary>
    public bool IsUsingJetpack => isUsingJetpack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentFuel = fuelMax;

        if (jetpackParticles != null) jetpackParticles.Stop();
        if (jetpackSmoke != null) jetpackSmoke.Stop();
        if (jetPackAudio != null) jetPackAudio.Stop();
    }

    private void FixedUpdate()
    {
        if (isUsingJetpack)
        {
            ApplyJetpackForce();
        }
        else
        {
            RechargeFuel();
        }
    }

    /// <summary>
    /// Démarre le jetpack (appelé par PlayerAbilityManager).
    /// </summary>
    public void StartJetpack()
    {
        if (currentFuel <= 0)
        {
            StopJetpack();
            return;
        }

        isUsingJetpack = true;

        // Activer effets visuels et audio
        if (jetpackParticles != null && !jetpackParticles.isPlaying)
            jetpackParticles.Play();

        if (jetpackSmoke != null && !jetpackSmoke.isPlaying)
            jetpackSmoke.Play();

        if (jetPackAudio != null && !jetPackAudio.isPlaying)
            jetPackAudio.Play();
    }

    /// <summary>
    /// Arrête le jetpack (appelé par PlayerAbilityManager).
    /// </summary>
    public void StopJetpack()
    {
        isUsingJetpack = false;

        if (jetpackParticles != null && jetpackParticles.isPlaying)
            jetpackParticles.Stop();

        if (jetpackSmoke != null && jetpackSmoke.isPlaying)
            jetpackSmoke.Stop();

        if (jetPackAudio != null && jetPackAudio.isPlaying)
            jetPackAudio.Stop();
    }

    /// <summary>
    /// Applique la poussée verticale du jetpack et consomme du carburant.
    /// </summary>
    private void ApplyJetpackForce()
    {
        if (currentFuel <= 0)
        {
            StopJetpack();
            return;
        }

        rb.AddForce(Vector2.up * jetPackForce, ForceMode2D.Force);
        currentFuel -= Time.fixedDeltaTime * fuelConsumptionRate;
        currentFuel = Mathf.Clamp(currentFuel, 0f, fuelMax);
    }

    /// <summary>
    /// Recharge lentement le carburant quand le jetpack n'est pas utilisé.
    /// </summary>
    private void RechargeFuel()
    {
        if (currentFuel < fuelMax)
        {
            currentFuel += Time.fixedDeltaTime * fuelRechargeRate;
            currentFuel = Mathf.Clamp(currentFuel, 0f, fuelMax);
        }
    }

    /// <summary>
    /// Ratio de carburant entre 0 et 1 (utile pour afficher une jauge).
    /// </summary>
    public float CurrentFuelRatio => (fuelMax <= 0) ? 0f : currentFuel / fuelMax;

    /// <summary>
    /// Recharge instantanément le carburant (ex: power-up).
    /// </summary>
    public void RefillFuel() => currentFuel = fuelMax;
}
