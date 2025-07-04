using System.Collections;
using TMPro;
using UnityEngine;

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
    [SerializeField] private int stone;
    [SerializeField] private int iron;
    [SerializeField] private int tools;

    //farm, house, ironMines, goldMines, woodcutter, blacksmith, quarry,
    [Space(10)]
    [Header("Buildings")]
    [SerializeField] private int house; //1 house takes 4 people
    [SerializeField] private int farm; //x
    [SerializeField] private int woodcutter; //x
    [SerializeField] private int blacksmith;
    [SerializeField] private int quarry;
    [SerializeField] private int ironMines;
    [SerializeField] private int goldMines;

    [Space(10)]
    [Header("Resources Texts")]
    [SerializeField] private TMP_Text daysText;
    [SerializeField] private TMP_Text populationText;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text foodText;
    [SerializeField] private TMP_Text ironText;

    [Space(10)]
    [Header("Buildings Texts")]
    [SerializeField] private TMP_Text houseText;
    [SerializeField] private TMP_Text farmText;
    [SerializeField] private TMP_Text woodcutterText;
    [SerializeField] private TMP_Text notifiactionText;
    private float timer;
    private bool isGameRunning;

    private void Awake()
    {
        UpdateText();
    }
    private void Update()
    {
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
            FoodConsumption();
            IncreasePopulation();

            UpdateText();

            timer = 0;
        }
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
    }
    /// <summary>
    /// Gather food
    /// </summary>
    private void FoodGathering()
    {
        food += unemployed / 2;
    }
    /// <summary>
    /// produce food
    /// </summary>
    private void FoodProduction()
    {
        food += farm * 4;
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
    private void WorkerAssign(int amount)
    {
        unemployed -= amount;
        workers += amount;
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
        if (wood >= 10 && CanAssignWorker(2))
        {
            wood -= 10;
            WorkerAssign(2);
            farm++;
            UpdateText();
            string text = $"You successfully built a farm";
            StartCoroutine(NotificationText(text));
        }
        else
        {
            string text = $"You need {Mathf.Abs(10 - wood)} wood or{Mathf.Abs(1 - unemployed)} more people";
            StartCoroutine(NotificationText(text));
        }
    }
    /// <summary>
    /// izgradi woodcuttera
    /// </summary>
    public void BuildWoodcutter()
    {
        if(wood >= 5 && iron > 0 && CanAssignWorker(1))
        {
            iron--;
            wood -= 5;
            WorkerAssign(1);
            woodcutter++;
            UpdateText();
            string text = $"You have built woodcutters hut";
            StartCoroutine(NotificationText(text));
        }
        else
        {
            string text = $"You need {5 - wood} wood or {Mathf.Abs(1 - iron)} more iron or{Mathf.Abs(1 - unemployed)} more people";
            StartCoroutine(NotificationText(text));
        }
    }
    /// <summary>
    /// izgradi house
    /// </summary>
    public void BuildHouse()
    {
        if(wood >= 2)
        {
            wood -= 2;
            house++;
            UpdateText();
            string text = $"You successfully built a house";
            StartCoroutine(NotificationText(text));
        }
        else
        {
            string text = $"You need {Mathf.Abs(2 - wood)} more wood";
            StartCoroutine(NotificationText(text));
        }
    }

    /// <summary>
    /// proizvodnja drva
    /// </summary>
    private void WoodProduction()
    {
        wood += woodcutter * 2;
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

        //buildings
        houseText.text = $"HOUSES: {house}";
        farmText.text = $"FARMS: {farm}";
        woodcutterText.text = $"WOODCUTTERS: {woodcutter}";
    }

    IEnumerator NotificationText(string text)
    {
        notifiactionText.text = text;
        yield return new WaitForSeconds(2);
        notifiactionText.text = string.Empty;
    }

    //TODO: Make this method a class
    //private void BuildCost(int woodCost, int stoneCost, int workerAssign)
    //{
    //    if(wood >= woodCost && stone >= stoneCost && unemployed >= workerAssign)
    //    {
    //        wood -= woodCost;
    //        stone -= stoneCost;
    //        unemployed -= workerAssign;
    //        workers += workerAssign;
    //    }
    //}
}
