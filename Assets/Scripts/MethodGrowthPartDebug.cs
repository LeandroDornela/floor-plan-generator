using UnityEngine;
using UnityEditor;

public partial class MethodGrowth
{
    public bool DEBUG_GrowSequential(Zone zone, CellsGrid cellsGrid)
    {
        if(TryGrowFromSide(Zone.Side.Right, zone, cellsGrid))
            return true;
        if(TryGrowFromSide(Zone.Side.Left, zone, cellsGrid))
            return true;
        if(TryGrowFromSide(Zone.Side.Top, zone, cellsGrid))
            return true;
        if(TryGrowFromSide(Zone.Side.Bottom, zone, cellsGrid))
            return true;
        return false;
    }


    public override void OnDrawGizmos()
    {
        Vector3 from = new Vector3();
        Vector3 to = new Vector3();
        CellsLineDescription zb = _zoneBorder_TEMP;
        float os = 0.5f;
        
        if(_zoneBorder_TEMP != null)
        switch(_zoneBorder_TEMP.side)
        {
            case Zone.Side.Top:
            from = new Vector3(zb.firstCellCoord.x - os,
                              1,
                              -zb.firstCellCoord.y);
            to = new Vector3(zb.firstCellCoord.x + os + zb.numberOfCells - 1,
                             1,
                             -zb.firstCellCoord.y);
            break;
            case Zone.Side.Bottom:
            from = new Vector3(zb.firstCellCoord.x - os,
                              1,
                              -zb.firstCellCoord.y);
            to = new Vector3(zb.firstCellCoord.x + os + zb.numberOfCells - 1,
                             1,
                             -zb.firstCellCoord.y);
            break;
            case Zone.Side.Left:
            from = new Vector3(zb.firstCellCoord.x,
                              1,
                              -(zb.firstCellCoord.y - os));
            to = new Vector3(zb.firstCellCoord.x,
                             1,
                             -(zb.firstCellCoord.y + os + zb.numberOfCells - 1));
            break;
            case Zone.Side.Right:
            from = new Vector3(zb.firstCellCoord.x,
                              1,
                              -(zb.firstCellCoord.y - os));
            to = new Vector3(zb.firstCellCoord.x,
                             1,
                             -(zb.firstCellCoord.y + os + zb.numberOfCells - 1));
            break;
        }

        Gizmos.color = Color.white;
        Gizmos.DrawLine(from, to);


        if(_floorPlanManager.CellsGrid._cells != null)
        foreach(var cell in _floorPlanManager.CellsGrid._cells)
        {
            Handles.Label(new Vector3(cell.GridPosition.x - os, 1, -cell.GridPosition.y + os), $"[{cell.GridPosition.x}, {cell.GridPosition.y}]");
        }

        foreach(var zone in _floorPlanManager.ZonesInstances)
        {
            if(zone.Value.IsLShaped)
            {
                Gizmos.color = Color.white;
                var pos = new Vector3(zone.Value._lBorderCells.firstCellCoord.x, 1, -zone.Value._lBorderCells.firstCellCoord.y);
                Gizmos.DrawWireSphere(pos + new Vector3(0,-1,0), 0.25f);

                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.black;
                Handles.Label(pos, $"{zone.Value._lBorderCells.numberOfCells} on {zone.Value._lBorderCells.side}", style);
            }
        }
    }
}
