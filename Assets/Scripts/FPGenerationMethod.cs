using UnityEngine;
using Cysharp.Threading.Tasks;

public class FPGenerationMethod
{
    protected FloorPlanManager _floorPlanManager;
    [SerializeField] protected bool _initialized = false;

    // OBS: outra opção seria adicionar os eventos no Flor plan manager, entretanto cada metodo pode ter suas particularidades de quando
    // chanmar um evento de forma otima alem de obrigar a chamada mesmo em metodos que não faram uso destes eventos para melhora
    // de performance.
    public delegate void CellsGridChangedHandler(CellsGrid cellsGrid);
    protected event CellsGridChangedHandler _OnCellsGridChanged;
    public event CellsGridChangedHandler OnCellsGridChanged{add => _OnCellsGridChanged += value; remove => _OnCellsGridChanged -= value;}
    public delegate void CellChangedHandler(Cell cell);
    protected event CellChangedHandler _OnCellChanged;
    public event CellChangedHandler OnCellChanged{add => _OnCellChanged += value; remove => _OnCellChanged -= value;}
    
    public void TriggerOnCellsGridChanged(CellsGrid cellsGrid)
    {
        _OnCellsGridChanged?.Invoke(cellsGrid);
    }

    public void TriggerOnCellChanged(Cell cell)
    {
        _OnCellChanged?.Invoke(cell);
    }

    public virtual bool Init(FloorPlanManager floorPlanManager)
    {
        _floorPlanManager = floorPlanManager;
        _initialized = true;
        return _initialized;
    }

    public virtual async UniTask<bool> Run()
    {
        return false;
    }

    public virtual void OnDrawGizmos()
    {
        
    }
}
