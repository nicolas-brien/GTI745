using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject gameButtonPrefab;
    public GameObject cubeButtonPrefab;
    public Material material;
    public Text infoText;
    public List<ButtonSetting> buttonSettings;
    public Transform gameFieldTransform;
    List<GameObject> gameButtons;
    List<GameObject> cubeButtons;
    public Camera camera;
    int beepCount = 1;
    int round = 1;
    int record = 0;
    List<int> beeps = new List<int>();
    List<int> playerBeeps;
    System.Random rg;
    bool inputEnabled = false;
    bool gameOver = false;
    Button beginRayCast;
    public Collider coll;
    public Text stepText;
    public Text roundText;
    public Text recordText;
    float timeForRestart = 5;

    public float minX = -360.0f;
    public float maxX = 360.0f;

    public float minY = -45.0f;
    public float maxY = 45.0f;

    public float sensX = 100.0f;
    public float sensY = 100.0f;

    float rotationY = 0.0f;
    float rotationX = 0.0f;

    void Start()
    {
        Cursor.visible = false;

        gameButtons = new List<GameObject>();
        cubeButtons = new List<GameObject>();

        float depth = 5f;

        CreateCubeButton(0, new Vector2(0,0), depth);
        CreateCubeButton(1, new Vector2(32,0), depth);
        CreateCubeButton(2, new Vector2(64, 0), depth);
        CreateCubeButton(3, new Vector2(96, 0), depth);
        CreateCubeButton(4, new Vector2(-32, 0), depth);
        CreateCubeButton(5, new Vector2(-64, 0), depth);
        CreateCubeButton(6, new Vector2(-96, 0), depth);


        StartCoroutine(SimonSays());
    }

    IEnumerator SimonSays() {
        inputEnabled = false;
        roundText.text = "Round " + round;
        stepText.text = "Listen";
        stepText.color = Color.red;
        rg = new System.Random((int)System.DateTime.Now.Millisecond);

        setBeeps();

        yield return new WaitForSeconds(2.5f);

        for (int i = 0; i < beeps.Count; i++)
        {
            beep(beeps[i]);

            yield return new WaitForSeconds(2.5f);
        }

        inputEnabled = true;
        stepText.text = "Play";
        stepText.color = Color.green;

        yield return null;
    }

    void beep(int index) {
        LeanTween.value(cubeButtons[index], buttonSettings[index].normalColor, buttonSettings[index].highlightColor, 0.25f).setOnUpdate((Color color) =>
        {
            cubeButtons[index].GetComponent<Renderer>().material.color = color;
        });

        LeanTween.value(cubeButtons[index], buttonSettings[index].highlightColor, buttonSettings[index].normalColor, 0.25f)
            .setDelay(2.0f)
            .setOnUpdate((Color color) => {
                cubeButtons[index].GetComponent<Renderer>().material.color = color;
        });

        playAudio(index);
    }

    void setBeeps() {
        playerBeeps = new List<int>();
        
        beeps.Add(rg.Next(0, cubeButtons.Count));

        beepCount++;
    }


    void CreateCubeButton(int index, Vector2 rotation, float depth)
    {
        GameObject cubeButton = Instantiate(cubeButtonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        
        cubeButton.transform.localPosition = new Vector3(Mathf.Sin(rotation.x * (Mathf.PI * 2) / 360) * depth, 0, Mathf.Cos(rotation.x * (Mathf.PI * 2) / 360) * depth);
        cubeButton.transform.rotation = Quaternion.Euler(new Vector3(rotation.y, rotation.x, 0)); 


     //   cubeButton.GetComponent<Renderer>().material = Instantiate(material) as Material;
        cubeButton.GetComponent<Renderer>().material.color = buttonSettings[index].normalColor;
        cubeButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            onGameButtonClick(index);
        });

        cubeButtons.Add(cubeButton);

        {
            float length = 2f;
            float frequency = 0.0001f * ((float)index + 1f);

            AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(0f, 1f, 0f, -1f), new Keyframe(length, 0f, -1f, 0f));
            AnimationCurve frequencyCurve = new AnimationCurve(new Keyframe(0f, frequency, 0f, 0f), new Keyframe(length, frequency, 0f, 0f));

            LeanAudioOptions audioOptions = LeanAudio.options();
            audioOptions.setWaveSine();
            audioOptions.setFrequency(44100);


            AudioClip audioClip = LeanAudio.createAudio(volumeCurve, frequencyCurve, audioOptions);
            cubeButton.GetComponent<AudioSource>().clip = audioClip;
        }
    }

    void CreateGameButton(int index, Vector3 position, Vector3 rotation)
    {
        GameObject gameButton = Instantiate(gameButtonPrefab, Vector3.zero, Quaternion.identity) as GameObject;

        gameButton.transform.SetParent(gameFieldTransform);
        gameButton.transform.localPosition = position;
        gameButton.transform.Rotate(rotation);

        gameButton.GetComponent<Image>().color = buttonSettings[index].normalColor;
        gameButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            onGameButtonClick(index);
        });

        gameButtons.Add(gameButton);
    }

    void playAudio(int index) {

        AudioSource audioSource = cubeButtons[index].GetComponent<AudioSource>();
        AudioSource.PlayClipAtPoint(audioSource.clip, cubeButtons[index].transform.position);
    }

    void onGameButtonClick(int index) {
        if (!inputEnabled) {
            return;
        }

        beep(index);

        playerBeeps.Add(index);

        if (beeps[playerBeeps.Count - 1] != index) {
            GameOver();
            return;
        }

        if (beeps.Count == playerBeeps.Count) {
            round++;
            StartCoroutine(SimonSays());
        }
    }

    void GameOver()
    {
        gameOver = true;
        inputEnabled = false;
        infoText.text = "Game Over !";
        stepText.text = "";

        if (round > 1 && round > record)
        {
            record = round;
            recordText.text = "Best : " + record;
        }
        round = 1;
    }

    delegate void del1(Button i);
    delegate void del2(del1 i);
    private void Update()
    {
        del2 for_collided_button = (del1 d) => {
            var ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                d(hit.collider.gameObject.GetComponent<Button>());
            }
        };

        if (Input.GetButtonDown("Jump"))
        {
            for_collided_button((Button b) => beginRayCast = b);
        }
        else if (beginRayCast)
        {

            for_collided_button((Button b) => {
                if (b == beginRayCast)
                {
                    b.onClick.Invoke();
                }
            });

            beginRayCast = null;
        }

        if (gameOver)
        {
            timeForRestart -= Time.deltaTime;
            if(timeForRestart < 0)
            {
                restart();
            }
        }

        rotationX += Input.GetAxis("Mouse X") * sensX * Time.deltaTime;
        //rotationY += Input.GetAxis("Mouse Y") * sensY * Time.deltaTime;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);
        camera.transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
    }

    void restart()
    {
        beepCount = 1;
        beeps = new List<int>();
        gameOver = false;
        inputEnabled = false;
        infoText.text = "";
        timeForRestart = 5;
        StartCoroutine(SimonSays());
    }
}
