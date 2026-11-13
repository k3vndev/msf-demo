using DG.Tweening;
using UnityEngine;

public abstract class LevelDoor : MonoBehaviour
{
    [SerializeField] Transform doorMesh;
    [field: Space, SerializeField] public bool IsOpen { get; private set; } = true;

    [Header("SFX")]
    [SerializeField] AudioClip openSound;
    [SerializeField] AudioClip closeSound;

    readonly float OPEN_ANGLE = -88f;
    readonly float ANIMATION_TIME = 0.75f;

    protected PlayerMovement PlayerMovement { get; private set; }
    protected SledController SledController { get; private set; }

    SFXPlayer sfxPlayer;

    protected virtual void Start()
    {
        PlayerMovement = Utils.GetComp<PlayerMovement>("Player");
        SledController = Utils.GetComp<SledController>("Sled");
        sfxPlayer = GetComponent<SFXPlayer>();
    }

    protected virtual void Update() { }

    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;
        Rotate(OPEN_ANGLE);

        PlayClip(openSound);
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        Rotate(0);

        PlayClip(closeSound);
    }

    void PlayClip(AudioClip clip)
    {
        sfxPlayer
            .PlaySound(clip)?
            .SetPitchRange(-0.25f, 0.05f)
            .SetFadeTime(0.2f, 0.3f)
            .SetVolume(0.3f);
    }

    void Rotate(float target)
    {
        doorMesh.DOKill();
        doorMesh
            .DORotate(Vector3.up * target, ANIMATION_TIME, RotateMode.Fast)
            .SetEase(Ease.OutBack);
    }

    void OnValidate()
    {
        Vector3 rotation = Vector3.up * (IsOpen ? OPEN_ANGLE : 0);
        doorMesh.rotation = Quaternion.Euler(rotation);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sled"))
            OnSledTriggerEnter();
    }

    protected virtual void OnSledTriggerEnter() { }
}