using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuBar : MonoBehaviour
{
    [Serializable] 
    public struct MenuOption
    {
        public string optionName;
        public bool selected;
        public TextMeshProUGUI tmp;
        public GameObject go_underline;
        public EmilEvent onSelected;
    }

    public Color selected;
    public Color unselected;
    
    public List<MenuOption> menuOptions = new();
    

    public void SelectOption(string optionName)
    {
        bool found = false;
        for(int i = 0; i < menuOptions.Count; i++)
        {
            var menuOption = menuOptions[i];
            ref var option = ref menuOption;
            
            if(option.optionName == optionName)
            {
                option.selected = true;
                option.tmp.color = selected; 
                option.go_underline.SetActive(true);
                found = true;
                option.onSelected?.Invoke(gameObject);
            }
            else
            {
                option.selected = false;
                option.tmp.color = unselected;
                option.go_underline.SetActive(false);
            }
        }
        
        if(!found) Debug.LogError("Option not found: " + optionName);
    }

}
