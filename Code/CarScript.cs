using System;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;

public class CarScript : MonoBehaviour
{
    Transform[] rayMarkers;
    BoxCollider2D[] checkpoints;
    BoxCollider2D currentCheckpoint;
    BoxCollider2D nextCheckpoint;
    Rigidbody2D rigidbody;
    public Network NeuralNet;
    SpriteRenderer spriteRenderer;
    
    [Range(0.5f, 5f)] [SerializeField] float rayLength = 0.5f;
    public int speed = 2000;
    public int turnSpeed = 500;
    public float totalFitness;
    public float fitness;
    public float turn;

    GeneticAlgorithm geneticAlgorithm;
    public bool crashed;
    void Start()
    {
        rayMarkers = transform.GetComponentsInChildren<Transform>().Where(child => child.name == "Ray Marker").ToArray();
        checkpoints = GameObject.FindGameObjectWithTag("Checkpoints").GetComponentsInChildren<BoxCollider2D>().Where(child => child.transform.name == "Checkpoint").ToArray();
        currentCheckpoint = checkpoints[0];
        nextCheckpoint = checkpoints[1];
        fitness = Vector2.Distance(currentCheckpoint.transform.position, nextCheckpoint.transform.position);
        geneticAlgorithm = GameObject.FindGameObjectWithTag("Game").GetComponent<GeneticAlgorithm>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
    }

    public CarScript InitNetwork()
    {
        NeuralNet = new Network(new[]
        { // Input Layer
            new Neuron(NeuronType.SIGMOID),
            new Neuron(NeuronType.SIGMOID),
            new Neuron(NeuronType.SIGMOID)
        }, new[] 
        { // Hidden Layer
            new Neuron(NeuronType.SIGMOID),
            new Neuron(NeuronType.SIGMOID),
            new Neuron(NeuronType.SIGMOID)
        }, new[]
        { // Output Layer
            new Neuron(NeuronType.BIPOLAR)
        });

        return this;
    }

    public float CalculateFitness() // There are probably better ways to calculate fitness than this lmao
    {
        return fitness - Vector2.Distance(transform.position, nextCheckpoint.transform.position);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        BoxCollider2D colCollider = col.gameObject.GetComponent<BoxCollider2D>();
        if (col.CompareTag("Wall"))
        {
            crashed = true;
        }
        else if (colCollider == nextCheckpoint)
        {
            int colIndex = Array.IndexOf(checkpoints, col.gameObject.GetComponent<BoxCollider2D>());
            
            fitness = totalFitness + Vector2.Distance(currentCheckpoint.transform.position, nextCheckpoint.transform.position);
            currentCheckpoint = colCollider;
            nextCheckpoint = colIndex + 1 == checkpoints.Length ?  checkpoints[0] : checkpoints[colIndex + 1];
        }
    }


    void Update()
    {
        int index = Array.IndexOf(geneticAlgorithm.cars.ToArray(), this);
        Visuals(index);
        if (crashed)
        {
            rigidbody.velocity = Vector2.zero;
            if (index > 1) spriteRenderer.color = new Color(1f, 0.4f, 0.23f);
            return;
        }
        
        Vector2 origin = transform.position + transform.right * (transform.lossyScale.x * 0.5f);
        Vector2[] dirs = {
            transform.right * 0.5f - transform.up,
            transform.right,
            transform.right * 0.5f + transform.up
        };
        
        float[] inputValues = new float[NeuralNet.layers[0].Length];
        for (int i = 0; i < rayMarkers.Length; i++)
        {
            RaycastHit2D ray = Physics2D.Raycast(origin, dirs[i].normalized, rayLength,  LayerMask.GetMask("Wall"));
            Vector2 rayPos = ray.collider == null ? origin + dirs[i] * rayLength : ray.point;
            rayMarkers[i].position = rayPos;
            inputValues[i] = Vector2.Distance(origin, rayPos);
        }
        NeuralNet.SetInputs(inputValues);
        
        totalFitness = CalculateFitness();
        turn = NeuralNet.layers[NeuralNet.layers.Length - 1][0].value;
    }

    void FixedUpdate()
    {
        if(crashed) return;
        transform.Rotate(transform.forward * (turn * Time.fixedDeltaTime * turnSpeed));
        rigidbody.velocity = transform.right * (Time.fixedDeltaTime * speed);
    }

    void Visuals(int index) {
        switch (index)
        {
            case 0:
                spriteRenderer.color = new Color(0.63f, 1f, 0.32f);
                break;
            case 1:
                spriteRenderer.color = new Color(0.44f, 1f, 0.69f);
                break;
            default:
                spriteRenderer.color = new Color(1f, 0.83f, 0.62f);
                break;
        }
        Vector3 pos = transform.localPosition;
        transform.localPosition = new Vector3(pos.x, pos.y, geneticAlgorithm.cars[0] == this ? -1f : 0f);
    }
}
