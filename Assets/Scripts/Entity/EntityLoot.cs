﻿using UnityEngine;

namespace PipelineDreams {
    public class EntityLoot : MonoBehaviour {
        Entity entity;
        ItemContainerPlayer PI;
        private void Awake() {
            GetComponent<EntityDeath>().OnEntityDeath += EntityLoot_OnEntityDeath;
            entity = GetComponent<Entity>();
            PI = (ItemContainerPlayer)FindObjectOfType(typeof(ItemContainerPlayer));
        }

        private void EntityLoot_OnEntityDeath(Entity obj) {
            if (Random.Range(0, 1) < entity.Data.FindParameterFloat("LootChance"))
                PI.AddItem(entity.Data.FindParameterString("Loot"));
        }

        // Start is called before the first frame update
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
    }
}