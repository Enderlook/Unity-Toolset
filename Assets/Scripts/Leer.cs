/*
Lista de clases e interfaces importantes par ael uso en Unity.
Enderlook.Unity.Async.Switch: Propiedades de ayuda para continuar la ejecuci�n de un m�todo asincronico en un hilo de fondo o el principal de Unity.
Enderlook.Unity.Coroutines.CoroutineAwaiter: Extensi�n de �UnityEngine.Coroutine� para permitir el uso de �await� en ellos.
Enderlook.Unity.Coroutines.CoroutineExtensions: M�todos de extensi�n de �UnityEngine.GameObject� y �UnityEngine.MonoBehaviour� para ejecutar las nuevas coroutinas en un adminitrador global.
Enderlook.Unity.Coroutines.CoroutineManager: Administrador de nuevas coroutinas.
Enderlook.Unity.Coroutines.ValueCoroutine: Representa una coroutina del nuevo sistema que aloca menos memoria y posee m�s car�cteristicas.
Enderlook.Unity.Coroutines.ValueYieldInstruction: Tipo de dato devuelto para coroutinas de �Enderlook.Unity.Coroutines.ValueCoroutine�.
Enderlook.Unity.Coroutines.Yield: M�todos de ayuda para crear instancias de �Enderlook.Unity.Coroutines.ValueYieldInstruction�.
Enderlook.Unity.Coroutines.Wait: M�todos de ayuda para ahorrar alocaciones de memoria al usar las coroutinas de Unity. Tambi�n permite esperar typos de datos adicionales.
Enderlook.Unity.Threading.UnityThread: M�todos de ayuda para ejecutar cosas en el thread de Unity o para comprobar si uno se encuentra en dicho thread.
Enderlook.Unity.Jobs.IManagedJob: Interfaz para la ejecuci�n de Unity Jobs �Unity.Jobs.IJob� que almacenan objetos en la memor�a administreada.
Enderlook.Unity.Jobs.IManagedJobExtensions: Extensiones de m�todos sobre �Enderlook.Unity.Jobs.IManagedJob�, �Unity.Jobs.IJob�, y �System.Action<,>� (y sobrecargas de este delegate) para ejecutarlos dentro de un Unity job.
Enderlook.Unity.Jobs.IManagedJobParallelFor: Interfaz para la ejecuci�n de Unity Jobs �Unity.Jobs.IJobParallelFor� que almacenan objetos en la memor�a administreada.
Enderlook.Unity.Jobs.IManagedJobParallelForExtensions: Extensiones de m�todos sobre �Enderlook.Unity.Jobs.IManagedJobParallelFor�, �Unity.Jobs.IJobParallelFor�.
Enderlook.Unity.Jobs.JobHandleAwaiter: Extensi�n de �Unity.Jobs.JobHandle� para permitir el uso de �await� en ellos.
Enderlook.Unity.Jobs.JobManager: Extensi�n de �Unity.Jobs.JobHandle� para llamar automaticamente �.OnComplete()� cuando termine o para ejecutar callbacks tras su completado.
Enderlook.Unity.Toolset.Utils.AssetDatabaseHelper: M�todos de ayuda para `UnityEditor.AssetDatabase`.
Enderlook.Unity.Toolset.Utils.AudioUtil: Puente para ejecutar API internas de Unity que permiten manipular audios en el editor. Dado que es interna de Unity, puede ser inestable entre distintas versiones de Unity.
Enderlook.Unity.Toolset.Utils.EditorGUIHelper: M�todos de ayuda sobre `UnityEditor.EditorGUILayout`.
Enderlook.Unity.Toolset.Utils.ReflectionHelper: M�todos de ayuda (y extensiones) que usan reflecci�n para la construcci�n de editores.
Enderlook.Unity.Toolset.Utils.SerializedObjectExtensions: M�todos de extensi�n sobre `UnityEditor.SerializedObject`.
Enderlook.Unity.Toolset.Utils.SerializedPropertyHelper: M�todos de ayuda y de extensi�n sobre `UnityEditor.SerializedPropety`.
Enderlook.Unity.Pathfinding.ISteeringBehaviour: Intefaz para crear steering behaviours.
Enderlook.Unity.Pathfinding.Path<Vector3>: Determina el camino de un agente.

Componentes:
Enderlook.Unity.AudioManager.AudioController: Configuraci�n y punto de entrada del sistema de audio.
Enderlook.Unity.Coroutines.AutomaticCoroutineScheduler: Componente usado para ejecutar las nuevas coroutinas.
Enderlook.Unity.Pathfinding.FlockingFollower: Determina que el agente pertenece a un reba�o.
Enderlook.Unity.Pathfinding.FlockingLeader: Determina que el agente es el lider de un reba�o.
Enderlook.Unity.Pathfinding.NavigationAgentRigidbody: Sistema de navegaci�n de cada agente, al contrario que el `UnityEngine.AI.NavAgent`, este usa el `UnityEngine.Rigidbody` para funcionar.
Enderlook.Unity.Pathfinding.ObstacleAvoidance: Configura como debe de evitar obstaculos el agente.
Enderlook.Unity.Pathfinding.PathFollower: Configura que el agente debe de seguir un `Endelrook.Unity.Pathfinding.Path<Vector3>`.
Enderlook.Unity.Pathfinding.NavigationSurface: Configura y crea el mapa de navegaci�n.

Editores (Algunos inspectores cambian su apariencia mientras el juego esta activo):
Enderlook.Unity.AudioManager.AudioUnitEditor para Enderlook.Unity.AudioManager.AudioUnit.
Enderlook.Unity.Coroutines.AutomaticCoroutineSchedulerEditor para Enderlook.Unity.Coroutines.AutomaticCoroutineScheduler.
Enderlook.Unity.Coroutines.GlobalCoroutinesManagerUnitEditor para Enderlook.Unity.Coroutines.GlobalCoroutinesManagerUnit.
Enderlook.Unity.Pathfinding.FlockingLeaderEditor para Enderlook.Unity.Pathfinding.FlockingLeader.
Enderlook.Unity.Pathfinding.FlockingFollowerEditor para Enderlook.Unity.Pathfinding.FlockingFollower.
Enderlook.Unity.Pathfinding.NavigationSurfaceEditor para Enderlook.Unity.Pathfinding.NavigaitonSurface.
Enderlook.Unity.Pathfinding.NavigationAgentRigidbodyEditor para Enderlook.Unity.Pathfinding.NavigationAgentRigidbody.
Enderlook.Unity.Pathfinding.PathFollowerEditor para Enderlook.Unity.Pathfinding.PathFollower.

Scriptable Objects:
Enderlook.Unity.AudioManager.AudioBag: Colecci�n no ordenada de `Enderlook.Unity.AudioManager.AudioUnit`s.
Enderlook.Unity.AudioManager.AudioControllerUnit: Configuraci�n global del sistema de audio.
Enderlook.Unity.AudioManager.AudioPlay: Represeneta un `UnityEngine.AudioFile` que esta siendo reproducido. Permite controlarlo.
Enderlook.Unity.AudioManager.AudioSequence: Colecci�n ordenada de `Enderlook.Unity.AudioManager.AudioUnit`s.
Enderlook.Unity.AudioManager.AudioUnit: Configura como se reproduce un `UnityEngine.AudioClip` y es el punto central del sistema de audio.
Enderlook.Unity.Coroutines.GlobalCoroutinesManagerUnit: Configuraci�on global del administrador global del nuevo sistema de coroutinas.

Ventanas (Algunas ventanas cambian su apariencia mientras el juego esta activo):
Enderlook.Unity.JobsInfo: Ventana que muestra informaci�n sobre �Unity.Jobs.JobHandle�s esperando ser completados o esperando ejecutar sus callbacks.
Enderlook.Unity.Coroutines.CoroutinesInfo: Ventana que muestra informaci�n del pool de objetos de `Enderlook.Unity.Coroutines.Wait` y que permite configurar el `Enderlook.Unity.Coroutines.CoroutineManager` global de la reimplementaci�n de coroutinas mejoradas.
Enderlook.Unity.Toolset.ExpandableWindow: Ventana donde se expande la propiedad visualizada. No funciona en ciertos tipos de datos como los primitivos.
Enderlook.Unity.Toolset.ObjectWindow: Ventana que permite editar el contenido de un campo, crear, cambiar o editar objetos que hereden de `UnityEngine.Object`, incluyendo nombre y hide flags.

Lista de menu contextual:
Assets/Enderlook/Audio Manager/Create Audio Unit: Crea un `Enderlook.Unity.AudioManager.AudioUnit` desde un `UnityEngine.AudioClip`.
Assets/Enderlook/Audio Manager/Create Audio Bag: Crea un `Enderlook.Unity.AudioManager.AudioBag` desde una colleci�n de `Enderlook.Unity.AudioManager.AudioUnit`s o `UnityEngineAudioClip`s.
Assets/Enderlook/Audio Manager/Create Audio Sequence: Crea un `Enderlook.Unity.AudioManager.AudioSequence` desde una colleci�n de `Enderlook.Unity.AudioManager.AudioUnit`s o `UnityEngine.AudioClip`s.
Assets/Enderlook/Toolset/Extract Sub-Asset: Extrae un asset de otro asset.
Enderlook/Coroutines Information: Abre la ventana �Enderlook.Unity.Coroutines.CoroutinesInfo�.
Enderlook/Jobs Information: Abre la ventana �Enderlook.Unity.Jobs.JobsInfo�.
Enderlook/Toolset/Draw Vector Relative To Transform/Enable Visualization: Muestra las posiciones de las propiedades serializadas del editor actual que tengan el attributo `DrawVectorRelativeToTransform` en la escena.
Enderlook/Toolset/Draw Vector Relative To Transform/Enable Scene GUI Editing: Permite activar presionando Ctrl y una esfera de posici�n, la ventana de edici�n de una propiedad serializada que tenga el attributo `DrawVectorRelativeToTransform`.
Enderlook/Toolset/Checking/Mode/Refresh: Vuelve a ejecutar los analisis usando reflecci�n en los ensamblados en busca de errores en la colocaci�n de attributos. (Los attributos estan cofigurados para reportar errores comunes de sus usos).
Enderlook/Toolset/Checking/Mode/Disabled: Desactiva el analisis autom�tico realizado despu�s de compilar.
Enderlook/Toolset/Checking/Mode/Unity Compilation Pipeline: Configura que solo se analizen los scripts dentro de la l�nea de compilaci�n de Unity.
Enderlook/Toolset/Checking/Mode/Entire AppDomain: Configura que se analizen todos los scripts dentro del AppDomain.
Propiedad Serializada -> Open In Window: Abre la ventana `Enderlook.Unity.Toolset.ExpandableWindow`.
Propiedad Serializada -> Object Menu: Abre la ventana `Enderlook.Unity.Toolset.ObjectMenu`.
*/