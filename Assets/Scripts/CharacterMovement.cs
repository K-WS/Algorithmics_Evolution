using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private GameController parentController;

    //public GameObject character;
    public Subject sub;
    private float speed = 2f;
    private float energy = 2000f;
    private float startEnergy = 2000f;
    public int foodCollected = 0;

    public Transform target;
    private bool active = true;

    private void Awake()
    {
        sub = new Subject(this);
    }


    //Start is called when the object is instantiated
    void Start()
    {
        parentController = GetComponentInParent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {

        /*if(foodCollected >= 2)
        {
            energy = 0;
            sub.Notify();
        }*/

        if (active) 
        {
            

            if (parentController.foodList.Count != 0)
            {
                //Assign movement speed
                float step = Time.deltaTime * speed;

                //Assign target to move towards
                Vector3 targetDirection = new Vector3(0, 0, 0);

                GameObject goMin = null;
                float minDist = Mathf.Infinity;
                Vector3 currentPos = transform.position;

                foreach (GameObject go in parentController.foodList)
                {
                    float dist = Vector3.Distance(go.transform.position, currentPos);
                    if (dist < minDist)
                    {
                        goMin = go;
                        minDist = dist;
                    }
                }

                target = goMin.transform;
                targetDirection = target.position - transform.position;


                //Rotate the "forward" vector towards the target direction by one step?
                Vector3 rotDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360f, 0.0f);

                transform.position += targetDirection.normalized * step;//transform.forward * step;
                transform.rotation = Quaternion.LookRotation(rotDirection);

                
            }
            energy -= 1 * speed;

            //If energy runs out here, send observation notification
            if (energy <= 0 || foodCollected >= 2 || parentController.foodList.Count == 0)
            {
                sub.Notify();
                active = false;
            }
                
        }
            
    }

    public void ResetEnergy()
    {
        energy = startEnergy;
        foodCollected = 0;
        active = true;
    }
}
