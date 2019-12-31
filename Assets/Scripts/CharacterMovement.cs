using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private GameController parentController;

    //public GameObject character;
    public Subject sub;
    private float speed = 2f;
    public float energy;
    public float startEnergy;
    public int foodCollected = 0;

    public Transform target;
    private bool active = true;

    private float waitTime = 0.1f;

    private void Awake()
    {
        sub = new Subject(this);
    }


    //Start is called when the object is instantiated
    void Start()
    {
        parentController = GetComponentInParent<GameController>();
        startEnergy = 2000f;
        energy = 2000f;
    }

    // Update is called once per frame
    void Update()
    {
        //Forced waiting timer to make all characters and food to properly spawn without causing crashes
        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
        }
        else if(active)
        {
            //Check to see if character has no food to go after
            if(target == null)
            {
                DetectFood();
            }

            //Error prevention check if there isn't enough food to go for everyone
            if(target != null)
            {
                //Assign movement speed
                float step = Time.deltaTime * speed;

                //Assign target to move towards
                Vector3 targetDirection = target.position - transform.position;
                //Rotate the "forward" vector towards the target direction by one step?
                Vector3 rotDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360f, 0.0f);

                transform.position += targetDirection.normalized * step;//transform.forward * step;
                transform.rotation = Quaternion.LookRotation(rotDirection);

                energy -= speed * transform.localScale.x;

                //If energy runs out here, send observation notification
                if (energy <= 0 || parentController.foodList.Count == 0)
                {
                    sub.Notify();
                    active = false;
                }
            }
        }
    }

    public void ResetEnergy()
    {
        energy = startEnergy;
        foodCollected = 0;
        active = true;
    }

    public void DetectFood()
    {
        //If new food is spawned, make sure to make the old food available first
        if(target != null)
        {
            //target.gameObject.GetComponent<Food>().occupied = false;
            target.gameObject.GetComponent<Food>().occupier = this.gameObject;
        }

        //Find closest food and assign it, if none found, is null
        GameObject goMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject go in parentController.foodList)
        {
            //General check to see if the food hasn't been occupied
            //Or if it is and the player is much larger than them
            //if(go.GetComponent<Food>().occupied == false)
            if(go.GetComponent<Food>().occupier == null ||
                go.GetComponent<Food>().occupier != null &&
                go.GetComponent<Food>().occupier.transform.localScale.x -
                    gameObject.transform.localScale.x >= 0.5)
            {
                float dist = Vector3.Distance(go.transform.position, currentPos);
                if (dist < minDist)
                {
                    goMin = go;
                    minDist = dist;
                }
            }
            
        }

        if(goMin != null)
        {
            //If food is already occupied, then scare them away.
            if (goMin.GetComponent<Food>().occupier != null)
                goMin.GetComponent<Food>().occupier.GetComponent<CharacterMovement>().DetectFood();

            //goMin.GetComponent<Food>().occupied = true;
            goMin.GetComponent<Food>().occupier = this.gameObject;
            target = goMin.transform;
        }
    }
}
