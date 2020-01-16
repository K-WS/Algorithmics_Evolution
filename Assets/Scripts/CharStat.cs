using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharStat : MonoBehaviour
{

    public void Awake()
    {
        gameObject.transform.parent = GameObject.Find("Graph").transform;
    }

    //Use owner data to recolor and reposition to new location on graph
    public void Realign(GameObject go)
    {
        GetComponent<MeshRenderer>().materials[0].color = go.GetComponent<MeshRenderer>().materials[0].color;

        Vector3 pLoc = transform.parent.position;
        gameObject.transform.position = new Vector3(pLoc.x + GetComponent<MeshRenderer>().materials[0].color[0] * 20,
                                                    pLoc.y + GetComponent<MeshRenderer>().materials[0].color[1] * 20,
                                                    pLoc.z + GetComponent<MeshRenderer>().materials[0].color[2] * 20);
    }

    //Reference to destroy this gameobject when the owner is also destroyed
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
