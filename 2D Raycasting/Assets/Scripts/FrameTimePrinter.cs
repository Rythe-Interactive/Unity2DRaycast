using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class FrameTimePrinter : MonoBehaviour
{
    /// <summary>
    /// singleton stuff to acces this script from anywhere
    /// </summary>
    private static FrameTimePrinter instance;
    public static FrameTimePrinter Instance()
    {
        return instance;
    }
    private void Awake()
    {
        instance = this;
        NameList = new List<string>();
        textContainers = new List<TMP_Text>();
    }

    //text prefab
    public GameObject prefab;

    //Information containers
    int id = 0;
    List<TMP_Text> textContainers;
    List<string> NameList;

    //init lists 
    private void Start()
    {
        //NameList = new List<string>();
        //textContainers = new List<TMP_Text>();
    }
    //Creates new text, returns id
    public int CreateNewFramePrinter(string name)
    {
        GameObject obj = Instantiate(prefab, this.transform);
        TMP_Text text = obj.GetComponentInChildren<TMP_Text>();
        if (text)
        {
            NameList.Add(name);
            id++;
        }
        return (id);
    }
    //Updates based on ID 
    public void GetUpdate(int pID, float value)
    {
        string message = NameList[pID] + value.ToString();
        textContainers[pID].SetText(message);
    }
    public void GetUpdate(int pID, string value)
    {
        string message = NameList[pID] + value;
        textContainers[pID].SetText(message);
    }
}
