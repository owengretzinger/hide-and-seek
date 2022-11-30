using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotColdMeter : MonoBehaviour
{
    private Transform trans;
    private PhotonView pView;


    private GameObject hotColdSliderObject;
    private Gradient hotColdGradient;
    private Slider hotColdSlider;
    private Image hotColdSliderFill;
    [HideInInspector] public float scrambleHotColdSliderTime;

    private Hider[] hiders;


    private void Start()
    {
        trans = transform;
        pView = GetComponent<PhotonView>();
        //hotColdSliderObject = GameObject.Find("Hot Cold Meter").transform.GetChild(0).gameObject;
        //hotColdSliderObject.SetActive(false);

        if (pView.IsMine)
        {
            hotColdSliderObject = Instantiate(GetComponent<Seeker>().hotColdSliderPrefab, GameObject.Find("Canvas").transform);
            hotColdSlider = hotColdSliderObject.GetComponentInChildren<Slider>();
            Image[] imgs = hotColdSliderObject.GetComponentsInChildren<Image>();
            foreach (Image img in imgs) if (img.gameObject.name == "Hot Cold Meter Fill") hotColdSliderFill = img;
            GradientColorKey[] colorKey = new GradientColorKey[2];
            colorKey[0].color = Color.blue;
            colorKey[0].time = 0.0f;
            colorKey[1].color = Color.red;
            colorKey[1].time = 1.0f;

            hotColdGradient = new Gradient
            {
                colorKeys = colorKey
            };
        }

        hiders = FindObjectsOfType<Hider>();
    }


    private void Update()
    {
        if (hiders != null)
        {
            if (pView.IsMine && hiders.Length > 0)
            {
                if (scrambleHotColdSliderTime > 0f)
                {
                    ScrambleHotColdSlider();
                }
                else
                {
                    UpdateHotColdSlider();
                }
            }
        }
    }

    private void UpdateHotColdSlider()
    {
        float min = 2f;
        float max = 25f;
        max += min;

        //float minDistance = (hiders[0].transform.position - trans.position).magnitude;
        float minDistance = 10000f;
        foreach (Hider hider in hiders)
        {
            if (hider == null) return;

            if (hider.isDead) continue;
            float dist = (hider.transform.position - trans.position).magnitude;
            if (dist < minDistance)
            {
                minDistance = dist;
            }
        }
        minDistance = Mathf.Clamp(minDistance, min, max);

        float closeness = max - minDistance + min;

        float normalizedValue = closeness / max;

        hotColdSlider.value = normalizedValue;
        hotColdSliderFill.color = hotColdGradient.Evaluate(normalizedValue);
    }

    private void ScrambleHotColdSlider()
    {
        hotColdSlider.value = 1f;

        float value = scrambleHotColdSliderTime - Mathf.Floor(scrambleHotColdSliderTime);
        if (value > 0.5f) value = 1f - value;
        value *= 2f;
        hotColdSliderFill.color = hotColdGradient.Evaluate(value);
        scrambleHotColdSliderTime -= Time.deltaTime;
    }

    private void OnDisable()
    {
        if (pView.IsMine) Destroy(hotColdSliderObject);
    }
}
