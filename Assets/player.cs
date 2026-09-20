using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    public PlayerMovement playerMovement;
    public float health = 100f;
    public float maxHealth = 100f;
    public Slider healthSlider;
    public CameraController cameraController;
    public GameObject swordObject;
    public GameObject swordSwingEffectPrefab;
    public float attackRange = 2.5f;
    public float attackDamage = 25f;
    public float attackCooldown = 0.6f;
    public float defenseReduction = 0.5f;
    public float dialogueRange = 3.5f;
    public float healOnPickup = 20f;
    public float maxHealthRestore = 100f;
    public GameObject helmets;
    public GameObject chestplates;
    public GameObject armArmor;
    public GameObject belts;
    public GameObject legArmor;
    public GameObject shoes;
    public TMP_Dropdown helmetsDropdown;
    public TMP_Dropdown chestplatesDropdown;
    public TMP_Dropdown armArmorDropdown;
    public TMP_Dropdown legArmorDropdown;
    public TMP_Dropdown beltsDropdown;
    public TMP_Dropdown shoesDropdown;
    private Transform initialTransform;
    private static Player instance;
    private float attackTimer;
    private bool isDefending;
    public static Player Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Player>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("Player");
                    instance = obj.AddComponent<Player>();
                }
            }
            return instance;
        }
    }
    public List<GameObject> helmetslist = new List<GameObject>();
    public List<GameObject> chestplateslist = new List<GameObject>();
    public List<GameObject> armArmorList = new List<GameObject>();
    public List<GameObject> legArmorList = new List<GameObject>();
    public List<GameObject> beltsList = new List<GameObject>();
    public List<GameObject> shoesList = new List<GameObject>();

    void Start()
    {
        instance=this;






        initialTransform=transform;
        playerMovement.enabled=false;
        cameraController.enabled=false;
        transform.Rotate(Vector3.up, -90f);

        SetupEquipmentDropdown(helmetsDropdown, helmets, helmetslist, OnHelmetDropdown);
        SetupEquipmentDropdown(chestplatesDropdown, chestplates, chestplateslist, OnChestplateDropdown);
        SetupEquipmentDropdown(armArmorDropdown, armArmor, armArmorList, OnArmArmorDropdown);
        SetupEquipmentDropdown(legArmorDropdown, legArmor, legArmorList, OnLegArmorDropdown);
        SetupEquipmentDropdown(beltsDropdown, belts, beltsList, OnBeltDropdown);
        SetupEquipmentDropdown(shoesDropdown, shoes, shoesList, OnShoeDropdown);

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }
    }

    private void SetupEquipmentDropdown(TMP_Dropdown dropdown, GameObject parent, List<GameObject> itemList, UnityEngine.Events.UnityAction<int> callback)
    {
        itemList.Clear();
        List<string> optionNames = new List<string>();

        if (parent != null)
        {
            foreach (Transform child in parent.transform)
            {
                if (child == null || child.gameObject == null)
                    continue;

                itemList.Add(child.gameObject);
                optionNames.Add(child.gameObject.name);
                child.gameObject.SetActive(false);
            }
        }

        if (dropdown != null)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(optionNames);
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener(callback);
        }
    }

    private void SetSelectedEquipment(GameObject parent, List<GameObject> itemList, int index)
    {
        if (parent == null || index < 0 || index >= itemList.Count)
            return;

        parent.SetActive(true);

        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] != null)
            {
                itemList[i].SetActive(i == index);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        if (HasSword())
        {
            if (Input.GetButtonDown("Fire1") && attackTimer <= 0f)
            {
                PerformAttack();
            }

            isDefending = Input.GetButton("Fire2");

            if (swordObject != null)
            {
                Animator swordAnimator = swordObject.GetComponentInChildren<Animator>();
                if (swordAnimator != null)
                {
                    swordAnimator.SetBool("Defending", isDefending);
                }
            }
        }
        else
        {
            isDefending = false;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryStartDialogue();
        }

        if (healthSlider != null)
        {
            healthSlider.value = health;
        }
    }

    private void TryStartDialogue()
    {
        Vector3 interactionOrigin = transform.position + transform.forward * 1.2f;
        Collider[] hits = Physics.OverlapSphere(interactionOrigin, dialogueRange);

        foreach (Collider hit in hits)
        {
            EnemyBehavior enemy = hit.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                enemy.StartDialogue();
                break;
            }
        }
    }

    public void EquipSword(GameObject sword)
    {
        swordObject = sword;
        if (swordObject != null)
        {
            swordObject.SetActive(true);
        }

        if (healthSlider != null)
        {
            healthSlider.value = health;
        }
    }

    public bool HasSword()
    {
        return swordObject != null && swordObject.activeInHierarchy;
    }

    public void RestoreHealth(float amount)
    {
        if (amount <= 0f)
            return;

        health = Mathf.Min(maxHealth, health + amount);
        if (healthSlider != null)
        {
            healthSlider.value = health;
        }
    }

    private void PerformAttack()
    {
        if (!HasSword())
            return;

        attackTimer = attackCooldown;

        Animator swordAnimator = swordObject != null ? swordObject.GetComponentInChildren<Animator>() : null;
        if (swordAnimator != null)
        {
            swordAnimator.SetTrigger("Swing");
        }

        Vector3 attackOrigin = transform.position + transform.forward * 1.2f;
        Collider[] hits = Physics.OverlapSphere(attackOrigin, attackRange);

        foreach (Collider hit in hits)
        {
            EnemyBehavior enemy = hit.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                enemy.ReactToHit(hit.transform.position);
                SpawnHitEffect(hit.transform.position);
            }
        }
    }

    private void SpawnHitEffect(Vector3 position)
    {
        if (swordSwingEffectPrefab != null)
        {
            Instantiate(swordSwingEffectPrefab, position, Quaternion.identity);
            return;
        }

        GameObject hitEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hitEffect.transform.position = position;
        hitEffect.transform.localScale = Vector3.one * 0.15f;
        hitEffect.GetComponent<Renderer>().material.color = Color.yellow;
        Collider hitCollider = hitEffect.GetComponent<Collider>();
        if (hitCollider != null)
        {
            hitCollider.enabled = false;
        }
        Destroy(hitEffect, 0.15f);
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
            return;

        if (isDefending)
        {
            damage *= 1f - defenseReduction;
        }

        health -= damage;
        health = Mathf.Clamp(health, 0f, maxHealth);

        if (healthSlider != null)
        {
            healthSlider.value = health;
        }

        if (health <= 0f)
        {
            OnDeath();
        }
        else
        {
            Debug.Log("Player health: " + health);
        }
    }

    private void OnDeath()
    {
        Debug.Log("Player has died from fall damage.");

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (cameraController != null)
            cameraController.enabled = false;
    }

    public void OnCreatorExit()
    {
        playerMovement.enabled=true;
        cameraController.enabled=true;
        transform.position=initialTransform.position;
        transform.rotation=initialTransform.rotation;
    }
   public void OnHelmetToggleChanged(Toggle toggle)
    {
        if (helmets != null)
        {
            helmets.SetActive(toggle.isOn);

            if (!toggle.isOn)
            {
                foreach (GameObject helmet in helmetslist)
                {
                    if (helmet != null)
                        helmet.SetActive(false);
                }
            }
        }
    }
    public void OnChestplateToggleChanged(Toggle toggle)
    {
        chestplates.SetActive(toggle.isOn);
    }
    public void OnArmToggleChanged(Toggle toggle)
    {
        armArmor.SetActive(toggle.isOn);
    }
    public void OnLegArmorToggleChanged(Toggle toggle)
    {
        legArmor.SetActive(toggle.isOn);
    }
    public void OnBeltToggleChanged(Toggle toggle)
    {
        belts.SetActive(toggle.isOn);
    }
    public void OnShoeToggleChanged(Toggle toggle)
    {
        shoes.SetActive(toggle.isOn);
    }
    public void OnHelmetDropdown(int index)
    {
        if (helmetsDropdown != null)
        {
            index = helmetsDropdown.value;
        }

        SetSelectedEquipment(helmets, helmetslist, index);
    }

    public void OnChestplateDropdown(int index)
    {
        if (chestplatesDropdown != null)
        {
            index = chestplatesDropdown.value;
        }

        SetSelectedEquipment(chestplates, chestplateslist, index);
    }

    public void OnArmArmorDropdown(int index)
    {
        if (armArmorDropdown != null)
        {
            index = armArmorDropdown.value;
        }

        SetSelectedEquipment(armArmor, armArmorList, index);
    }

    public void OnLegArmorDropdown(int index)
    {
        if (legArmorDropdown != null)
        {
            index = legArmorDropdown.value;
        }

        SetSelectedEquipment(legArmor, legArmorList, index);
    }

    public void OnBeltDropdown(int index)
    {
        if (beltsDropdown != null)
        {
            index = beltsDropdown.value;
        }

        SetSelectedEquipment(belts, beltsList, index);
    }

    public void OnShoeDropdown(int index)
    {
        if (shoesDropdown != null)
        {
            index = shoesDropdown.value;
        }

        SetSelectedEquipment(shoes, shoesList, index);
    }




}