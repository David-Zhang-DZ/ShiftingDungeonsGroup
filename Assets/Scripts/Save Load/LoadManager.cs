﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadManager : MonoBehaviour
{
    public ItemDictionary ItemDictionary;

    private Inventory _inventory;
    private Equipment _equipment;
    private DungeonManager _dungeon_manager;

    void Start()
    {
        ItemDictionary.BuildDictionary();

        _inventory = FindObjectOfType<Inventory>();
        _equipment = FindObjectOfType<Equipment>();
        _dungeon_manager = FindObjectOfType<DungeonManager>();
    }

    public void LoadGame()
    {
        SaveData saveData = SaveSystem.Load();

        _dungeon_manager.Level = saveData.Level;
        _dungeon_manager.MaxLevel = saveData.MaxLevel;

        for (int i = 0; i < _inventory.InventorySlots.Count; i++)
        {
            _inventory.InventorySlots[i].Item = saveData.InventoryItemNames[i] == "" ? null : ItemDictionary.GetItem(saveData.InventoryItemNames[i]);
        }

        _equipment.PrimaryWeapon.Item = saveData.PrimaryWeaponName == "" ? null : ItemDictionary.GetItem(saveData.PrimaryWeaponName);
        _equipment.PrimaryWeapon.EquipmentStats.Item = _equipment.PrimaryWeapon.Item;
        _equipment.PrimaryWeapon.EquipmentStats.UpdateItem();
        _equipment.SecondaryWeapon.Item = saveData.SecondaryWeaponName == "" ? null : ItemDictionary.GetItem(saveData.SecondaryWeaponName);
        _equipment.SecondaryWeapon.EquipmentStats.Item = _equipment.SecondaryWeapon.Item;
        _equipment.SecondaryWeapon.EquipmentStats.UpdateItem();
    }
}
