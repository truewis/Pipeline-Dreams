﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollectionUI : MonoBehaviour
{
    List<ItemUI> ItemUIs;
    ItemCollection PI;
    
    private void Awake() {
        PI = (ItemCollection)FindObjectOfType(typeof(ItemCollection));
        ItemUIs = new List<ItemUI>(GetComponentsInChildren<ItemUI>());
        PI.OnRefreshItems += PI_OnRefreshUI;
        PI.OnChangeItemSlotAvailability += PI_OnRefreshItemSlotUI;
    }
    private void Start() {

        PI.InvokeUIRefresh();
    }

    private void PI_OnRefreshItemSlotUI(int num) {
        for (int i = ItemUIs.Count-1; i >= 0; i--) {
            ItemUIs[i].gameObject.SetActive(i<num);
        }
    }

    private void PI_OnRefreshUI(Item[] obj) {

        for (int i = ItemUIs.Count-1; i >= obj.Length; i--) {
            ItemUIs[i].Clear();
        }
        for (int i = obj.Length-1; i >= 0; i--) {
            ItemUIs[i].Refresh(obj[i]);
        }
    }

    
}