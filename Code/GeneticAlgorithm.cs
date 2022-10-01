using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    public GameObject car;
    public Transform trackStart;
    public TMPro.TextMeshProUGUI fitnessText;
    public TMPro.TextMeshProUGUI turnText;
    public TMPro.TextMeshProUGUI generationText;
    [Range(1, 100)] public int generationSize = 50;
    [Range(0, 1)] public float mutationRate = 0.5f;
    int generation = 1;
    
    public List<CarScript> cars = new List<CarScript>();
    // Start is called before the first frame update
    void Start()
    {
        SetGeneration(generation);
        NewGeneration();
    }

     public void NewGeneration()
    {
        foreach(CarScript car in cars) Destroy(car.gameObject);
        cars = new List<CarScript>();
        for (int i = 0; i < generationSize; i++)
            cars.Add(Instantiate(car, trackStart.position, trackStart.rotation, transform).GetComponent<CarScript>().InitNetwork());
    }

     void SetGeneration(int gen)
     {
         generation = gen;
         generationText.text = "Generation: " + gen;
     }

     public void Restart()
     {
         NewGeneration();
         SetGeneration(1);
     }
     
    void Update()
    {
        cars = cars.OrderByDescending(car => car.totalFitness).ToList();
        CarScript[] aliveCars = cars.Where(car => !car.crashed).ToArray();
        CarScript bestCar = cars[0];
        CarScript secondBest = cars[1];
        if (aliveCars.Length == 0)
        {
            NewGeneration();
            SetGeneration(generation + 1);
            foreach (CarScript car in cars)
                car.NeuralNet.Cross(bestCar.NeuralNet, secondBest.NeuralNet, mutationRate);
        }

        fitnessText.text = "Fitness: " + bestCar.totalFitness;
        turnText.text = "Turn: " + bestCar.turn;
        Vector2 pos = bestCar.transform.position;
        Camera.main.transform.position = new Vector3(pos.x, pos.y, -100f);
    }
}
