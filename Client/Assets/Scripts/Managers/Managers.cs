using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers s_instance; // 유일성이 보장된다
    private static Managers Instance { get { Init(); return s_instance; } } // 유일한 매니저를 갖고온다

    private GameManager _game = new GameManager();
    private LobbyManager _lobby = new LobbyManager();
    private MapManager _map = new MapManager();
    private NetworkManager _network = new NetworkManager();
    private ObjectManager _object = new ObjectManager();
    private RoomManager _room = new RoomManager();
    private PoolManager _pool = new PoolManager();
    private InputManager _input = new InputManager();
    private DataManager _data = new DataManager();
    private ResourceManager _resource = new ResourceManager();
    private SceneManagerEx _scene = new SceneManagerEx();
    private SoundManager _sound = new SoundManager();
    private UIManager _ui = new UIManager();
    private URLManager _url = new URLManager();

    public static GameManager Game { get { return Instance._game; } }
    public static LobbyManager Lobby { get { return Instance._lobby; } }
    public static MapManager Map { get { return Instance._map; } }
    public static NetworkManager Network { get { return Instance._network; } }
    public static ObjectManager Object { get { return Instance._object; } }
    public static RoomManager Room { get { return Instance._room; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static DataManager Data { get { return Instance._data; } }
    public static InputManager Input { get { return Instance._input; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static UIManager UI { get { return Instance._ui; } }
    public static URLManager URL { get { return Instance._url; } }

    void Start()
    {
        Init();
        // 주석 해제하면 s3의 json 정보 가져옴
        //StartCoroutine(CoDataManagerInit());
        StartCoroutine(_network.CoDownloadServerURL());
    }

    void Update()
    {
        _network.OnUpdate();
        _input.OnUpdate();
    }

    static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

            //s_instance._network.Init();
            s_instance._data.Init();
            s_instance._sound.Init();
            s_instance._resource.Init();
            s_instance._room.Init();
        }
    }

    public static void Clear()
    {
        Sound.Clear();
        UI.Clear();

        Pool.Clear();
        Object.Clear();
    }
    public IEnumerator CoDataManagerInit()
    {
        // 추가될 json 데이터들 가져오는 코루틴 넣어주기
        //StartCoroutine(Managers.Data.CoDownLoadJsonData());

        yield return null;
    }
}