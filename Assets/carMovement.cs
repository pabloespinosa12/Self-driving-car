using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class carMovement : MonoBehaviour
{
    //Velocidad y direccion manuales
    [Range(-1f,1f)]
    public float velocidad,direccion; //Ambos de -1,1
    //Variables de control del movimiento
    public float maxGiro,maxVelocidad;
    //Rigidbody
    Rigidbody rb;
    //Rayos
    float distanciaDer,distanciaIzq;
    //Distancia recorrida por el coche
    float distanciaRecorrida=0;
    //Posicion y rotacion iniciales
    Vector3 posicionInicial,rotacionInicial;
    //Paredes que bordean la pista, las instancio para invisibilizarlas via script
    private GameObject[] paredes;
    //Entrenando o no
    bool training = false;
    //Parametros del algoritmo genetico
    public GameObject objectoAlgoritmo;
    algoritmo_Genetico algoritmo_Genetico;
    RedNeuronal red=null;
    //Tiempo por individuo
    float tiempoVivo=0;
    

    // Start is called before the first frame update
    void Start()
    {
        if(!training)borrarParedes();
        objectoAlgoritmo = GameObject.FindWithTag("ObjetoAlgoritmo");
        algoritmo_Genetico = objectoAlgoritmo.GetComponent<algoritmo_Genetico>();
        maxVelocidad = 3;
        posicionInicial = transform.position;
        rotacionInicial = transform.eulerAngles;
        rb = GetComponent<Rigidbody>();
        //Si estamos entrenando llamamos cogemos la Red del individuo actual en el algoritmo genetico (GetRed) y ponemos el tiempo x20
        if(training){
           getRed();
           Time.timeScale = 20; 
        } 
        else {
            Time.timeScale = 2; 
            leerRedDeFichero();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {   
        if(red != null){
            visionPorRayos();
            float v,d;
            (v,d) = red.calcularResultado(distanciaDer,distanciaIzq);
            Vector3 posicionAntesDeMoverme = transform.position;
            //print("V: " + v + " D: " + d);
            MoverCocheAutomatico(v,d);
            //MoverCocheManual();
            distanciaRecorrida+=Vector3.Distance(transform.position,posicionAntesDeMoverme); //Voy computando la distancia recorrida frame a frame
            if(training){
                tiempoVivo += Time.deltaTime;
                if(tiempoVivo > 240f){ //Si lleva mas de 4 minutos paramos
                    print("SALTA TEMPORIZADOR ===============================================" + "FITNESS " + fitness());
                    if(fitness()>430f) print("ESTE ES BUENO, fitness: " + fitness());
                    Reset();
                }
            }
        }
    }

    //Funciones

    //El coche de momento se mueve cinematicamente
    void MoverCocheManual(){
        transform.Translate(Vector3.forward * velocidad * maxVelocidad * Time.deltaTime);
        transform.Rotate(new Vector3(0,direccion,0));
    }

    void MoverCocheAutomatico(float v, float d){
        transform.Translate(Vector3.forward * v * maxVelocidad * Time.deltaTime);
        transform.Rotate(new Vector3(0,d,0));       
    }

    void OnCollisionEnter(Collision collision){
        if(collision.gameObject.tag == "pared"){
            Reset();    
        }
    }

    void Reset(){
        if(training){
            algoritmo_Genetico.individuoMuerto(fitness());
            getRed();
        }else{
            red=null;
        }
        tiempoVivo=0;
        distanciaRecorrida=0;
        if(red != null){
            transform.position = posicionInicial;
            transform.eulerAngles = rotacionInicial;
        }
    }

    void visionPorRayos(){
        RaycastHit impactoRayoDerecho;
        bool impactoDer = Physics.Raycast (transform.position, transform.forward + transform.right, out impactoRayoDerecho);
        Debug.DrawRay(transform.position, transform.forward + transform.right, Color.white);
        if(impactoDer){
            distanciaDer = impactoRayoDerecho.distance; 
            //print("Distancia DERECHA: " + distanciaDer);
        }

        RaycastHit impactoRayoIzquierdo;
        bool impactoIzq = Physics.Raycast (transform.position, transform.forward - transform.right,out impactoRayoIzquierdo);
        Debug.DrawRay(transform.position, transform.forward - transform.right, Color.white);
        if(impactoIzq){
            distanciaIzq = impactoRayoIzquierdo.distance;
            //print("Distancia IZQUIERDA: " + distanciaIzq);
        }
    }

    //Borra todas las paredes
    void borrarParedes(){
        paredes = GameObject.FindGameObjectsWithTag("pared");
        foreach (GameObject pared in paredes)
        {
            MeshRenderer mesh = pared.GetComponent<MeshRenderer>();
            mesh.enabled=false;
        }
    }

    //Calcula lo bueno que es este coche
    float fitness(){
        //Velocidad media
        float velocidad = distanciaRecorrida / tiempoVivo;
        //Le damos una ponderacion a la distancia y otra a la velocidad (nos interesa mas la distancia)
        return distanciaRecorrida * 1.4f + velocidad * 0.2f;
    }

    void getRed(){
        try{
            red = algoritmo_Genetico.getIndividuoActual();
        }
        catch (IndexOutOfRangeException e){
            //Si entra aqui es porque el getRed() del metodo Reset se ejecuta antes que el destroy del metodo
            //individuo muerto. Esto no tiene sentido porque se llama antes al otro pero supongo que será algun bug de unity
            //Por eso lo controlo con el catch y hago la red a null para que el coche no se mueva ni haga nada hasta que se destruya
            print( "Este error surge de la concurrencia pero no tiene sentido que se haga\n" + e.Message);
            red = null;
        }
    }

    void leerRedDeFichero(){
        try{
            
            StreamReader sr = new StreamReader("D:\\3ero\\videjuegos\\trabajofinalDefinitivo\\Assets\\ParametrosDEPRUEBA.txt");
            String line = sr.ReadLine();
            List<float> parametros = new List<float>();
            //Leemos el fichero completo
            while (line != null)
            {
                //print(line);
                parametros.Add(float.Parse(line));
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();
            red = new RedNeuronal();
            print("Fichero leido");
            red.setParametros(parametros);
        }
        catch(Exception e){
            print("Error en la lectura del fichero: " + e.Message);
        }
    }
}
