using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;
using static EMILtools.Extensions.EnumerateEX;

public class GameObjectSetter : MonoBehaviour
{
    public PairStringObj[] pairs;
    public GameObject Enable(string _name) => pairs.SetActive(_name, true);
    public GameObject Disable(string _name) => pairs.SetActive(_name, false);

    public void EnableAll() => pairs.SetAllActive(true);
    public void DisableAll() => pairs.SetAllActive(false);


}

