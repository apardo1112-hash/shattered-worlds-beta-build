using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordOnGround : MonoBehaviour
{
    public GameObject swordOnHand;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            gameObject.SetActive(false);

            if (swordOnHand != null)
            {
                swordOnHand.SetActive(true);
            }

            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                player.EquipSword(swordOnHand);
            }
        }
    }
}
