/*
Lista de clases e interfaces importantes par ael uso en Unity.
Enderlook.Unity.Async.Switch: Propiedades de ayuda para continuar la ejecución de un método asincronico en un hilo de fondo o el principal de Unity.
Enderlook.Unity.Coroutines.CoroutineAwaiter: Extensión de ´UnityEngine.Coroutine´ para permitir el uso de ´await´ en ellos.
Enderlook.Unity.Coroutines.CoroutineExtensions: Métodos de extensión de ´UnityEngine.GameObject´ y ´UnityEngine.MonoBehaviour´ para ejecutar las nuevas coroutinas en un adminitrador global.
Enderlook.Unity.Coroutines.CoroutineManager: Administrador de nuevas coroutinas.
Enderlook.Unity.Coroutines.ValueCoroutine: Representa una coroutina del nuevo sistema que aloca menos memoria y posee más carácteristicas.
Enderlook.Unity.Coroutines.ValueYieldInstruction: Tipo de dato devuelto para coroutinas de ´Enderlook.Unity.Coroutines.ValueCoroutine´.
Enderlook.Unity.Coroutines.Yield: Métodos de ayuda para crear instancias de ´Enderlook.Unity.Coroutines.ValueYieldInstruction´.
Enderlook.Unity.Coroutines.Wait: Métodos de ayuda para ahorrar alocaciones de memoria al usar las coroutinas de Unity. También permite esperar typos de datos adicionales.
Enderlook.Unity.Threading.UnityThread: Métodos de ayuda para ejecutar cosas en el thread de Unity o para comprobar si uno se encuentra en dicho thread.
Enderlook.Unity.Jobs.IManagedJob: Interfaz para la ejecución de Unity Jobs ´Unity.Jobs.IJob´ que almacenan objetos en la memoría administreada.
Enderlook.Unity.Jobs.IManagedJobExtensions: Extensiones de métodos sobre ´Enderlook.Unity.Jobs.IManagedJob´, ´Unity.Jobs.IJob´, y ´System.Action<,>´ (y sobrecargas de este delegate) para ejecutarlos dentro de un Unity job.
Enderlook.Unity.Jobs.IManagedJobParallelFor: Interfaz para la ejecución de Unity Jobs ´Unity.Jobs.IJobParallelFor´ que almacenan objetos en la memoría administreada.
Enderlook.Unity.Jobs.IManagedJobParallelForExtensions: Extensiones de métodos sobre ´Enderlook.Unity.Jobs.IManagedJobParallelFor´, ´Unity.Jobs.IJobParallelFor´.
Enderlook.Unity.Jobs.JobHandleAwaiter: Extensión de ´Unity.Jobs.JobHandle´ para permitir el uso de ´await´ en ellos.
Enderlook.Unity.Jobs.JobManager: Extensión de ´Unity.Jobs.JobHandle´ para llamar automaticamente ´.OnComplete()´ cuando termine o para ejecutar callbacks tras su completado.
Enderlook.Unity.Toolset.Utils.AssetDatabaseHelper: Métodos de ayuda para `UnityEditor.AssetDatabase`.
Enderlook.Unity.Toolset.Utils.AudioUtil: Puente para ejecutar API internas de Unity que permiten manipular audios en el editor. Dado que es interna de Unity, puede ser inestable entre distintas versiones de Unity.
Enderlook.Unity.Toolset.Utils.EditorGUIHelper: Métodos de ayuda sobre `UnityEditor.EditorGUILayout`.
Enderlook.Unity.Toolset.Utils.ReflectionHelper: Métodos de ayuda (y extensiones) que usan reflección para la construcción de editores.
Enderlook.Unity.Toolset.Utils.SerializedObjectExtensions: Métodos de extensión sobre `UnityEditor.SerializedObject`.
Enderlook.Unity.Toolset.Utils.SerializedPropertyHelper: Métodos de ayuda y de extensión sobre `UnityEditor.SerializedPropety`.
Enderlook.Unity.Pathfinding.ISteeringBehaviour: Intefaz para crear steering behaviours.
Enderlook.Unity.Pathfinding.Path<Vector3>: Determina el camino de un agente.

Componentes:
Enderlook.Unity.AudioManager.AudioController: Configuración y punto de entrada del sistema de audio.
Enderlook.Unity.Coroutines.AutomaticCoroutineScheduler: Componente usado para ejecutar las nuevas coroutinas.
Enderlook.Unity.Pathfinding.FlockingFollower: Determina que el agente pertenece a un rebaño.
Enderlook.Unity.Pathfinding.FlockingLeader: Determina que el agente es el lider de un rebaño.
Enderlook.Unity.Pathfinding.NavigationAgentRigidbody: Sistema de navegación de cada agente, al contrario que el `UnityEngine.AI.NavAgent`, este usa el `UnityEngine.Rigidbody` para funcionar.
Enderlook.Unity.Pathfinding.ObstacleAvoidance: Configura como debe de evitar obstaculos el agente.
Enderlook.Unity.Pathfinding.PathFollower: Configura que el agente debe de seguir un `Endelrook.Unity.Pathfinding.Path<Vector3>`.
Enderlook.Unity.Pathfinding.NavigationSurface: Configura y crea el mapa de navegación.

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
Enderlook.Unity.AudioManager.AudioBag: Colección no ordenada de `Enderlook.Unity.AudioManager.AudioUnit`s.
Enderlook.Unity.AudioManager.AudioControllerUnit: Configuración global del sistema de audio.
Enderlook.Unity.AudioManager.AudioPlay: Represeneta un `UnityEngine.AudioFile` que esta siendo reproducido. Permite controlarlo.
Enderlook.Unity.AudioManager.AudioSequence: Colección ordenada de `Enderlook.Unity.AudioManager.AudioUnit`s.
Enderlook.Unity.AudioManager.AudioUnit: Configura como se reproduce un `UnityEngine.AudioClip` y es el punto central del sistema de audio.
Enderlook.Unity.Coroutines.GlobalCoroutinesManagerUnit: Configuracióon global del administrador global del nuevo sistema de coroutinas.

Ventanas (Algunas ventanas cambian su apariencia mientras el juego esta activo):
Enderlook.Unity.JobsInfo: Ventana que muestra información sobre ´Unity.Jobs.JobHandle´s esperando ser completados o esperando ejecutar sus callbacks.
Enderlook.Unity.Coroutines.CoroutinesInfo: Ventana que muestra información del pool de objetos de `Enderlook.Unity.Coroutines.Wait` y que permite configurar el `Enderlook.Unity.Coroutines.CoroutineManager` global de la reimplementación de coroutinas mejoradas.
Enderlook.Unity.Toolset.ExpandableWindow: Ventana donde se expande la propiedad visualizada. No funciona en ciertos tipos de datos como los primitivos.
Enderlook.Unity.Toolset.ObjectWindow: Ventana que permite editar el contenido de un campo, crear, cambiar o editar objetos que hereden de `UnityEngine.Object`, incluyendo nombre y hide flags.

Lista de menu contextual:
Assets/Enderlook/Audio Manager/Create Audio Unit: Crea un `Enderlook.Unity.AudioManager.AudioUnit` desde un `UnityEngine.AudioClip`.
Assets/Enderlook/Audio Manager/Create Audio Bag: Crea un `Enderlook.Unity.AudioManager.AudioBag` desde una colleción de `Enderlook.Unity.AudioManager.AudioUnit`s o `UnityEngineAudioClip`s.
Assets/Enderlook/Audio Manager/Create Audio Sequence: Crea un `Enderlook.Unity.AudioManager.AudioSequence` desde una colleción de `Enderlook.Unity.AudioManager.AudioUnit`s o `UnityEngine.AudioClip`s.
Assets/Enderlook/Toolset/Extract Sub-Asset: Extrae un asset de otro asset.
Enderlook/Coroutines Information: Abre la ventana ´Enderlook.Unity.Coroutines.CoroutinesInfo´.
Enderlook/Jobs Information: Abre la ventana ´Enderlook.Unity.Jobs.JobsInfo´.
Enderlook/Toolset/Draw Vector Relative To Transform/Enable Visualization: Muestra las posiciones de las propiedades serializadas del editor actual que tengan el attributo `DrawVectorRelativeToTransform` en la escena.
Enderlook/Toolset/Draw Vector Relative To Transform/Enable Scene GUI Editing: Permite activar presionando Ctrl y una esfera de posición, la ventana de edición de una propiedad serializada que tenga el attributo `DrawVectorRelativeToTransform`.
Enderlook/Toolset/Checking/Mode/Refresh: Vuelve a ejecutar los analisis usando reflección en los ensamblados en busca de errores en la colocación de attributos. (Los attributos estan cofigurados para reportar errores comunes de sus usos).
Enderlook/Toolset/Checking/Mode/Disabled: Desactiva el analisis automático realizado después de compilar.
Enderlook/Toolset/Checking/Mode/Unity Compilation Pipeline: Configura que solo se analizen los scripts dentro de la línea de compilación de Unity.
Enderlook/Toolset/Checking/Mode/Entire AppDomain: Configura que se analizen todos los scripts dentro del AppDomain.
Propiedad Serializada -> Open In Window: Abre la ventana `Enderlook.Unity.Toolset.ExpandableWindow`.
Propiedad Serializada -> Object Menu: Abre la ventana `Enderlook.Unity.Toolset.ObjectMenu`.
*/