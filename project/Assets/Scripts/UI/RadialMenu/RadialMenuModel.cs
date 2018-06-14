using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenuModel : BaseUIModel
{
    [Inject]
    public IInputManager InputManager { get; private set; }
    [Inject]
    public ILifeCycle LifeCycle { get; private set; }

    public RadialMenuHandler radialMenuHandler { get { return Handler as RadialMenuHandler; } }

    private List<ItemContext> context = new List<ItemContext>();
    List<RadialItemHandler> items = new List<RadialItemHandler>();
    private int selectedItemNum = -1;
    private int openedItemsCount = 0;

    #region RadialMenuModel
    public void SetContext(List<ItemContext> itemsContext)
    {
        if (itemsContext == null || itemsContext.Count <= 0)
        {
            Debug.Log("Context is null! Cant create Radial Menu");
            Shutdown();
        }
        context = itemsContext;
        CreateItems(itemsContext);
    }

    public void OpenNextItem()
    {
        if (openedItemsCount >= items.Count)
        {
            //All items is open;
            return;
        }

        RadialItemHandler item = items[openedItemsCount];
        item.Open();
        openedItemsCount++;
        if(radialMenuHandler.IsOneByOne)
        {
            item.AppearSignal.AddListener(OpenNextItem);
        }
        else
        {
            OpenNextItem();
        }
    }

    private void CloseNextItem()
    {
        if(openedItemsCount > 0 || !radialMenuHandler.IsOneByOne)
        {
            Handler.Close();
            return;
        }

        RadialItemHandler item = items[openedItemsCount];
        item.Close();
        item.DisappearSignal.AddListener(CloseNextItem);
    }

    private void CloseAll()
    {
        if(items.Count <= 0)
        {
            Debug.LogError("Cant close radial items, there are no items.");
            Handler.Close();
            return;
        }

        items[0].DisappearSignal.AddListener(Handler.Close);

        for (int i = 0; i < items.Count; i++)
        {
            items[i].Close();
        }
    }

    public void CreateItems(List<ItemContext> itemsContext)
    {
        List<RadialItemHandler> radialItems = new List<RadialItemHandler>();
        for (int i = 0; i < itemsContext.Count; i++)
        {
            radialItems.Add(UIManager.CreateHandler<RadialItemHandler>(radialMenuHandler.transform, itemsContext[i].PrefabName));
        }
        items = radialItems;
        radialMenuHandler.SetItems(radialItems);
    }

    public void OnItemSelected(int num)
    {
        selectedItemNum = num;
    }
    #endregion

    #region override
    public override void Init(GameObject window, bool globalSignals = false)
    {
        base.Init(window, globalSignals);

        if (radialMenuHandler == null)
        {
            return;
        }
        radialMenuHandler.ItemSelectedSignal.AddListener(OnItemSelected);
        LifeCycle.OnUpdate.AddListener(UpdateLC);
    }

    public override void Open()
    {
        OpenNextItem();
        if (InputManager.Touches.Count > 0)
        {
            base.Open();
        }
        else
        {
            Shutdown();
        }
    }

    public override void OnDisappeared()
    {
        if (selectedItemNum < 0 || selectedItemNum >= context.Count)
        {
            base.OnDisappeared();
            return;
        }
        context[selectedItemNum].ItemAction.Invoke();
        context.Clear();
        base.OnDisappeared();
    }

    public override void Destroy()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].AppearSignal.RemoveListener(OpenNextItem);
            if (radialMenuHandler.IsOneByOne)
            {
                items[i].DisappearSignal.RemoveListener(CloseNextItem);
            }
            UISignals.ReturnToPool.Dispatch(items[i]);
        }

        radialMenuHandler.ItemSelectedSignal.RemoveListener(OnItemSelected);

        if (!radialMenuHandler.IsOneByOne)
        {
            if (items.Count > 0)
            {
                items[0].DisappearSignal.RemoveListener(Handler.Close);
            }
        }

        items.Clear();
        LifeCycle.OnUpdate.RemoveListener(UpdateLC);
        base.Destroy();
    }
    #endregion

    public void UpdateLC(float deltaTime)
    {
        List<ITouchData> touches = InputManager.Touches;
        if(touches == null || touches.Count <= 0)
        {
            if (radialMenuHandler.IsOneByOne)
            {
                CloseNextItem();
            }
            else
            {
                CloseAll();
            }
            return;
        }

        if (touches[0].Phase == TouchPhase.Ended || touches[0].Phase == TouchPhase.Canceled)
        {
            if (radialMenuHandler.IsOneByOne)
            {
                CloseNextItem();
            }
            else
            {
                CloseAll();
            }
            return;
        }

        //for (int i = 0; i < touches.Count; i++)
        //    Debug.Log(touches[i].Position);
        radialMenuHandler.UpdateItems(touches[0]);
    }
}
