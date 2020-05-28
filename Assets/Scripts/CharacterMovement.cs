using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private GameController parentController;
    public GameObject statter;
    public GameObject headSpike;
    public GameObject bodyLSpike;
    public GameObject bodyRSpike;


    public Subject sub;
    private float speed; //Stat 1, movement, affects energy consumption speed**2
    private int quality; //Stat 2, minimum food quality to aim after when possible
                         //Stat 3 is object scale, affects energy consumption scale**3

    private float oldSpeed;
    private float oldQuality;
    private float oldSize;

    private float speedBias;
    private float qualityBias;
    private float sizeBias;

    public float energy;
    public float startEnergy;
    public float foodCollected;
    public float foodNeeded;
    

    private Transform target;
    private bool active = true;

    //Separate statistics used as global variables in DetectFood to reduce code there
    public bool desperate;
    private GameObject occupierToScare;
    private int qualityThreshold;
    private GameObject goMin;
    private float minDist;
    public bool complex;

    //Separate animation handler to pass 
    private Animator animator;


    public float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    private void Awake()
    {
        sub = new Subject(this);
    }


    //Start is called when the object is instantiated
    void Start()
    {
        if (complex)
        {
            animator = GetComponent<Animator>();

            animator.SetBool("ShouldRun", speed < 5 ? false : true);
            animator.SetFloat("Speed", speed);
            animator.SetFloat("Energy", energy);
            //animator.SetFloat("WalkMultiplier", Remap(speed, 1f, 5f, 0.8f, 2f) / transform.localScale.x);
            animator.SetFloat("WalkMultiplier", Remap(speed, 1f, 5f, 0.8f, 2f) / (1 + (transform.localScale.x - 1) / Remap(speed, 1f, 5f, 1f, 3f)));
            animator.SetFloat("RunMultiplier", Remap(speed, 5f, 10f, 1.25f, 2.5f) / (1 + (transform.localScale.x - 1) / Remap(speed, 5f, 10f, 1f, 3f)));

            setSpikes();
        }
        
        

        parentController = GetComponentInParent<GameController>();
        foodCollected = 0;
        foodNeeded = 2;

        speedBias = 0f;
        qualityBias = 0f;
        sizeBias = 0f;

        oldSpeed = Mathf.Infinity;
        oldQuality = Mathf.Infinity;
        oldSize = Mathf.Infinity;
}

    /*
        Update is called once per frame, it ensures that the player is always
        moving towards it's target when it still needs food, and if not done,
        search for new food to continue moving towards
    */
    void Update()
    {
        if(active)
        {
            //Check to see if character has no food to go after
            if(target == null)
            {
                //No food or enough food consumed, end
                if (parentController.foodList.Count == 0 || foodCollected >= foodNeeded)
                {
                    StopCharacter();
                }
                //There is food to get, find it
                else
                    DetectFood(false);
            }

            //There is a target to currently move towards
            if(target != null)
            {
                //Assign movement speed
                float step = Time.deltaTime * speed;

                //Assign target to move towards and Rotate the "forward" vector towards the target direction
                Vector3 targetDirection = new Vector3(target.position.x, transform.position.y, target.position.z) - new Vector3(transform.position.x, transform.position.y, transform.position.z);
                Vector3 rotDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360f, 0.0f);

                if (Vector3.Distance(gameObject.transform.position, target.transform.position) <= transform.localScale.x/2)
                {

                    foodCollected += 0.2f * target.GetComponent<Food>().GetQuality();
                    energy += 100f * target.GetComponent<Food>().GetQuality();

                    parentController.foodList.Remove(target.GetComponent<Food>().GetID());
                    Destroy(target.gameObject);

                }
                    
                else
                    transform.position += targetDirection.normalized * step;//transform.forward * step;

                transform.rotation = Quaternion.LookRotation(rotDirection);

                energy -= Mathf.Pow(speed, 2f) * Mathf.Pow(transform.localScale.x, 3);

                //If energy runs out here or has gotten enough food, send observation notification
                if (energy <= 0)
                {
                    StopCharacter();
                }
            }
        }
    }



    public void StopCharacter()
    {
        sub.Notify();
        active = false;
        energy = 0;
        if(complex)
            animator.SetFloat("Energy", 0);

        if (target != null)
        {
            target.GetComponent<Food>().RemoveOccupier();
            target = null;
            parentController.GetComponent<GameController>().DetectFood();
        }    
    }

    public void ResetEnergy()
    {
        energy = startEnergy;
        foodCollected = 0;
        active = true;
        target = null;

        if (complex)
        {
            animator = GetComponent<Animator>();

            animator.SetBool("ShouldRun", speed < 5 ? false : true);
            animator.SetFloat("Speed", speed);
            animator.SetFloat("Energy", energy);
            animator.SetFloat("WalkMultiplier", Remap(speed, 1f, 5f, 0.8f, 2f) / (1 + (transform.localScale.x - 1) / Remap(speed, 1f, 5f, 1f, 3f)));
            animator.SetFloat("RunMultiplier", Remap(speed, 5f, 10f, 1.25f, 2.5f) / (1 + (transform.localScale.x - 1) / Remap(speed, 5f, 10f, 1f, 3f)));

        }
    }

    /*
    This method is used to detect food and pick the best choice based on animal stats.
    */
    public void DetectFood(bool scared)
    {

        if(active == true)
        {
            desperate = false;
            occupierToScare = null;
            qualityThreshold = 1;

            //Since another animal scared this one, remove only target, the other has assigned occupier
            if (scared == true)
                target = null;

            //If going for new food, make sure to make the old food available first
            if (target != null)
            {
                target.gameObject.GetComponent<Food>().RemoveOccupier();
                target = null;
            }

            //Keep track of closest food...
            goMin = null;
            minDist = Mathf.Infinity;

            GameObject goDesperate = null;
            float minDistDesperate = Mathf.Infinity;

            //... And current position
            Vector3 currentPos = transform.position;



            //Go through every food to determine best choice
            //foreach (GameObject go in parentController.foodList)
            foreach (KeyValuePair<int, GameObject> entry in parentController.foodList)
            {
                GameObject go = entry.Value;

                //Get distance between animal and food
                float dist = Vector3.Distance(currentPos, go.transform.position);

                Food food = go.GetComponent<Food>();
                GameObject foodOccupier = food.GetOccupier();
                int foodQuality = food.GetQuality();

                //Hack to break away, since for some reason, when the round restarts, existing characters see only 0 food quality
                if (foodQuality == 0)
                    return;

                //First barrier, determine if suitable quality

                //1. Larger or equal than quality, other conditions blocked off, check distance
                if (foodQuality >= quality)
                {
                    //Exception, check if better quality, if yes, overtake without checking distance
                    if (quality != qualityThreshold)
                    {
                        //Only check if occupied
                        if (foodOccupier == null)
                        {
                            UpdateClosest(foodOccupier, quality, go, dist);
                        }

                        else if (foodOccupier != null && gameObject.transform.localScale.x - foodOccupier.transform.localScale.x >= 0.15)
                        {
                            UpdateClosest(foodOccupier, foodQuality, go, dist);
                        }

                    }

                    //Quality and qualityThreshold same, meaning previous element was quality itself, compare distance, don't change qualitythreshold
                    else
                    {
                        //Check if shorter distance
                        if (dist < minDist)
                        {
                            //And finally, check if food not occupied or is but character larger
                            if (foodOccupier == null)
                            {
                                UpdateClosest(foodOccupier, 0, go, dist);
                            }

                            else if (foodOccupier != null && gameObject.transform.localScale.x - foodOccupier.transform.localScale.x >= 0.15)
                            {
                                UpdateClosest(foodOccupier, 0, go, dist);
                            }
                        }
                    }
                }

                //2. less than quality, but equal qualityThreshold, must check distance
                else if (foodQuality == qualityThreshold)
                {
                    //Check if shorter distance
                    if (dist < minDist)
                    {
                        //And finally, check if food not occupied or is but character larger
                        if (foodOccupier == null)
                        {
                            UpdateClosest(foodOccupier, 0, go, dist);
                        }


                        else if (foodOccupier != null && gameObject.transform.localScale.x - foodOccupier.transform.localScale.x >= 0.15)
                        {
                            UpdateClosest(foodOccupier, 0, go, dist);
                        }
                    }
                }

                //3. less than quality, but better qualityThreshold, do not check distance, since better food
                else if (foodQuality > qualityThreshold)
                {
                    //check if occupied or not but larger
                    if (foodOccupier == null)
                    {
                        UpdateClosest(foodOccupier, foodQuality, go, dist);
                    }


                    else if (foodOccupier != null && gameObject.transform.localScale.x - foodOccupier.transform.localScale.x >= 0.15)
                    {
                        UpdateClosest(foodOccupier, foodQuality, go, dist);
                    }

                }

                //Separate case for desperate mode to just check closest food
                if (dist < minDistDesperate)
                {
                    goDesperate = go;
                    minDistDesperate = dist;
                }
            }

            // pick closest food as target
            if (goMin != null)
            {
                target = goMin.transform;
                goMin.GetComponent<Food>().SetOccupier(this.gameObject);

                //Scare away character
                if (occupierToScare != null)
                    occupierToScare.GetComponent<CharacterMovement>().DetectFood(true);
            }
            else if (goDesperate != null)
            {
                //Do not reassign occupier; Depending on who grabs the food first. Due to the go becoming null, both animals should then reuse DetectFood
                target = goDesperate.transform;
                desperate = true;
            }
        }
    }

    //Used in DetectFood to update goMin and minDist related variables whenever conditions met
    private void UpdateClosest(GameObject toScare, int newQualThreshold, GameObject min, float dMin)
    {
        occupierToScare = toScare;
        if (newQualThreshold != 0)
            qualityThreshold = newQualThreshold;

        goMin = min;
        minDist = dMin;
    }

    public void updateBiases()
    {
        //only survived, biases halved
        if(foodCollected >= foodNeeded/2 && foodCollected < foodNeeded)
        {
            qualityBias /= 2;
            sizeBias /= 2;
            speedBias /= 2;
        }

        //reproduced, therefore good stats
        //Note that if the bias becomes too large, the animal can only evolve in one direction
        //Consider this as specialized overtuning that causes extinction
        else if(foodCollected >= foodNeeded && oldQuality != Mathf.Infinity && oldSize != Mathf.Infinity && oldSpeed != Mathf.Infinity)
        {
            float speedDif = speed - oldSpeed;
            float qualityDif = quality - oldQuality;
            float sizeDif = transform.localScale.x - oldSize;

            speedBias += speedDif >= 0 ? 0.1f : -0.1f;
            sizeBias += sizeDif >= 0 ? 0.04f : -0.04f;

            if (qualityDif >= 1)
                qualityBias += 0.01f;
            else if (qualityDif <= -1)
                qualityBias -= 0.01f; 
        }

        

    }

    public void Mutate()
    {
        oldSpeed = speed;
        oldQuality = quality;
        oldSize = transform.localScale.x;

        float speedChange = Random.Range(-0.5f + speedBias, 0.5f + speedBias);
        float sizeChange = Random.Range(-0.2f + sizeBias, 0.2f + sizeBias);
        float qualityChange = Random.Range(0f, 1f) + qualityBias;

        speed += speedChange;
        if (speed < 1f) speed = 1f;
        if (speed > 10f) speed = 10f;

        //Change scale based on size changes
        transform.localScale += new Vector3(sizeChange, sizeChange, sizeChange);
        if (transform.localScale.x < 1) transform.localScale = new Vector3(1f,1f,1f);
        if (transform.localScale.x > 4) transform.localScale = new Vector3(4f, 4f, 4f);

        //Also move spikes based on size
        if(complex)
            setSpikes();

        if (qualityChange < 0.1f && quality > 1) quality -= 1;
        else if (qualityChange > 0.9f && quality < 5) quality += 1;

        ChangeColor(false);

        statter.GetComponent<CharStat>().Realign(gameObject, complex);
    }

    public void ChangeColor(bool first)
    {
        if (complex == false)
            GetComponent<MeshRenderer>().materials[0].color = new Color(
                                                                        0.1f  * speed,
                                                                        0.25f * transform.localScale.x,
                                                                        0.2f  * quality,
                                                                        1f);
        else
        {
            gameObject.transform.Find("Dinosaur").GetComponent<SkinnedMeshRenderer>().materials[1].color = new Color(
                                                                        0.1f * speed,
                                                                        0.25f * transform.localScale.x,
                                                                        0.2f * quality,
                                                                        1f);
            if (first)
            {
                gameObject.transform.Find("Dinosaur").GetComponent<SkinnedMeshRenderer>().materials[0].color = Random.ColorHSV(0f, 1f, 0.7f, 1.0f);
            }
        }
    }

    private void setSpikes()
    {
        bodyLSpike.transform.localPosition = new Vector3(bodyLSpike.transform.localPosition.x,
                                                        bodyLSpike.transform.localPosition.y,
                                                        Remap(transform.localScale.x, 1f, 4f, 0.00045f, -0.00175f));
        bodyRSpike.transform.localPosition = new Vector3(bodyRSpike.transform.localPosition.x,
                                                        bodyRSpike.transform.localPosition.y,
                                                        Remap(transform.localScale.x, 1f, 4f, 0.00045f, -0.00175f));

        headSpike.transform.localPosition = new Vector3(headSpike.transform.localPosition.x,
                                                        headSpike.transform.localPosition.y,
                                                        Remap(transform.localScale.x, 1f, 4f, -0.0005f, -0.00221f));
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
