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
    public void Realign(GameObject go, bool complex)
    {

        
        Color color;
        
        if(complex == false)
            color = go.GetComponent<MeshRenderer>().materials[0].color;
        else
        {
            /*gameObject.transform.Find("Dinosaur").GetComponent<SkinnedMeshRenderer>().materials[1].color = new Color(
                                                                        0.1f * speed,
                                                                        0.25f * transform.localScale.x,
                                                                        0.2f * quality,
                                                                        1f);*/
            color = go.transform.Find("Dinosaur").GetComponent<SkinnedMeshRenderer>().materials[1].color;
        }
        GetComponent<MeshRenderer>().materials[0].color = color;

        Vector3 pLoc = transform.parent.position;
        gameObject.transform.position = new Vector3(pLoc.x + color[0] * 20,
                                                    pLoc.y + color[1] * 20,
                                                    pLoc.z + color[2] * 20);
    }

    //Reference to destroy this gameobject when the owner is also destroyed
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
