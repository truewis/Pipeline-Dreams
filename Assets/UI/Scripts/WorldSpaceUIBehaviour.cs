﻿using UnityEngine;

namespace PipelineDreams {
    public class WorldSpaceUIBehaviour : MonoBehaviour {
        // Start is called before the first frame update
        void Start() {
            GetComponent<Canvas>().worldCamera = Camera.main;
        }

        // Update is called once per frame
        void Update() {

        }
        public void Alert() {
            Debug.Log("Button Pressed");
        }
    }
}