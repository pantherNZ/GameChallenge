using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Level6_Missile : BaseLevel
{
    GameObject window;

    public override void OnStartLevel()
    {
        window = desktop.CreateWindow( "Missiles" );
    }

    protected override void OnLevelUpdate()
    {
        base.OnLevelUpdate();
    }

}