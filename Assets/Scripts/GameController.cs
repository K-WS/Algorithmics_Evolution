using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour, Observer
{
    //Food info
    public GameObject food;
    public List<GameObject> foodList = new List<GameObject>();
    private int foodCount = 40; //Starting food count, but used whenever new food created

    //Character related Prefabs
    public GameObject character;
    public GameObject stats;

    //Character info
    public List<GameObject> characterList = new List<GameObject>();
    private int characterCount = 100; //Starting character count, however, not reused again

    private float startingSpeed;
    private float startingSize;
    private int startingQuality;
    private float startingEnergy;


    //Life cycle info
    private int expendedChars;
    private bool reset;
    private float timeToReset;

    //World info
    private int radius = 30;
    public GameObject circleFloor;
    private bool init;

    //UI info
    public InputField characterCountText;
    public InputField foodCountText;
    public InputField StartEnergyText;
    public Slider startSpeedText;
    public Slider startSizeText;
    public Slider startQualityText;

    public Button startButton;
    public Button resetButton;

    public Image setupPanel;


    // Start is called before the first frame update
    void Start()
    {
        startButton.onClick.AddListener(StartClick);
        resetButton.onClick.AddListener(ResetClick);

        expendedChars = 0;
        reset = false;
        timeToReset = 0;
        init = false;

        circleFloor.transform.localScale = new Vector3(radius, 10f, radius);
        circleFloor.transform.position = new Vector3(0, -10f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        //Close application when escape pressed
        if (Input.GetKeyDown("escape"))
            Application.Quit();

        if (reset == true)
        {
            timeToReset += Time.deltaTime;
            if(timeToReset >= 1)
            {
                reset = false;
                timeToReset = 0;

                MakeFood(foodCount);
                if(init == false)
                {
                    SpawnCharacters(characterCount);
                    init = true;
                }
                else
                {
                    MutateCharacters();
                }
                
                
            }
        }
    }

    /*-------------------------------------------------------*/
    /*--------------UI Button related Functions--------------*/
    /*-------------------------------------------------------*/

    private void StartClick()
    {
        characterCount = int.Parse(characterCountText.text);
        foodCount = int.Parse(foodCountText.text);

        startingSpeed = startSpeedText.value;
        startingSize = startSizeText.value;
        startingQuality = (int) startQualityText.value;
        startingEnergy = int.Parse(StartEnergyText.text);

        reset = true;
        startButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(true);
        setupPanel.gameObject.SetActive(false);
    }

    private void ResetClick()
    {
        //Destroy all characters
        for (int i = characterList.Count - 1; i >= 0; i--)
        {
            GameObject go = characterList[i];
            DestroyCharacter(go, i);
        }

        //Destroy all food
        DestroyFood();

        //Ensure all time and spawn related stats are also reset
        expendedChars = 0;
        reset = false;
        timeToReset = 0;
        init = false;


        startButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(false);
        setupPanel.gameObject.SetActive(true);
    }


    /*-------------------------------------------------------*/
    /*--------------Character related Functions--------------*/
    /*-------------------------------------------------------*/

    //Method to create a new character
    private void CreateCharacter()
    {
        GameObject prefab = Instantiate(character);
        //float speed, float size, int quality, float energy
        prefab.GetComponent<CharacterMovement>().SetStats(startingSpeed, startingSize, startingQuality, startingEnergy);
        prefab.GetComponent<CharacterMovement>().sub.AddObserver(this);
        RepositionCharacter(prefab);

        prefab.transform.parent = this.transform;
        characterList.Add(prefab);

        //Although this should be called in the character itself, for init purposes, I want the statistics to work properly and hacked the first time in here.
        prefab.GetComponent<CharacterMovement>().ChangeColor();

        prefab.GetComponent<CharacterMovement>().statter = Instantiate(stats);
        prefab.GetComponent<CharacterMovement>().statter.GetComponent<CharStat>().Realign(prefab);
    }

    //Method to use a preset character to make a new character from
    private GameObject CreateCharacter(GameObject go)
    {
        GameObject newGO = Instantiate(go);
        newGO.GetComponent<CharacterMovement>().SetStats(startingSpeed, startingSize, startingQuality, startingEnergy);
        newGO.GetComponent<CharacterMovement>().sub.AddObserver(this);
        RepositionCharacter(newGO);

        newGO.transform.parent = this.transform;
        characterList.Add(newGO);

        newGO.GetComponent<CharacterMovement>().ChangeColor();

        GameObject statter = Instantiate(stats);
        newGO.GetComponent<CharacterMovement>().statter = statter;
        statter.GetComponent<CharStat>().Realign(newGO);

        return newGO;
    }

    //Method to destroy a character
    private void DestroyCharacter(GameObject go, int i)
    {
        go.GetComponent<CharacterMovement>().sub.removeObserver(this);
        characterList.RemoveAt(i);
        Destroy(go.GetComponent<CharacterMovement>().statter);
        Destroy(go);
    }


    //Method used in start to create the first characters
    private void SpawnCharacters(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            CreateCharacter();
        }
    }

    //Method that visits all characters food collection and mutates/reproduces/kills based on results
    //Make sure that all characters after mutations are then properly relocated
    private void MutateCharacters()
    {
        for (int i = characterList.Count - 1; i >= 0; i--)
        {
            GameObject go = characterList[i];

            //Not enough food
            if(go.GetComponent<CharacterMovement>().foodCollected < go.GetComponent<CharacterMovement>().foodNeeded/2)
            {
                DestroyCharacter(go, i);
            }

            //Best case scenario, enough food to reproduce
            else if(go.GetComponent<CharacterMovement>().foodCollected >= go.GetComponent<CharacterMovement>().foodNeeded)
            {
                RepositionCharacter(go);

                GameObject newGO = CreateCharacter(go);

                go.GetComponent<CharacterMovement>().updateBiases();
                newGO.GetComponent<CharacterMovement>().updateBiases();

                go.GetComponent<CharacterMovement>().Mutate();
                newGO.GetComponent<CharacterMovement>().Mutate();

                go.GetComponent<CharacterMovement>().ResetEnergy();
                newGO.GetComponent<CharacterMovement>().ResetEnergy();
            }

            //Midcase, enough food to survive
            else
            {
                RepositionCharacter(go);
                go.GetComponent<CharacterMovement>().ResetEnergy();
            }

        }
    }

    private void RepositionCharacter(GameObject go)
    {
        float radian = Random.Range(0, 2 * Mathf.PI);

        float sin = Mathf.Sin(radian) * (radius / 2 - 0.5f);
        float cos = Mathf.Cos(radian) * (radius / 2 - 0.5f);

        go.transform.position = new Vector3(sin, 0.5f, cos);

    }

    //Separate Observer when a character runs out of energy
    public void SubjectUpdate(object sender)
    {
        expendedChars += 1;

        //Start resetting process
        if (expendedChars == characterList.Count)
        {
            expendedChars = 0;
            reset = true;
        }
    }

    /*-------------------------------------------------------*/
    /*----------------Food related Functions-----------------*/
    /*-------------------------------------------------------*/

    //Method to clear and respawn as much food as is needed.
    private void MakeFood(int amount)
    {
        DestroyFood();
        AddFood(amount);
    }

    //Method to delete all food currently on the field
    private void DestroyFood()
    {
        foreach (GameObject go in foodList)
            Destroy(go);  //Destroy leftover food gameobjects
        foodList.Clear(); //Empties all leftover food references in the list.
    }

    //Method that, instead of resetting and creating food, only adds a smaller amount
    private void AddFood(int amount)
    {
        //Add the actual food
        for (int i = 0; i < amount; i++)
        {
            Vector2 area = Random.insideUnitCircle * (radius / 2 - 2f);
            GameObject prefab = Instantiate(food,
                                            new Vector3(area.x, 0.5f, area.y),
                                            Quaternion.identity);
            prefab.transform.parent = this.transform;
            foodList.Add(prefab);
        }
        //DetectFood();
    }

    public void DetectFood()
    {
        //Notify all players that new food has been created
        foreach (GameObject character in characterList)
        {
            character.GetComponent<CharacterMovement>().DetectFood(false);
        }
    }

}
