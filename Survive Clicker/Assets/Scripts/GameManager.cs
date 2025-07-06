using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SeasonType
{
    Summer,
    Autumn,
    Winter,
    Spring
}
[System.Serializable]
public class SeasonEffect
{
    public SeasonType season;
    public float foodProductionMultiplier = 1f;
    public float woodProductionMultiplier = 1f;
    public float stoneProductionMultiplier = 1f;
    public float foodGatheringMultiplier = 1f;
    public float ironProductionMultiplier = 1f;
    public float toolsProductionMultiplier = 1f;
}

public class PopulationImageState
{
    public RectTransform rectTransform;
    public Vector2 targetPosition;
    public float currentMoveTime; // Timer for when to pick a new target
}
public class GameManager : MonoBehaviour
{
    //population, wood, gold, food, stone, iron, tools
    [SerializeField] private int days; //x
    [SerializeField] private int winPopulation = 100; //x

    [Header("Resources")]
    [SerializeField] private int workers; //x
    [SerializeField] private int unemployed; //x
    [SerializeField] private int wood; //x
    [SerializeField] private int food; //x
    [SerializeField] private int stone; //x
    [SerializeField] private int iron; //x
    [SerializeField] private int tools; //x

    //farm, house, ironMines, goldMines, woodcutter, blacksmith, quarry,
    [Space(10)]
    [Header("Buildings")]
    [SerializeField] private int house; //1 house takes 4 people
    [SerializeField] private int farm; //x
    [SerializeField] private int woodcutter; //x
    [SerializeField] private int blacksmith; //x
    [SerializeField] private int quarry; //x
    [SerializeField] private int ironMines; //x

    [Space(10)]
    [Header("Resources Texts")]
    [SerializeField] private TMP_Text daysText;
    [SerializeField] private TMP_Text populationText;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text foodText;
    [SerializeField] private TMP_Text ironText;
    [SerializeField] private TMP_Text stoneText;
    [SerializeField] private TMP_Text toolsText;

    [Space(10)]
    [Header("Buildings Texts & Tool Efficiency")]
    [SerializeField] private TMP_Text houseText;
    [SerializeField] private TMP_Text farmText;
    [SerializeField] private TMP_Text woodcutterText;
    [SerializeField] private TMP_Text quarryText;
    [SerializeField] private TMP_Text ironMinesText;
    [SerializeField] private TMP_Text blacksmithText;
    [SerializeField] private TMP_Text notifiactionText;
    [SerializeField] private float baseToolEfficiency = 0.05f;

    private const int InitialDays = 0;
    private const int InitialWorkers = 0; 
    private const int InitialUnemployed = 10; 
    private const int InitialWood = 15;
    private const int InitialFood = 100;
    private const int InitialStone = 10;
    private const int InitialIron = 0;
    private const int InitialTools = 0;

    private const int InitialHouse = 3; 
    private const int InitialFarm = 0;
    private const int InitialWoodcutter = 0;
    private const int InitialBlacksmith = 0;
    private const int InitialQuarry = 0;
    private const int InitialIronMines = 0;

    private const int InitialBackground = 0; 
    private const bool InitialIsGameRunning = false;
    private const bool InitialIsLosingPeople = false;


    [Space(10)]
    [Header("Season references")]
    [SerializeField] List<string> currentSeason;
    [SerializeField] Image gameBackground;
    [SerializeField] List<Color> gameColors;
    [SerializeField] float seasonTimeToPass = 180f;
    [SerializeField] private TMP_Text seasonText;
    [SerializeField] private TMP_Text seasonTimeText;
    [SerializeField] private List<SeasonEffect> seasonEffects;

    [Space(10)]
    [Header("Population UI Images")]
    [SerializeField] private Transform populationImageContainer;
    [SerializeField] private GameObject populationImagePrefab;
    [SerializeField] private float populationMoveSpeed = 1f;
    [SerializeField] private float populationChangeTargetInterval = 2f;
    [SerializeField] private float populationPadding = 0f;
    private List<PopulationImageState> currentPopulationImages = new List<PopulationImageState>();

    private float timer;
    private int currentBackground;
    private bool isGameRunning;
    private bool isLosingPeople;

    private SeasonEffect currentSeasonEffect;

    private void Awake()
    {
        currentBackground = 0;
    }
    private void Update()
    {
        //time scales
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Time.timeScale = 0;
            Debug.Log($"X0 speed");
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale = 1;
            Debug.Log($"X1 speed");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = 2.5f;
            Debug.Log($"X2.5 speed");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Time.timeScale = 5;
            Debug.Log($"X5 SPEED");
        }

