using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    private GameController parentController;
    private int quality;
    private GameObject occupier;

    void Start()
    {
        parentController = GetComponentInParent<GameController>();
        quality = Random.Range(1,6);
        occupier = null;

        switch (quality)
        {
            case 1:
                this.GetComponent<MeshRenderer>().materials[0].color = new Color(
                                                                            0.8f,
                                                                            0f,
                                                                            0f,
                                                                            1);
                break;
            case 2:
                this.GetComponent<MeshRenderer>().materials[0].color = new Color(
                                                                            0.8f,
                                                                            0.36f,
                                                                            0.13f,
                                                                            1);
                break;
            case 3:
                this.GetComponent<MeshRenderer>().materials[0].color = new Color(
                                                                            0.8f,
                                                                            0.72f,
                                                                            0.13f,
                                                                            1);
                break;
            case 4:
                this.GetComponent<MeshRenderer>().materials[0].color = new Color(
                                                                            0.4f,
                                                                            0.8f,
                                                                            0.13f,
                                                                            1);
                break;
            case 5:
                this.GetComponent<MeshRenderer>().materials[0].color = new Color(
                                                                            0.2f,
                                                                            0.8f,
                                                                            0.72f,
                                                                            1);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        //Both regular case and desperate case assigned here
        if (other.gameObject.CompareTag("Player") && parentController != null && (other.gameObject == occupier || other.GetComponent<CharacterMovement>().desperate == true))
        {
            other.GetComponentInParent<CharacterMovement>().foodCollected += 0.2f*quality;
            other.GetComponentInParent<CharacterMovement>().energy += 100f * quality;

            parentController.foodList.Remove(gameObject);
            Destroy(gameObject);
        }
    }

    public void RemoveOccupier()
    {
        occupier = null;
    }

    public void SetOccupier(GameObject go) 
    {
        occupier = go;
    }

    public GameObject GetOccupier()
    {
        return occupier;
    }

    public int GetQuality()
    {
        return quality;
    }
}
