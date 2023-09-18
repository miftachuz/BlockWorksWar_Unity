using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Utils;

public class Helper : MonoBehaviour
{
    public InputDeviceCharacteristics Device { get; set; }

    private List<(InputFeatureUsage<bool>, Action<bool>, bool)> Blasdfnln = new List<(InputFeatureUsage<bool>, Action<bool>, bool)>();

    public void RegisterButton(InputFeatureUsage<bool> triggerButton, Action<bool> action)
    {
        Blasdfnln.Add((triggerButton, action, false));
    }

    private void Update()
    {
        var device = Device.GetDevice();

        for (var i = 0; i < Blasdfnln.Count; i++)
        {
            var (inputFeatureUsage, action, item3) = Blasdfnln[i];
            if (item3 == false)
            {
                if (device.GetFeatureValue(inputFeatureUsage) == true)
                {
                    Blasdfnln[i] = (inputFeatureUsage, action, true);
                    action(true);
                }
            }
            else
            {
                if (device.GetFeatureValue(inputFeatureUsage) == false)
                {
                    Blasdfnln[i] = (inputFeatureUsage, action, false);
                    action(false);
                }
            }
        }
    }
}