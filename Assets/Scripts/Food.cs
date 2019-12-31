using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    private GameController parentController;
    private int quality;
    //public bool occupied;
    public GameObject occupier;

    void Start()
    {
        parentController = GetComponentInParent<GameController>();
        quality = Random.Range(1,5);
        //occupied = false
        occupier = null;

        this.GetComponent<MeshRenderer>().materials[0].color = new Color(
                                                                        0.8f - 0.15f * (quality-1),
                                                                        0.62f + 0.02f * (quality-1),
                                                                        0.016f + 0.08f * (quality-1), 
                                                                        1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && parentController != null)
        {
            other.GetComponentInParent<CharacterMovement>().foodCollected += 1;
            other.GetComponentInParent<CharacterMovement>().energy += 100f * quality;
            //This should be a part of mutations, but testing size here
            other.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);

            parentController.foodList.Remove(gameObject);
            Destroy(gameObject);
        }
        
    }
}
