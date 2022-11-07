using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turns of the light effect of all the attack and hit partical effects.
/// The partical effect is alive longer then we want the light to be turned on
/// this turns it off after a certain time.
/// </summary>
public class AutoTurnOffLightComponent : MonoBehaviour
{
    [SerializeField] private float aliveTime = 0.4f;
    [SerializeField] private Light thisLight;

    public void OnEnable()
    {
        StartCoroutine(AutoTurnOff());
    }

    public void OnDisable()
    {
        thisLight.enabled = true;
    }

    IEnumerator AutoTurnOff()
    {
        yield return new WaitForSeconds(aliveTime);
        thisLight.enabled = false;
    }
}