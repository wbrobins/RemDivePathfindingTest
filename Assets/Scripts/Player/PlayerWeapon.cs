using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    private bool shooting = false;
    [SerializeField] private new GameObject camera;
    [SerializeField] private GameObject playerHandObj;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    private PlayerHand playerHand;
    [SerializeField] private GameObject player;
    [SerializeField] private int currWeaponSlot = 0;
    [SerializeField] private LayerMask shootRaycastMask = ~0;
    [SerializeField] private BulletHoleSpawner bulletHoleSpawner;

    [Header("Weapon Bob")]
    [Tooltip("Transform to sway. Should NOT be driven by the Animator (see note below)")]
    [SerializeField] private Transform bobTarget;
    [SerializeField] private float bobAmount = 0.03f;
    [Tooltip("Bob cycles per meter traveled, so faster movement = faster bob")]
    [SerializeField] private float bobFrequency = 1.2f;
    [SerializeField] private float bobSmoothing = 12f;
    [Tooltip("Speed at which bob reaches full amplitude")]
    [SerializeField] private float bobFullSpeed = 10f;
    [SerializeField] private float swayRoll = 2f;  
    [SerializeField] private float swayYaw = 1f;      
    [SerializeField] private float swayDip = 0.4f;

    [Header("Air Drift")]
    [SerializeField] private float airDriftAmount = 0.04f;    //max vertical offset in meters
    [SerializeField] private float airDriftMaxSpeed = 10f;    //vertical speed at which drift maxes out
    [SerializeField] private float airDriftSmoothing = 8f;
    private float airDriftOffset;

    [Header("Slide Tilt")]
    [SerializeField] private float slidePitch = 25f;         //degrees the weapon tips upward
    [SerializeField] private float slideTiltSmoothing = 10f;
    private float slidePitchOffset;

    private float bobPhase;
    private Vector3 bobBasePos;
    private Quaternion bobBaseRot;
    private Vector3 bobPosOffset;
    private Vector3 bobRotOffset;

    public bool Shooting => shooting;
    public Weapon[] weapons;
    public Weapon CurrWeapon {get; private set;}
    private GameObject currWeaponPrefab;
    private PlayerController playerController;

    [Header("Footsteps")]
    [Tooltip("Separate from the weapon's AudioSource, since that one is used for the fire sound")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float footstepVolume = 0.6f;
    [SerializeField] private float crouchVolumeMultiplier = 0.4f;
    [SerializeField] private Vector2 footstepPitchRange = new Vector2(0.92f, 1.08f);

    private int lastStepIndex = -1;
    private int lastClipIndex = -1;
    private bool wasMoving;

    void Awake()
    {
        playerHand = playerHandObj.GetComponent<PlayerHand>();

        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogError("PlayerWeapon: no weapons assigned.", this);
            enabled = false;
            return;
        }
        SetUp();
        SetWeapon(currWeaponSlot);
    }

    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        playerController.UpdateAmmoText(CurrWeapon);   
        if (bobTarget != null)
        {
            bobBasePos = bobTarget.localPosition;
            bobBaseRot = bobTarget.localRotation;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !shooting && !playerHand.Carrying && Cursor.lockState == CursorLockMode.Locked)
        {
            StartCoroutine(ShootRoutine());
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && !shooting)
        {
            SetWeapon(0);
        } 
        else if (Input.GetKeyDown(KeyCode.Alpha2) && !shooting)
        {
            SetWeapon(1);
        } 
        else if (Input.GetKeyDown(KeyCode.F) && !shooting)
        {
            CycleWeapon();
        } 
        else if (Input.GetKeyDown(KeyCode.R) && !shooting && CurrWeapon.CurrBarrelAmmo != CurrWeapon.Base.MaxBarrelAmmo)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    void LateUpdate()
    {
        if (bobTarget == null || playerController == null) return;

        MovementState state = playerController.CurrState;
        bool groundMoving = playerController.Grounded && (state == MovementState.Walking || state == MovementState.Sprinting || state == MovementState.Crouching);

        float speed = playerController.HorizontalSpeed;
        bool moving = groundMoving && speed > 0.5f;
        bool canBob = moving && !shooting; //shooting covers fire + reload

        //restart the cycle when you start moving, so the first step isn't delayed
        if (moving && !wasMoving)
        {
            bobPhase = 0f;
            lastStepIndex = -1;
        }
        wasMoving = moving;

        //phase advances whenever you're moving, so footsteps keep going while firing/reloading
        if (moving)
        {
            float cycleSpeed = Mathf.Lerp(speed, bobFullSpeed, 0.5f);
            bobPhase += cycleSpeed * bobFrequency * Time.deltaTime * Mathf.PI * 2f;
            CheckFootstep(state);
        }

        Vector3 targetPos = Vector3.zero;
        Vector3 targetRot = Vector3.zero;

        if (canBob)
        {
            float amp = Mathf.Clamp01(speed / bobFullSpeed);
            float s = Mathf.Sin(bobPhase);
            float lag = Mathf.Sin(bobPhase - 0.6f); //rotation trails position for the pendulum feel

            targetPos = new Vector3(s * bobAmount, Mathf.Cos(bobPhase * 2f) * bobAmount * swayDip, 0f) * amp;

            targetRot = new Vector3(0f, lag * swayYaw, -lag * swayRoll) * amp;
        }

        float t = Time.deltaTime * bobSmoothing;
        bobPosOffset = Vector3.Lerp(bobPosOffset, targetPos, t);
        bobRotOffset = Vector3.Lerp(bobRotOffset, targetRot, t);

        //rising pushes the weapon down, falling lifts it up
        float targetDrift = 0f;
        if (!playerController.Grounded)
        {
            float vy = Mathf.Clamp(playerController.VerticalSpeed / airDriftMaxSpeed, -1f, 1f);
            targetDrift = -vy * airDriftAmount;
        }
        airDriftOffset = Mathf.Lerp(airDriftOffset, targetDrift, Time.deltaTime * airDriftSmoothing);

        bool sliding = state == MovementState.Sliding;
        float targetPitch = sliding ? -slidePitch : 0f; //negative X pitches up
        float tiltRate = sliding ? slideTiltSmoothing : slideTiltSmoothing * 1.5f;
        slidePitchOffset = Mathf.Lerp(slidePitchOffset, targetPitch, Time.deltaTime * tiltRate);

        bobTarget.localPosition = bobBasePos + bobPosOffset + Vector3.up * airDriftOffset;
        bobTarget.localRotation = bobBaseRot * Quaternion.Euler(bobRotOffset + new Vector3(slidePitchOffset, 0f, 0f));
    }

    void SetUp()
    {
        foreach(Weapon weapon in weapons)
        {
            weapon.SetShootDelay(weapon.Base.ShootDelay);
            weapon.SetDamage(weapon.Base.Damage);
            weapon.SetAmmo(weapon.Base.MaxBarrelAmmo, weapon.Base.MaxTotalAmmo);
            weapon.SetSpread(weapon.Base.PelletsPerShot, weapon.Base.SpreadAngle);
        }
    }

    void CycleWeapon()
    {
        switch (currWeaponSlot)
        {
            case 0:
                currWeaponPrefab.SetActive(false);
                SetWeapon(1);
                break;
            case 1:
                currWeaponPrefab.SetActive(false);
                SetWeapon(0);
                break;
            default:
                Debug.Log("Weapon error: wrong value passed @ PlayerWeapon.cs");
                return;
        }

        playerController.UpdateAmmoText(CurrWeapon);   
    }

    void SetWeapon(int slot)
    {
        if(CurrWeapon != null)
        {
            Destroy(currWeaponPrefab);
        }
        CurrWeapon = weapons[slot];
        currWeaponSlot = slot;
        Debug.Log("Current Weapon: " + CurrWeapon.Base.Name);
        currWeaponPrefab = Instantiate(CurrWeapon.Base.BasePrefab, transform.position, camera.transform.rotation, transform);
        currWeaponPrefab.SetActive(false);
        SetLayerRecursive(currWeaponPrefab, LayerMask.NameToLayer("Weapon"));
        if(playerController != null) 
        {
            playerController.UpdateAmmoText(CurrWeapon);     
        }
        audioSource.resource = CurrWeapon.Base.FireSound;
        StartCoroutine(SwapDelay());
    }

    IEnumerator SwapDelay()
    {
        animator.SetTrigger("Swap");
        yield return new WaitForSeconds(.1f);
        currWeaponPrefab.SetActive(true);
    }

    void Reload()
    {
        Debug.Log("Reloading...");
        if(CurrWeapon.TotalAmmo >= CurrWeapon.Base.MaxBarrelAmmo)
        {
            CurrWeapon.SetAmmo(CurrWeapon.Base.MaxBarrelAmmo, CurrWeapon.TotalAmmo - (CurrWeapon.Base.MaxBarrelAmmo - CurrWeapon.CurrBarrelAmmo));
        }
        else if(CurrWeapon.TotalAmmo < CurrWeapon.Base.MaxBarrelAmmo)
        {
            CurrWeapon.SetAmmo(CurrWeapon.TotalAmmo, 0);
        }  
    }

    void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    void CheckFootstep(MovementState state)
    {
        //one step each time the phase passes the far left/right of the swing
        int stepIndex = Mathf.FloorToInt((bobPhase - Mathf.PI * 0.5f) / Mathf.PI);
        if (stepIndex == lastStepIndex) return;
        lastStepIndex = stepIndex;

        PlayFootstep(state);
    }

    void PlayFootstep(MovementState state)
    {
        if (footstepSource == null || footstepClips == null || footstepClips.Length == 0) return;

        //random clip, never the same one twice in a row
        int index = Random.Range(0, footstepClips.Length);
        if (footstepClips.Length > 1 && index == lastClipIndex)
        {
            index = (index + 1) % footstepClips.Length;
        }
        lastClipIndex = index;

        float volume = footstepVolume * (state == MovementState.Crouching ? crouchVolumeMultiplier : 1f);
        footstepSource.pitch = Random.Range(footstepPitchRange.x, footstepPitchRange.y);
        footstepSource.PlayOneShot(footstepClips[index], volume);
    }

    IEnumerator ShootRoutine()
    {
        //shoot
        if (CurrWeapon.CurrBarrelAmmo > 0)
        {
            shooting = true;

            Vector3 origin = camera.transform.position;
            Vector3 forward = camera.transform.forward;

            animator.SetTrigger("Fire");
            audioSource.Play();

            //collect hits first so one shot = one damage application per enemy, and each explosive only blows up once.
            var enemyDamage = new Dictionary<EnemyBehavior, int>();
            var explosives = new HashSet<CarryableBehavior>();

            for (int i = 0; i < CurrWeapon.PelletsPerShot; i++)
            {
                Vector3 direction = CurrWeapon.GetSpreadDirection(forward);
                Debug.DrawRay(origin, direction * 50f, Color.red, 1f);

                if (Physics.Raycast(origin, direction, out RaycastHit hit, 50f, shootRaycastMask))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        EnemyBehavior enemy = hit.collider.GetComponent<EnemyBehavior>();
                        if (enemy != null)
                        {
                            enemyDamage.TryGetValue(enemy, out int total);
                            enemyDamage[enemy] = total + CurrWeapon.Damage;
                        }
                    }
                    else
                    {
                        if (hit.collider.TryGetComponent(out CarryableBehavior explosive) && explosive.IsExplodable)
                        {
                            explosives.Add(explosive);
                        }
                        else
                        {
                            bulletHoleSpawner.Spawn(CurrWeapon.Base.BulletHolePrefab, hit);
                        }
                    }
                }
            }

            foreach (var kvp in enemyDamage)
            {
                kvp.Key.TakeDamage(kvp.Value, gameObject);
                kvp.Key.Engage(player.transform, true);
            }

            foreach (var explosive in explosives)
            {
                explosive.Explode();
            }

            CurrWeapon.DepleteAmmo(1);
            playerController.UpdateAmmoText(CurrWeapon);
            yield return new WaitForSeconds(CurrWeapon.ShootDelay);
            shooting = false;
        }
        else
        {
            Debug.Log("Out of ammo");
            playerController.UpdateMiddleAlertText("Out of ammo. Press R to reload.");
        } 
    }

    IEnumerator ReloadRoutine()
    {
        shooting = true;
        playerController.UpdateMiddleAlertText("");
        animator.SetTrigger("Reload");
        playerController.UpdateAmmoText("Reloading...");  
        yield return new WaitForSeconds(1);
        Reload();
        shooting = false;
        playerController.UpdateAmmoText(CurrWeapon);  
    }
}
