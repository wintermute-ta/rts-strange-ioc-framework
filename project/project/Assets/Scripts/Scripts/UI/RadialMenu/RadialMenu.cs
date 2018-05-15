using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//TODO Include StrangeIoC
[RequireComponent(typeof(RectTransform))]
public class RadialMenu : MonoBehaviour {

    [SerializeField]
    private Transform itemParent;

    [SerializeField]
    private float maxAngle = 360.0f;  

    private int selectedItemNum = -1;

    private RadialItemContext[] context;

    public Action CloseAction { get; set; }

    void Start()
    { 
    }

    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            OnClose();
        }
    }

    public void Init(RadialItemContext[] itemsContext, Vector3 globalPosition)
    {
        if(itemsContext == null || itemsContext.Length <= 0)
        {
            Debug.Log("Context is null! Cant create Radial Menu");
            OnClose();
            return;
        }

        context = itemsContext;

        Canvas canvas = GetComponent<Canvas>();
        if(canvas!=null && canvas.renderMode == RenderMode.WorldSpace)
        {
            CreateItemsWorldSpace(itemsContext);
            transform.position = globalPosition;
            ((RectTransform)transform).rotation = Camera.main.transform.rotation;
            transform.position = Vector3.Lerp(transform.position, Camera.main.transform.position, 0.5f);
        }
        else
        {
            CreateItemsScreenSpace(itemsContext);
            itemParent.position = Camera.main.WorldToScreenPoint(globalPosition);
        }
    }

    private void CreateItemsWorldSpace(RadialItemContext[] itemsContext)
    {
        float step = maxAngle / itemsContext.Length;
        // to radians
        step *= (float)(Mathf.PI / 180.0d);

        float radius = Mathf.Abs(((RectTransform)transform).sizeDelta.x / 2.0f);
        
        for (int i = 0; i < itemsContext.Length; i++)
        {
            RadialItem prefab = ResourceManager.Instance.GetPrefabComponent<RadialItem>("RadialItem_" + itemsContext[i].type.ToString());

            if (prefab == null)
            {
                Debug.Log("Prefab with name " + "RadialItem_" + itemsContext[i].type.ToString() + " wasnt found");
                continue;
            }

            RadialItem item = Instantiate(prefab, itemParent);
            item.SetSize(new Vector2(radius, radius));
            item.transform.localPosition = new Vector3(Mathf.Sin(step * i) * radius, Mathf.Cos(step * i) * radius);
            item.gameObject.name = "RadialItem " + i;

            item.Init(OnItemSelected, i);
        }
    }

    private void CreateItemsScreenSpace(RadialItemContext[] itemsContext)
    {
        float step = maxAngle / itemsContext.Length;
        // to radians
        step *= (float)(Mathf.PI / 180.0d);

        float radius = 0.5f;
        float size = 0.25f;

        for (int i = 0; i < itemsContext.Length; i++)
        {
            RadialItem prefab = ResourceManager.Instance.GetPrefabComponent<RadialItem>("RadialItem_" + itemsContext[i].type.ToString());

            if (prefab == null)
            {
                Debug.Log("Prefab with name " + "RadialItem_" + itemsContext[i].type.ToString() + " wasnt found");
                continue;
            }

            RadialItem item = Instantiate(prefab, itemParent);
            Vector2 position = new Vector2(Mathf.Sin(step * i) * radius + 0.5f, Mathf.Cos(step * i) * radius + 0.5f);
            item.SetAnchors(new Vector2(position.x + size, position.y + size), new Vector2(position.x - size, position.y - size));
            item.gameObject.name = "RadialItem " + i;

            item.Init(OnItemSelected, i);
        }
    }

    private void OnClose()
    {
        if(context!=null && context.Length > selectedItemNum && selectedItemNum >= 0)
        {
            if(context[selectedItemNum].action != null)
            {
                context[selectedItemNum].action.Invoke();
            }
        }
        CloseAction.Invoke();
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void OnItemSelected(int num)
    {
        if(num < 0 || num >= context.Length)
        {
            selectedItemNum = -1;
            return;
        }
        selectedItemNum = num;
    }
}