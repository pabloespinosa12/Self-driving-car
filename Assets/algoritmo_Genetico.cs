using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random=System.Random;
using System;
using System.IO;

public class algoritmo_Genetico : MonoBehaviour
{
    //Cantidad de individuos en este algoritmo.
    public int numeroIndiviudos;
    //Generacion actual e individuo actual
    int generacionActual=1,individuoActual=0;
    //Variables de control publicas: numero de generaciones y probabilidad de que un individuo mute
    public int numeroDeGeneraciones;   
    public Double probMutacion;
    //Lista de todos los individuos (Redes neuronales) 
    public List<RedNeuronal> individuos;
    //Prefab del coche y variables para crearlo
    public GameObject coche;
    GameObject cocheCreado;
    BoxCollider cocheCollider;
    carMovement scriptCoche;  
    //Modelo final aprendido
    RedNeuronal modeloAprendido;
    //Objeto Random para el crossOver
    Random random;
    //Esta entrando o no 
    //Si entrena se ejecuta normal y si no simplemente me crea un coche y hace que la camara lo siga
    bool training = false;
    
    // Start is called before the first frame update
    void Start()
    {
        if(training) CrearPoblacionInicial();
        else crearCoche();
        random = new Random();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Crea la población inicial
    public void CrearPoblacionInicial()
    {
        individuos = new List<RedNeuronal>();
        //Me creo numeroIndividuos redes neuronales con valores aleatorios
        for(int i = 0; i<numeroIndiviudos; i++){
            RedNeuronal red = new RedNeuronal();
            red.inicializa();
            individuos.Add(red);
        }
        //Creo el coche
        crearCoche();
    }

    void crearCoche(){
        cocheCreado = Instantiate(coche) as GameObject;
        cocheCreado.transform.position = new Vector3(3.231632f,0.218f,12.72662f);
        cocheCreado.transform.eulerAngles = new Vector3(0,-87.63f,0);
        cocheCreado.transform.localScale = new Vector3(0.3059342f, 0.2669752f,0.2270081f);
        Rigidbody rb = cocheCreado.AddComponent<Rigidbody>();
        cocheCollider = cocheCreado.AddComponent<BoxCollider>();
        cocheCollider.size = new Vector3(1.525284f,0.7877282f,4.725314f);
        scriptCoche = cocheCreado.AddComponent<carMovement>();
        cocheCreado.tag="Coche";
        
        if(!training){ 
            GameObject camera = GameObject.FindWithTag("CamaraCoche");
            camera.transform.position += new Vector3(1,0,0);
            camera.AddComponent<tercera_persona>();
        }
    }

    //Devuelve las 2 mejores redes de la poblacion actual
    //Obtengo la red con maximo fitness (padre) y la segunda con mas fitness (madre)
    //El maximo siempre se introduce primero en padre y en madre el segundo mayor valor
    public (RedNeuronal,RedNeuronal) ProcesoDeSeleccion()
    { 
        RedNeuronal padre=null,madre=null;
        foreach (RedNeuronal red in individuos){
            if(padre == null || padre.getFitness() < red.getFitness()) padre = red;
            if(red != padre && (madre == null || madre.getFitness() < red.getFitness())) madre = red;
        }
        return (padre,madre);    
    }

    //Inicia el proceso de cruce a partir de las dos mejores redes de la poblacion
    //Recorro los individuos
    //Para aquellas redes que no sean ni el padre ni la madre
    //Realizo el crossOver asignandole con un 50/50 de prob, o un gen de la madre o uno del padre
    public void Cruce(RedNeuronal padre, RedNeuronal madre)
    {
        List<float> parametrosPadre = padre.getParametros(), parametrosMadre = madre.getParametros(), parametrosAux;
        foreach(RedNeuronal red in individuos){
            if(red != padre && red != madre){ 
                parametrosAux = red.getParametros();
                Double mutacion = random.NextDouble();
                if(mutacion <= probMutacion){ 
                    for(int i = 0 ; i < parametrosAux.Count; i++){
                        parametrosAux[i] = UnityEngine.Random.Range(-1,1); //MUTACION TODOS SUS VALORES CAMBIAN
                    }
                }else{
                    for(int i = 0 ; i < parametrosAux.Count; i++){
                        Double padreOmadre = random.NextDouble();
                        if(padreOmadre <= 0.5) parametrosAux[i] = parametrosPadre[i];
                        else parametrosAux[i] = parametrosMadre[i];
                    }
                }
                try{
                    red.setParametros(parametrosAux);
                }catch(IndexOutOfRangeException e){
                    print(e.Message + "ERROR PARAMETROS INCORRECTOS");
                }
            }
        }       
    }
    
    //Comienzan las generaciones del algoritmo genetico
    //Para cada generacion, escoge los mejores padres, realiza el crossOver y recalcula los fitness de los individuos
    public void SiguienteGeneracion()
    {   
        if(generacionActual <= numeroDeGeneraciones){
            RedNeuronal padre,madre;
            (padre,madre) = ProcesoDeSeleccion();
            print("**************************** GENERACION " + generacionActual + " MEJOR ANTERIOR: " + padre.getFitness()+" ******************************");
            Cruce(padre, madre);
            individuoActual = 0;
            generacionActual++;
            crearCoche();
        }else{
            print("FIN DEL ALGORITMO");
            RedNeuronal padre,madre;
            (padre,madre) = ProcesoDeSeleccion();
            print("El mejor individuo es este:\n" + padre.getFitness());
            modeloAprendido = padre;
            guardarRed();
        }
        
        
    }

    public void individuoMuerto(float fitness){
        if(individuoActual < numeroIndiviudos){
            individuos[individuoActual].setFitness(fitness);
            individuoActual++;
            //Si muerte el ultimo individuo destruimos el coche
            if(individuoActual == numeroIndiviudos) {
                Destroy(cocheCreado);
                //Comenzamos la siguiente generacion
                SiguienteGeneracion();
            }
        }
    }

    public RedNeuronal getIndividuoActual(){
        if(individuoActual < numeroIndiviudos) return individuos[individuoActual];
        else throw new IndexOutOfRangeException();
    }

    void muestra(List<float> list){
        foreach (float item in list)
        {
            print(item + " ");
        }
    }

    void guardarRed(){
        try{
            StreamWriter sw = new StreamWriter("D:\\3ero\\videjuegos\\trabajofinalDefinitivo\\Assets\\testing.txt");
            foreach (float p in modeloAprendido.getParametros())
            {
                sw.WriteLine(p);
            }
            sw.Close();
        }
        catch(Exception e){
            print("Error en la escritura del fichero: " + e.Message);
        }
        finally{
            print("FICHERO ESCRITO");
        }
    }
}
