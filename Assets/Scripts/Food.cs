using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    private GameController parentController;
    private int quality;
    private GameObject occupier;

    private int ID; //For the Dictionary system

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
    public void SetID(int num)
    {
        ID = num;
    }

    public int GetID()
    {
        return ID;
    }
}
