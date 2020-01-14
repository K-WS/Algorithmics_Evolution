using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private GameController parentController;
    public GameObject statter; 

    public Subject sub;
    private float speed = 2f; //Stat 1, movement, affects energy consumption speed**2
    private int quality = 1;  //Stat 2, minimum food quality to aim after when possible
                              //Stat 3 is object scale, affects energy consumption scale**3
    public float energy;
    public float startEnergy;
    public float foodCollected;
    public float foodNeeded;

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
        //startEnergy = 4000f;
        //energy = 4000f;
        foodCollected = 0;
        foodNeeded = 2;
    }

    // Update is called once per frame
    void Update()
    {
        if(active)
        {
            //Check to see if character has no food to go after
            if(target == null)
            {
                //No food, end
                if (parentController.foodList.Count == 0)
                {
                    sub.Notify();
                    active = false;
                }
                //There is food, find it
                else 
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

                energy -= Mathf.Pow(speed, 2f) * Mathf.Pow(transform.localScale.x, 3);

                //If energy runs out here or has gotten enough food, send observation notification
                if (energy <= 0 || foodCollected >= foodNeeded)
                {
                    sub.Notify();
                    active = false;
                    target.GetComponent<Food>().occupier = null;

                    parentController.GetComponent<GameController>().DetectFood();

                }
            }
        }
    }

    public void ResetEnergy()
    {
        energy = startEnergy;
        foodCollected = 0;
        active = true;
        target = null;
    }

    /*
    This method is used to detect food and pick the best choice:
    1. General case, will always prefer a food that isn't picked, or scares the other character
       when it's large enough to go after some other food.
    2. If there isn't enough food to go for every character, will go for closest, scaring away
       others becomes ineffective when desperate for food.
    3. During the entire time, quality pays an important role. While the minimum quality threshold
       hasn't been met, both the normal and desperate case only save the new food as long as the
       current quality isn't worse.
       Otherwise, if the threshold is met, then it just saves any food over the threshold
    */
    public void DetectFood()
    {
        bool scare = false;
        int qualityThreshold = 1;
        int desperateThreshold = 1;


        //If going for new food, make sure to make the old food available first
        if (target != null)
            target.gameObject.GetComponent<Food>().occupier = null;

        //Find closest non-assinged food and assign it, if none found, take closest assigned
        GameObject goMin = null;
        float minDist = Mathf.Infinity;
        
        GameObject goForced = null;
        float minDistForced = Mathf.Infinity;

        Vector3 currentPos = transform.position;

        foreach (GameObject go in parentController.foodList)
        {

            //Get distance between object and food
            float dist = Vector3.Distance(go.transform.position, currentPos);

            Food food = go.GetComponent<Food>();

            //If food not occupied and quicker and minimum wanted quality, change goMin and minDist
            if (food.occupier == null && dist < minDist)
            {
                //Check Quality status
                //Forceful reset of qualityThreshold to not become stricter, if food good enough
                if (food.quality >= quality)
                {
                    qualityThreshold = quality;
                }
                //Actual check if the food is good enough
                if(food.quality >= qualityThreshold)
                {
                    qualityThreshold = food.quality;
                    goMin = go;
                    minDist = dist;
                    scare = false;
                }  
            }

            //If food is occupied, but character larger and minimum wanted quality, then change goMin and minDist and scare them
            else if (food.occupier != null &&
                    food.occupier.transform.localScale.x -
                    gameObject.transform.localScale.x >= 0.15 && 
                    dist < minDist)
            {

                //Check Quality status
                //Forceful reset of qualityThreshold to not become stricter, if food good enough
                if (food.quality >= quality)
                {
                    qualityThreshold = quality;
                }
                //Actual check if the food is good enough
                if (food.quality >= qualityThreshold)
                {
                    qualityThreshold = food.quality;
                    goMin = go;
                    minDist = dist;
                    scare = false;
                }
            }

            //If food is occupied and not larger, then add it to "last resort" quickest
            else if(go.GetComponent<Food>().occupier != null && dist < minDistForced)
            {
                //Check Desperate Quality status
                //Forceful reset of desperateThreshold to not become stricter, if food good enough
                if (food.quality >= quality)
                {
                    desperateThreshold = quality;
                }
                //Actual check if the food is good enough
                if (food.quality >= desperateThreshold)
                {
                    qualityThreshold = food.quality;
                    goForced = go;
                    minDistForced = dist;
                }
            }

        }

        //If no non-assigned food is available, pick forced food
        //Assinging occupier no longer matters in this case
        if (goMin == null && goForced != null)
        {
            target = goForced.transform;
        }

        //If goMin is not null, then assign it as true
        else if (goMin != null)
        {
            //If food is already occupied, then scare them away.
            if (scare == true)
                goMin.GetComponent<Food>().occupier.GetComponent<CharacterMovement>().DetectFood();

            goMin.GetComponent<Food>().occupier = this.gameObject;
            target = goMin.transform;
        }
    }

    public void Mutate()
    {
        float speedChange = Random.Range(-0.5f, 0.5f);
        float sizeChange = Random.Range(-0.2f, 0.2f);
        float qualityChange = Random.Range(0f, 1f);

        speed += speedChange;
        if (speed < 1f) speed = 1f;
        if (speed > 10f) speed = 10f;

        transform.localScale += new Vector3(sizeChange, sizeChange, sizeChange);
        if (transform.localScale.x < 1) transform.localScale = new Vector3(1f,1f,1f);
        if (transform.localScale.x > 4) transform.localScale = new Vector3(4f, 4f, 4f);

        if (qualityChange < 0.1f && quality > 1) quality -= 1;
        else if (qualityChange > 0.9f && quality < 5) quality += 1;

        ChangeColor();

        statter.GetComponent<CharStat>().Realign(gameObject);
    }

    public void ChangeColor()
    {
        GetComponent<MeshRenderer>().materials[0].color = new Color(
                                                                    0.1f * (speed),
                                                                    0.1f * (transform.localScale.x),
                                                                    quality * 0.2f,
                                                                    1f);
    }

    public void SetStats(float speed, float size, int quality, float energy)
    {
        this.speed = speed;
        transform.localScale = new Vector3(size, size, size);
        this.quality = quality;
        this.startEnergy = energy;
        this.energy = energy;
    }
}
