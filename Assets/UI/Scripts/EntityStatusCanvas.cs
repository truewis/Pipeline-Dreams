﻿using System.Collections.Generic;
using UnityEngine;

namespace PipelineDreams {
    public class EntityStatusCanvas : MonoBehaviour {
        [SerializeField] EntityStatusBar ESBPrefab;
        List<EntityStatusBar> ESBList = new List<EntityStatusBar>();
        [SerializeField] EntityDataContainer EM;
        [SerializeField] TaskManager CM;
        [SerializeField] Entity Player;
        MapDataContainer mManager;
        // Start is called before the first frame update
        private void Awake() {
            mManager = FindObjectOfType<MapDataContainer>();
            EM.OnNewEntitySpawn += (e) => {
                var obj = Instantiate(ESBPrefab, transform, true);
                ESBList.Add(obj);
                obj.Init(e);
                obj.Show(false);


            };
            EM.OnEntityDeath += (e) => {
                e.GetComponent<EntityAnimator>().OnDeathClipExit += () => {
                    var obj = ESBList.Find((x) => { return x.entity == e; });
                    ESBList.Remove(obj);
                    Destroy(obj.gameObject);
                };

            };

            Player.GetComponent<EntityDeath>().OnEntityDeath += (e) => {
                foreach (var obj in ESBList)
                    obj.enabled = false;
                Debug.Log("hide!");
            };



            CM.OnTaskEnd += ESBVisibilityRefresh;

        }

        private void ESBVisibilityRefresh() {
            foreach (var obj in ESBList) {

                obj.Show(mManager.IsLineOfSight(obj.entity.IdealPosition, Player.IdealPosition));//line of sight
            }
        }


    }
}