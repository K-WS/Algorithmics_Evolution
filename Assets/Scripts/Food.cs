using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    private GameController parentController;
    public int quality;
    private GameObject occupier;

    void Start()
    {
        parentController = GetComponentInParent<GameController>();
        quality = Random.Range(1,6);
        occupier = null;

        /*this.GetComponent<MeshRenderer>().materials[0].color = new Color(
                                                                        0.8f - 0.15f * (quality-1),
                                                                        0.62f + 0.02f * (quality-1),
                                                                        0.016f + 0.08f * (quality-1), 
                                                                        1f);*/
        if (quality == 1)
            this.GetComponent<MeshRenderer>().materials[0].color = new Color(
                                                                            0.8f,
                                                                            0f,
                                                                            0f,
                                                                            1);
        else if (quality == 2)
            this.GetComponent<MeshRenderer>().materials[0].color = new Color(
                                                                            0.8f,
                                                                            0.36f,
                                                                            0.13f,
                                                                            1);
        else if (quality == 3)
            this.GetComponent<MeshRenderer>().materials[0].color = new Color(
                                                                            0.8f,
                                                                            0.72f,
                                                                            0.13f,
                                                                            1);
        else if (quality == 4)
            this.GetComponent<MeshRenderer>().materials[0].color = new Color(
                                                                            0.4f,
                                                                            0.8f,
                                                                            0.13f,
                                                                            1);
        else if (quality == 5)
            this.GetComponent<MeshRenderer>().materials[0].color = new Color(
                                                                            0.2f,
                                                                            0.8f,
                                                                            0.72f,
                                                                            1);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && parentController != null && other.gameObject == occupier)
        {
            other.GetComponentInParent<CharacterMovement>().foodCollected += 0.2f*quality;
            other.GetComponentInParent<CharacterMovement>().energy += 100f * quality;

            parentController.foodList.Remove(gameObject);
            Destroy(gameObject);
        }
        else if(other.gameObject != occupier)
        {
            Debug.Log("Not the right person to collide in");
        }
    }

    /*private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && parentController != null && other.gameObject == occupier)
        {
            other.GetComponentInParent<CharacterMovement>().foodCollected += 0.2f * quality;
            other.GetComponentInParent<CharacterMovement>().energy += 100f * quality;

            parentController.foodList.Remove(gameObject);
            Destroy(gameObject);
        }
    }*/

    public void RemoveOccupier()
    {
        occupier = null;
        //Debug.Log("What about the occupier?");
        //Debug.Log(occupier);
    }

    public void SetOccupier(GameObject go) 
    {
        occupier = go;
    }

    public GameObject GetOccupier()
    {
        return occupier;
    }
}
