using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameManager
{
    BaseUnitView Get_UnitView(int id);
}