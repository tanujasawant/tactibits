using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEzExp;

public class Definition : MonoBehaviour {

    public GameObject prefab;
    GameObject lastCube = null;

    public int trials = 10;

    void Awake()
    {
        // GenerateNewCube();
        // TODO  :
        // Trial currentTrial = EzExp.Instance.LoadFile("Assets/ezExp/Examples/test_file.csv");
        EzExp.Instance.SetParameters("X", "Y", "DURATION");

        switch(EzExp.Instance.GetParameterData("TECHNIQUE"))
        {
            case "A":
                break;
            case "B":
                break;

        }
    }

    void Start()
    {
        EzExp.Instance.StartExperiment();
    }
    void Update()
    {
        CheckClick();
    }

    void CheckClick()
    {
        if (Input.GetMouseButtonDown(0))
        { // if left button pressed...
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject == lastCube)
                {
                    NextStep();
                }
            }
        }

    }

    void NextStep()
    {
        Destroy(lastCube);
        if (trials > 0)
        {
            GenerateNewCube();
            trials += -1;
        }
        else
        {
            Finished();
        }
    }

    void Finished()
    {
        Debug.Log("Finished ");
    }

    void GenerateNewCube()
    {
        lastCube = null;

        Vector3 randPos = new Vector3(Random.Range(-5, 5), Random.Range(-2, 2), Random.Range(-5, 5));
        lastCube = GameObject.Instantiate(prefab);
        lastCube.transform.position = randPos;
    }
}
