using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
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
            StoneProduction();
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

        if(food < 0)
        {
            unemployed--;
            workers--;
        }
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
    private void StoneProduction()
    {
        stone += quarry * 4;
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
            UpdateText();
            string text = $"You successfully built a {name}";
            StartCoroutine(NotificationText(text));
        }
        else
        {
            string text = $"You need {Mathf.Abs(woodCost - wood)} more wood, {Mathf.Abs(stone - stoneCost)} more stone and {Mathf.Abs(unemployed - workerAssign)} more people";
            StartCoroutine(NotificationText(text));
        }

    }
}
