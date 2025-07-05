
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
}

public class PopulationImageState
{
    public RectTransform rectTransform;
    public Vector2 targetPosition;
    public float currentMoveTime; // Timer for when to pick a new target
}
public class GameManager : MonoBehaviour
{
    //population, wood, gold, food, stone, iron, tools, 
    [SerializeField] private int days; //x

    [Header("Resources")]
    [SerializeField] private int workers; //x
    [SerializeField] private int unemployed; //x
    [SerializeField] private int wood; //x
    [SerializeField] private int gold;
    [SerializeField] private int food; //x
    [SerializeField] private int stone; //x
    [SerializeField] private int iron; //x
    [SerializeField] private int tools;

    //farm, house, ironMines, goldMines, woodcutter, blacksmith, quarry,
    [Space(10)]
    [Header("Buildings")]
    [SerializeField] private int house; //1 house takes 4 people
    [SerializeField] private int farm; //x
    [SerializeField] private int woodcutter; //x
    [SerializeField] private int blacksmith; //x
    [SerializeField] private int quarry; //x
    [SerializeField] private int ironMines;

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
    [Header("Buildings Texts")]
    [SerializeField] private TMP_Text houseText;
    [SerializeField] private TMP_Text farmText;
    [SerializeField] private TMP_Text woodcutterText;
    [SerializeField] private TMP_Text quarryText;
    [SerializeField] private TMP_Text blacksmithText;
    [SerializeField] private TMP_Text notifiactionText;

    [Space(10)]
    [Header("Season references")]
    [SerializeField] List<string> currentSeason;
    [SerializeField] Image gameBackground;
    [SerializeField] List<Color> gameColors;
    [SerializeField] float seasonTimeToPass = 180f;
    [SerializeField] private TMP_Text seasonText;
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
    private Coroutine seasons;

    private SeasonEffect currentSeasonEffect;

    private void Awake()
    {
        currentBackground = 0;
        UpdateText();
        UpdateSeasonState();
        UpdatePopulationImages();
        seasons = StartCoroutine(ChangeSeasons());
    }
    private void Update()
    {
        //time scales
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Time.timeScale = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = 2.5f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Time.timeScale = 5;
            Debug.Log("Speed!");
        }
        //one minute is one day
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
            FoodConsumption();
            IncreasePopulation();

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
    }
    /// <summary>
    /// Consume food
    /// </summary>
    private void FoodConsumption()
    {
        food -= Population();

        if (food < 0)
        {
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
    /// Gather food
    /// </summary>
    private void FoodGathering()
    {
        food += (int)(unemployed / 2 * currentSeasonEffect.foodGatheringMultiplier);
    }
    /// <summary>
    /// produce food
    /// </summary>
    private void FoodProduction()
    {
        food += (int)(farm * 4 * currentSeasonEffect.foodProductionMultiplier);
    }
    private void StoneProduction()
    {
        stone += (int)(quarry * 4 * currentSeasonEffect.stoneProductionMultiplier);
    }
    /// <summary>
    /// proizvodnja drva
    /// </summary>
    private void WoodProduction()
    {
        wood += (int)(woodcutter * 2 * currentSeasonEffect.woodProductionMultiplier);
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
        if(days % 2 == 0)
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
                GameObject newImageGO = Instantiate(populationImagePrefab, populationImageContainer);
                RectTransform newImageRect = newImageGO.GetComponent<RectTransform>();

                PopulationImageState newState = new PopulationImageState { rectTransform = newImageRect, currentMoveTime = 0f};
                SetNewTargetPosition(newState);
                currentPopulationImages.Add(newState);
            }
            else
            {
                Debug.LogWarning("Population Image Prefab or Container is not assigned in GameManager.");
                break;
            }
        }
    }

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
    }
    /// <summary>
    /// izgradi woodcuttera
    /// </summary>
    public void BuildWoodcutter()
    {
        Build(5, 1, 2, "Woodcutter's Lodge", ref woodcutter);
    }
    /// <summary>
    /// izgradi house
    /// </summary>
    public void BuildHouse()
    {
        Build(2, 0, 0, "House", ref house);
    }
    /// <summary>
    /// izgradi quarry
    /// </summary>
    public void BuildQuarry()
    {
        Build(5, 2, 2, "Quarry", ref quarry);
    }
    /// <summary>
    /// izgradi blacksmitha
    /// </summary>
    public void BuildBlacksmith()
    {
        Build(10, 10, 1, "Blacksmith", ref blacksmith);
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
        blacksmithText.text = $"BLACKSMITHS: {blacksmith}";
    }

    IEnumerator NotificationText(string text)
    {
        notifiactionText.text = text;
        yield return new WaitForSeconds(2);
        notifiactionText.text = string.Empty;
    }

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
    /// kretanje slike po ekranu(nisam znao kako pa sam koristio ai)
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

    private void SetNewTargetPosition(PopulationImageState imageState)
    {
        if (populationImageContainer == null || imageState.rectTransform == null) return;

        RectTransform parentRect = populationImageContainer.GetComponent<RectTransform>();
        if (parentRect == null) return;

        float myWidth = imageState.rectTransform.rect.width;
        float myHeight = imageState.rectTransform.rect.height;

        float minX = parentRect.rect.xMin + myWidth / 2f + populationPadding;
        float maxX = parentRect.rect.xMax - myWidth / 2f - populationPadding;
        float minY = parentRect.rect.yMin + myHeight / 2f + populationPadding;
        float maxY = parentRect.rect.yMax - myHeight / 2f - populationPadding;

        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        imageState.targetPosition = new Vector2(randomX, randomY);
    }

    private void GameOver()
    {
        StopAllCoroutines();
        Menu.instance.Defeated();
    }
}
