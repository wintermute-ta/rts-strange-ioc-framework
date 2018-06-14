using strange.extensions.command.impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitGameCommand : Command
{
    #region Public
    [Inject]
    public InitGameCompleteSignal InitGameComplete { get; private set; }

    public override void Execute()
    {
        InitGameComplete.Dispatch();        
    }
    #endregion
}
