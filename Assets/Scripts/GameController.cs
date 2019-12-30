using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour, Observer
{
    //Food info
    public GameObject food;
    public List<GameObject> foodList = new List<GameObject>();
    private int foodCount = 20; //Starting food count, but used whenever new food created

    //Character info
    public GameObject character;
    public List<GameObject> characterList = new List<GameObject>();
    private int characterCount = 5; //Starting character count, however, not reused again

    //Life cycle info
    private int expendedChars;
    private bool reset;
    private float timeToReset;

    //World info
    private int radius = 30;
    public GameObject circleFloor;

    private float foodTimer;
    private float foodRespawn;
    


    // Start is called before the first frame update
    void Start()
    {
        //Eventually, instead of just using it in start, special update method calls as well.
        MakeFood(foodCount);
        SpawnCharacters(characterCount);

        expendedChars = 0;
        reset = false;
        timeToReset = 0;
        foodTimer = 0;
        foodRespawn = Random.Range(2f, 5f);

        circleFloor.transform.localScale = new Vector3(radius, 0.0001f, radius);
    }

    // Update is called once per frame
    void Update()
    {
        foodTimer += Time.deltaTime;
        if(foodTimer >= foodRespawn)
        {
            foodRespawn = Random.Range(2f, 5f);
            foodTimer = 0;

            AddFood(3);

            
        }

        if(reset == true)
        {
            timeToReset += Time.deltaTime;
            if(timeToReset >= 3)
            {
                reset = false;
                timeToReset = 0;

                MakeFood(foodCount);
                MutateCharacters();
                
            }
        }
    }

    //Method to clear and respawn as much food as is needed.
    private void MakeFood(int amount)
    {
        foreach (GameObject go in foodList)
            Destroy(go);  //Destroy leftover food gameobjects
        foodList.Clear(); //Empties all leftover food references in the list.

        AddFood(amount);
    }

    //Method that, instead of resetting and creating food, only adds a smaller amount
    private void AddFood(int amount)
    {
        //Add the actual food
        for (int i = 0; i < amount; i++)
        {
            Vector2 area = Random.insideUnitCircle * (radius / 2 - 1f);
            GameObject prefab = Instantiate(food,
                                            new Vector3(area.x, 0.5f, area.y),
                                            Quaternion.identity);
            prefab.transform.parent = this.transform;
            foodList.Add(prefab);
        }

        //Notify all players that new food has been created
        foreach(GameObject character in characterList)
        {
            character.GetComponent<CharacterMovement>().DetectFood();
        }
    }

    //Method used in start to create the first characters
    private void SpawnCharacters(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject prefab = Instantiate(character);
            prefab.GetComponent<CharacterMovement>().sub.AddObserver(this);
            RepositionCharacter(prefab);

            prefab.transform.parent = this.transform;
            characterList.Add(prefab);
        }
    }

    //Method that visits all characters food collection and mutates/reproduces/kills based on results
    //Make sure that all characters after mutations are then properly relocated
    private void MutateCharacters()
    {
        /*
         TODO
         Go through CharacterList, analyse how much food they got.
         Based on that:
         1. Not enough food, Destroy them and remove them from the list of characters
         2. Enough food, Modify their stats? "Reproduce" and instantiate a new character into the list
         The new character should also be modified based on the results of the reproducee

        Make sure to assign the newly created object this as parent when adding to list:

        prefab.transform.parent = this.transform;
        characterList.Add(prefab);
        prefab.GetComponent<CharacterMovement>().sub.AddObserver(this);

        If removing, make sure to first remove the observer and character from list before destroying it.

        And reset the amount of food they collected
         */




        /*GameObject prefab = Instantiate(character);
        prefab.GetComponent<CharacterMovement>().sub.AddObserver(this);
        RepositionCharacter(prefab);

        prefab.transform.parent = this.transform;
        characterList.Add(prefab);*/


        //Relocate all characters currently in the list
        //This can be changed later to be part of the larger TODO
        foreach (GameObject go in characterList)
        {
            RepositionCharacter(go);
            go.GetComponent<CharacterMovement>().ResetEnergy();
        }
    }

    private void RepositionCharacter(GameObject go)
    {
        int xMult = (Random.Range(-1f, 1f) < 0) ? -1 : 1;
        int zMult = (Random.Range(-1f, 1f) < 0) ? -1 : 1;

        //If 0, pick x as the range, otherwise pick z as the range
        if (Random.Range(0, 1) == 0)
            go.transform.position = new Vector3(Random.Range(0f, 9.5f) * xMult, 0.5f, 9.5f * zMult);
        else
            go.transform.position = new Vector3(9.5f * xMult, 0.5f, Random.Range(0f, 9.5f) * zMult);

    }

    public void SubjectUpdate(object sender)
    {
        
        expendedChars += 1;
        //Debug.Log(expendedChars);
        if (expendedChars == characterList.Count)
        {
            expendedChars = 0;
            //Start resetting process
            //Debug.Log("Starting reset");
            reset = true;
        }
    }
}
