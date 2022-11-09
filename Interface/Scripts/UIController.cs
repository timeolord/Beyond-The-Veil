using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using World;

public class UIController : MonoBehaviour
{
    public GameObject worldGenerator;
    private UIDocument _document;
    /*private void OnEnable()
    {
        _document = GetComponent<UIDocument>();
        
        var root = _document.rootVisualElement;
        var generateWorldButton = root.Q<Button>("GenerateWorldButton");
        
        generateWorldButton.clicked += OnGenerateWorld;
        
        root.RegisterCallback<NavigationSubmitEvent>((evt) =>
        {
            evt.StopPropagation();
        }, TrickleDown.TrickleDown);
    }

    private void OnGenerateWorld()
    {
        worldGenerator.GetComponent<WorldGenerator>().GenerateWorld();
    }*/
}