        TimeOfDay();
        UpdatePopulationImageMovement();
    }

    /// <summary>
    /// mijenja dane
    /// </summary>
    private void TimeOfDay()
    {
        if (!isGameRunning)
        {
            return;
        }
        timer += Time.deltaTime;
        if (timer >= 60)
        {
            days++;
            FoodGathering();
            FoodProduction();
            WoodProduction();
            StoneProduction();
            IronProduction();
            ToolsProduction();
            FoodConsumption();
            IncreasePopulation();

            if (Population() >= winPopulation)
            {
                WinCondition();
            }

            UpdateText();
            UpdatePopulationImages();

            timer = 0;
        }
    }

    //changes seasons
    IEnumerator ChangeSeasons()
    {
        while (true)
        {
            Color startColor = gameBackground.color;
            currentBackground = (currentBackground + 1) % gameColors.Count;
            Color targetColor = gameColors[currentBackground];

            UpdateSeasonState();

            float tick = 0f;
            while (tick < seasonTimeToPass)
            {
                tick += Time.deltaTime;
                float progress = tick / seasonTimeToPass;
                gameBackground.color = Color.Lerp(startColor, targetColor, progress);
                seasonTimeText.text = $"Next season in {(int)(seasonTimeToPass - tick)} seconds";
                yield return null;
            }
            gameBackground.color = targetColor;
        }
    }


    /// <summary>
    /// current season
    /// </summary>
    private void UpdateSeasonState()
    {
        SeasonType newSeasonType = (SeasonType)(currentBackground % System.Enum.GetValues(typeof(SeasonType)).Length);

        currentSeasonEffect = seasonEffects.Find(effect => effect.season == newSeasonType);

        if (currentSeasonEffect == null)
        {
            Debug.Log("WTF assignaj efekte godisnjih doba u inspektoru");
            currentSeasonEffect = new SeasonEffect { season = newSeasonType, foodProductionMultiplier = 1f, woodProductionMultiplier = 1f, stoneProductionMultiplier = 1f, foodGatheringMultiplier = 1f };
        }
        seasonText.text = $"Season: {currentSeasonEffect.season.ToString()}";
    }

    public void InitializeGame()
    {
        isGameRunning = true;
        UpdateText();
        StartCoroutine(ChangeSeasons());
    }
    /// <summary>
    /// Consume food
    /// </summary>
    private void FoodConsumption()
    {
        string text = $"You lost food for {Population()} people";
        StartCoroutine(NotificationText(text));
        food -= Population();

        if (food <= 0)
        {
            food = 0;
            isLosingPeople = true;
            StartCoroutine(NotificationText($"You are losing people!"));
            if (unemployed > 0)
            {
                unemployed--;
            }
            else if (workers > 0)
            {
                workers--;
            }

            if (unemployed < 0) unemployed = 0;
            if (workers < 0) workers = 0;

            if (Population() <= 0)
            {
                GameOver();
            }
            
        }
    }

    /// <summary>
    /// ako imamo alate oni povecavaju produktivnost
    /// </summary>
    /// <returns></returns>
    private float ToolProductionMultiplier()
    {
        return 1f + (tools * baseToolEfficiency);
    }
    /// <summary>
    /// Gather food
    /// </summary>
    private void FoodGathering()
    {
        float toolEfficiency = ToolProductionMultiplier();
        food += (int)(unemployed / 2 * currentSeasonEffect.foodGatheringMultiplier * toolEfficiency);
    }
    /// <summary>
    /// produce food
    /// </summary>
    private void FoodProduction()
    {
        float toolEfficiency = ToolProductionMultiplier();
        food += (int)(farm * 4 * currentSeasonEffect.foodProductionMultiplier * toolEfficiency);
    }
    private void StoneProduction()
    {
        float toolEfficiency = ToolProductionMultiplier();
        stone += (int)(quarry * 4 * currentSeasonEffect.stoneProductionMultiplier * toolEfficiency);
    }
    /// <summary>
    /// proizvodnja drva
    /// </summary>
    private void WoodProduction()
    {
        float toolEfficiency = ToolProductionMultiplier();
        wood += (int)(woodcutter * 2 * currentSeasonEffect.woodProductionMultiplier * toolEfficiency);
    }
    private void IronProduction()
    {
        float toolEfficiency = ToolProductionMultiplier();
        iron += (int)(ironMines * 2 * currentSeasonEffect.ironProductionMultiplier * toolEfficiency);
    }
    private void ToolsProduction()
    {
        int ironNeeded = 3;
        if (iron > ironNeeded)
        {
            iron-= ironNeeded;
            tools += (int)(blacksmith * 1 * currentSeasonEffect.toolsProductionMultiplier);
        }
    }
    /// <summary>
    /// number of max residents * 4
    /// </summary>
    /// <returns></returns>
    private int GetMaxPopulation()
    {
        int maxPopulation = house * 4;
        return maxPopulation;
    }

    /// <summary>
    /// increase
    /// </summary>
    private void IncreasePopulation()
    {
        if(days % 2 == 0 && isLosingPeople == false)
        {
            if (Population() < GetMaxPopulation())
            {
                unemployed += house;
                if(GetMaxPopulation() < Population())
                {
                    unemployed = GetMaxPopulation() - workers;
                }
            }
        }
    }

    /// <summary>
    /// ukupna populacija
    /// </summary>
    /// <returns></returns>
    private int Population()
    {
        return workers + unemployed;
    }
    /// <summary>
    /// stvaranje populacije na ekranu
    /// </summary>
    private void UpdatePopulationImages()
    {
        foreach (PopulationImageState state in currentPopulationImages)
        {
            Destroy(state.rectTransform.gameObject);
        }
        currentPopulationImages.Clear();

        int currentPopulation = Population();
        for (int i = 0; i < currentPopulation; i++)
        {
            if (populationImagePrefab != null && populationImageContainer != null)
            {
                GameObject newImageGameObject = Instantiate(populationImagePrefab, populationImageContainer);
                RectTransform newImageRect = newImageGameObject.GetComponent<RectTransform>();

                PopulationImageState newState = new PopulationImageState { rectTransform = newImageRect, currentMoveTime = 0f};
                SetNewTargetPosition(newState);
                currentPopulationImages.Add(newState);
            }
            else
            {
                Debug.Log("Population Image prefab or container is not assigned in gameManager.");
                break;
            }
        }
    }
    /// <summary>
    /// uzimanje radnika od neradnika
    /// </summary>
    /// <param name="amount"></param>
    private void WorkerAssign(int amount)
    {
        if (CanAssignWorker(amount))
        {
            unemployed -= amount;
            workers += amount;
            UpdateText();
        }
        else StartCoroutine(NotificationText($"Cannot assign {amount} workers"));
    }
    /// <summary>
    /// je li moguce zaposliti se
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    private bool CanAssignWorker(int amount)
    {
        return unemployed >= amount;
    }
    /// <summary>
    /// izgradi farmu
    /// </summary>
    public void BuildFarm()
    {
        Build(5, 0, 2, "Farm", ref farm);
        UpdateText();
    }
    /// <summary>
    /// izgradi woodcuttera
    /// </summary>
    public void BuildWoodcutter()
    {
        Build(5, 1, 2, "Woodcutter's Lodge", ref woodcutter);
        UpdateText();
    }
    /// <summary>
    /// izgradi house
    /// </summary>
    public void BuildHouse()
    {
        Build(2, 0, 0, "House", ref house);
        UpdateText();
    }
    /// <summary>
    /// izgradi quarry
    /// </summary>
    public void BuildQuarry()
    {
        Build(5, 2, 2, "Quarry", ref quarry);
        UpdateText();
    }
    public void BuildIronMines()
    {
        Build(5, 10, 2, "Iron Mines", ref ironMines);
        UpdateText();
    }

    /// <summary>
    /// izgradi blacksmitha
    /// </summary>
    public void BuildBlacksmith()
    {
        Build(10, 10, 1, "Blacksmith", ref blacksmith);
        UpdateText();
    }

    /// <summary>
    /// updates text
    /// </summary>
    private void UpdateText()
    {
        //resources
        populationText.text = $"Population: { Population()}/{GetMaxPopulation()}\n   Workers: {workers}\n   Unemployed: {unemployed}";
        daysText.text = days.ToString();
        woodText.text = $"Wood: {wood}";
        foodText.text = $"Food: {food}";
        ironText.text = $"Iron: {iron}";
        stoneText.text = $"Stone: {stone}";
        toolsText.text = $"Tools: {tools}";

        //buildings
        houseText.text = $"HOUSES: {house}";
        farmText.text = $"FARMS: {farm}";
        woodcutterText.text = $"WOODCUTTERS: {woodcutter}";
        quarryText.text = $"QUARRIES: {quarry}";
        ironMinesText.text = $"IRON MINES: {ironMines}";
        blacksmithText.text = $"BLACKSMITHS: {blacksmith}";
    }

    IEnumerator NotificationText(string text)
    {
        notifiactionText.text = text;
        yield return new WaitForSeconds(2);
        notifiactionText.text = string.Empty;
    }

    /// <summary>
    /// univerzalna gradnja gradevina
    /// </summary>
    /// <param name="woodCost"></param>
    /// <param name="stoneCost"></param>
    /// <param name="workerAssign"></param>
    /// <param name="name"></param>
    /// <param name="buildingCount"></param>
    private void Build(int woodCost, int stoneCost, int workerAssign, string name, ref int buildingCount)
    {
        if (wood >= woodCost && stone >= stoneCost && unemployed >= workerAssign)
        {
            wood -= woodCost;
            stone -= stoneCost;
            WorkerAssign(workerAssign);
            buildingCount++;
            string text = $"You successfully built a {name}";
            StartCoroutine(NotificationText(text));
        }
        else
        {
            string text = $"You need ";
            List<string> missing = new List<string>();
            if (wood < woodCost)
            {
                missing.Add($"{woodCost - wood} more wood");
            }
            if (stone < stoneCost)
            {
                missing.Add($"{stoneCost - stone} more stone");
            }
            if (unemployed < workerAssign)
            {
                missing.Add($"{workerAssign - unemployed} more unemployed workers");
            }

            text += string.Join(", ", missing);
            StartCoroutine(NotificationText(text));
        }

    }


    /// <summary>
    /// kretanje slike po ekranu
    /// </summary>
    private void UpdatePopulationImageMovement()
    {
        if (populationImageContainer == null) return;

        RectTransform parentRect = populationImageContainer.GetComponent<RectTransform>();
        if (parentRect == null) return;

        foreach (PopulationImageState state in currentPopulationImages)
        {
            if (state.rectTransform == null) continue;

            state.currentMoveTime += Time.deltaTime;

            if (state.currentMoveTime >= populationChangeTargetInterval)
            {
                SetNewTargetPosition(state);
                state.currentMoveTime = 0f;
            }

            state.rectTransform.localPosition = Vector2.Lerp(state.rectTransform.localPosition, state.targetPosition, Time.deltaTime * populationMoveSpeed);
        }
    }
    /// <summary>
    /// postavljanje random pozicije na koju ce slika ici
    /// </summary>
    /// <param name="imageState"></param>
    private void SetNewTargetPosition(PopulationImageState imageState)
    {
        if (populationImageContainer == null || imageState.rectTransform == null) return;

        RectTransform parentRect = populationImageContainer.GetComponent<RectTransform>();
        if (parentRect == null) return;

        float width = imageState.rectTransform.rect.width;
        float height = imageState.rectTransform.rect.height;

        float minX = parentRect.rect.xMin + width / 2f + populationPadding;
        float maxX = parentRect.rect.xMax - width / 2f - populationPadding;
        float minY = parentRect.rect.yMin + height / 2f + populationPadding;
        float maxY = parentRect.rect.yMax - height / 2f - populationPadding;

        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        imageState.targetPosition = new Vector2(randomX, randomY);
    }

    private void GameOver()
    {
        StopAllCoroutines();
        Menu.instance.Defeated();
    }
    private void WinCondition()
    {
        StopAllCoroutines();
        Menu.instance.Win();
    }

    public void ResetGame()
    {
        StopAllCoroutines();

        days = InitialDays;
        workers = InitialWorkers;
        unemployed = InitialUnemployed;
        wood = InitialWood;
        food = InitialFood;
        stone = InitialStone;
        iron = InitialIron;
        tools = InitialTools;

        house = InitialHouse;
        farm = InitialFarm;
        woodcutter = InitialWoodcutter;
        blacksmith = InitialBlacksmith;
        quarry = InitialQuarry;
        ironMines = InitialIronMines;

        isGameRunning = InitialIsGameRunning;
        isLosingPeople = InitialIsLosingPeople;
        timer = 0f;

        currentBackground = InitialBackground;
        gameBackground.color = gameColors[currentBackground];
        UpdateSeasonState();

        UpdatePopulationImages();

        UpdateText();

        Debug.Log($"Reset to start values");

        
    }


}
