using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Animator _animator;

    private static readonly int AttackTriggerHash = Animator.StringToHash("AttackTrigger");

    // S'assurer que les dépendances sont là
    private void Awake()
    {
        if (_animator == null)
        {
            Debug.LogError("Animator manquant sur PlayerAttack.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Détection de l'input d'attaque (par exemple, la touche Espace)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Vérifier si le joueur n'est pas déjà en train d'attaquer ou de mourir
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                // Déclenche la transition dans l'Animator Controller
                _animator.SetTrigger(AttackTriggerHash);
            }
        }
    }
}
