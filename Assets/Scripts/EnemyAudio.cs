using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
    [Header("Audio Source Settings")]
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [Tooltip("Random snarls played while patrolling or idling.")]
    public AudioClip[] idleSnarls;
    [Tooltip("Screams triggered when entering chase mode.")]
    public AudioClip[] chaseScreams;
    [Tooltip("Roars/swings played when performing attacks.")]
    public AudioClip[] attackSounds;
    [Tooltip("Impact sounds played when taking damage.")]
    public AudioClip[] hurtSounds;
    [Tooltip("The final roar/screech played upon dying.")]
    public AudioClip[] deathSounds;

    [Header("Idle Snarl Timer (Seconds)")]
    public float minIdleInterval = 4f;
    public float maxIdleInterval = 10f;
    private float nextIdleSoundTime;

    private Animator animator;
    private bool isDead = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Configure default 3D audio settings
        audioSource.spatialBlend = 1.0f; // 100% 3D spatial audio
        audioSource.playOnAwake = false;

        ResetIdleTimer();
    }

    private void Update()
    {
        if (isDead || animator == null || idleSnarls.Length == 0) return;

        // Only play random snarls if not actively chasing or attacking
        bool isChasing = animator.GetBool("isChasing");
        bool isAttacking = animator.GetBool("isAttacking");

        if (!isChasing && !isAttacking && Time.time >= nextIdleSoundTime)
        {
            PlayRandomFromGroup(idleSnarls, 0.5f); // Quieter for ambient idle murmurs
            ResetIdleTimer();
        }
    }

    public void PlayChase()
    {
        if (isDead) return;
        PlayRandomFromGroup(chaseScreams, 0.9f);
    }

    public void PlayAttack()
    {
        if (isDead) return;
        PlayRandomFromGroup(attackSounds, 1.0f);
    }

    public void PlayHurt()
    {
        if (isDead) return;
        PlayRandomFromGroup(hurtSounds, 0.8f);
    }

    public void PlayDeath()
    {
        isDead = true;
        audioSource.Stop(); // Cut off any current screams
        PlayRandomFromGroup(deathSounds, 1.0f);
    }

    private void PlayRandomFromGroup(AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0 || audioSource == null) return;

        // Choose a random clip from the group
        AudioClip clip = clips[Random.Range(0, clips.Length)];

        // Random pitch variation so sounds NEVER feel robotic or repetitive
        audioSource.pitch = Random.Range(0.85f, 1.15f);
        audioSource.PlayOneShot(clip, volume);
    }

    private void ResetIdleTimer()
    {
        nextIdleSoundTime = Time.time + Random.Range(minIdleInterval, maxIdleInterval);
    }
}
