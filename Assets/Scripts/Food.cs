using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    private GameController parentController;

    void Start()
    {
        parentController = GetComponentInParent<GameController>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponentInParent<CharacterMovement>().foodCollected += 1;
            parentController.foodList.Remove(gameObject);
            Destroy(gameObject);
        }
        
    }
}
