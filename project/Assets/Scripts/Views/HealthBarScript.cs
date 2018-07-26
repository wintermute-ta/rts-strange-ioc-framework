using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class HealthBarScript : MonoBehaviour
    {
        private Slider obj;
        private Camera mainCamera;
        private Canvas canvas;

        void Start()
        {
            mainCamera = Camera.main;
        }
        private void Awake()
        {
            obj = GetComponent<Slider>();
            canvas = GetComponentInParent<Canvas>();
        }
        void Update()
        {
            //rotate canvas with healthbar to the camera
            canvas.transform.eulerAngles = new Vector3(
                mainCamera.transform.eulerAngles.x,
                mainCamera.transform.parent.gameObject.transform.eulerAngles.y,
                canvas.transform.eulerAngles.z
                );
        }

        /// <summary>
        /// Set start value
        /// </summary>
        public void SetStartValue(float maxValue, float currentValue)
        {
            obj.maxValue = maxValue;
            obj.value = currentValue;
        }
        /// <summary>
        /// Set new value
        /// </summary>
        /// <param name="value"></param>
        public void SetNewValue(float value)
        {
            obj.value = value;
        }
    }
}