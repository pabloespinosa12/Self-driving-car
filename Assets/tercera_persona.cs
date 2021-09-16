using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tercera_persona : MonoBehaviour
{
    Vector3 vectorAobjetivo, posicionInicialCamara;
    GameObject objetivo;
    void Start()
    {
        objetivo = GameObject.FindGameObjectWithTag("Coche");
        vectorAobjetivo = transform.position - objetivo.transform.position;
        posicionInicialCamara = transform.position;
    }
    void Update()
    {
        Vector3 targetCamPosicion = objetivo.transform.position + vectorAobjetivo;
        transform.position = Vector3.Lerp(posicionInicialCamara, targetCamPosicion, 0.8f);
        posicionInicialCamara = targetCamPosicion;
    }
}

