using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Level6_Missile : BaseLevel
{
    List<GameObject> windows = new List<GameObject>();
    GameObject shortcut;

    public override void OnStartLevel()
    {
        windows.Add( desktop.CreateWindow( "Missiles" ) );
        var icon = new DesktopIcon()
        {
            name = "Missiles",
            icon = Resources.Load<Texture2D>( "Textures/Full_Recycle_Bin" )
        };
        shortcut = desktop.CreateShortcut( icon, new Vector2Int( 0, 1 ), ( x ) => windows.Add( desktop.CreateWindow( "Missiles" ) ) );
    }

    protected override void OnLevelUpdate()
    {
        base.OnLevelUpdate();
    }

}