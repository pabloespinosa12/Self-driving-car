using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random=UnityEngine.Random;
using System;

public class RedNeuronal {
    //Definimos las 3 capas de la red
    float [] inputLayer = new float[2];
    float [] hiddenLayer = new float[2];
    float [] outputLayer = new float[2];
    //matrices de pesos (esto es lo que queremos estimar)
    float[,] pesos1 = new float[2,2]; 
    float[,] pesos2 = new float[2,2];
    //Una por nodo de la red excepto en la capa de entrada(esto tambien lo queremos estimar)
    float[] biases = new float[4]; 
    //fitness del individuo que uso la red
    float fitness;

    //Para los calculos usamos las matrices por simplicidad teorica pero para el algortimo usamos una lista de los parametros (genes) a 
    //estimar, en este caso son las biases y las matrices de pesos (pesos1 y pesos2) 
    List<float> parametros=null;

    //Inicializamos los pesos y las biases de forma aleatoria
    public void inicializa(){
        parametros = new List<float>();
        //Biases
        for(int i=0;i<biases.Length;i++){
            biases[i] = Random.Range(-1f,1f);
            parametros.Add(biases[i]);
        } 
        //Pesos
        for(int i = 0; i<2; i++){
            for(int j = 0; j<2 ; j++){
                pesos1[i,j] = Random.Range(-1f,1f);
                pesos2[i,j] = Random.Range(-1f,1f);
                parametros.Add(pesos1[i,j]);
                parametros.Add(pesos2[i,j]);
            }
        }
        
    }

    public (float,float) calcularResultado(float d1,float d2){
        inputLayer[0]= d1;
        inputLayer[1]= d2;
        normalizaMatriz(1);

        hiddenLayer[0] = inputLayer[0] * pesos1[0,0] + inputLayer[1] * pesos1[1,0] + biases[0];
        hiddenLayer[1] = inputLayer[0] * pesos1[0,1] + inputLayer[1] * pesos1[1,1] + biases[1];
        normalizaMatriz(2);
        
        outputLayer[0] = hiddenLayer[0] * pesos2[0,0] + hiddenLayer[1] * pesos2[1,0] + biases[2];
        outputLayer[1] = hiddenLayer[0] * pesos2[0,1] + hiddenLayer[1] * pesos2[1,1] + biases[3];

        return (sigmoid(outputLayer[0]),tangenteHiperbolica(outputLayer[1]));
    }

    public void setFitness(float fitness){
        this.fitness = fitness;
    }

    public float getFitness(){
        return fitness;
    }

    //Calcula la tangente hiperbolica de los elementos de la matriz indicada por 'matriz' donde: 
        //1 --> InputLayer 
        //2 --> HiddenLayer
        //3 --> OutputLayer
    void normalizaMatriz(int matriz){
        for(int i = 0 ; i < 2; i++){
            switch(matriz){
                case 1:
                    inputLayer[i] = tangenteHiperbolica(inputLayer[i]);
                break;
                case 2:
                    hiddenLayer[i] = tangenteHiperbolica(hiddenLayer[i]);
                break;
                case 3:
                    outputLayer[i] = tangenteHiperbolica(outputLayer[i]);
                break;
                
            }
        }
    }

    //calculo de la tangente hiperbólica
    float tangenteHiperbolica(float x){
       return (float)((Math.Exp(x) - Math.Exp(-x))/(Math.Exp(x) + Math.Exp(-x)));
    }

    //calculo de la funcion sigmoide
    float sigmoid(float x){
        return (float)(1 / (1 + Math.Exp(-x)));
    }

    //Getter y setter de la lista de parametros

    public void setParametros(List<float> listaDeParametros){
        if(parametros == null || parametros.Count == listaDeParametros.Count) {
            parametros = listaDeParametros;
            actualizarMatrices();
        }else throw new IndexOutOfRangeException();
    }

    public List<float> getParametros(){
        return parametros;
    }

    //Actualiza las matrices que se utilizan para computar la red
    void actualizarMatrices(){
        int index=0;
        for(int i= 0; i<biases.Length; i++){
            biases[i] = parametros[index];
            index++;
        } 
        for(int i = 0; i<pesos1.GetLength(0);i++){
            for(int j = 0; j<pesos1.GetLength(1);j++){
                pesos1[i,j] = parametros[index];
                index++;
                pesos2[i,j] = parametros[index]; 
                index++;
            }  
        }
    }
}