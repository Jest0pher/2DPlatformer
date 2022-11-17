using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    Camera cam;
    public float defaultOrthoSize = 12;
    public float minOrthoSize = 6;
    public float maxOrthoSize = 18;
    public float paddingPercentage = .1f;
    public float scalar;
    public Rect rect;

    bool onFrameDelay;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographicSize = defaultOrthoSize;
    }

    // Update is called once per frame
    void Update()
    {
        if (!onFrameDelay) {
            onFrameDelay = true;
            return;
        }

        float xMax = GameManager.Instance.players[0].transform.position.x;
        float xMin = GameManager.Instance.players[0].transform.position.x;
        float yMax = GameManager.Instance.players[0].transform.position.y;
        float yMin = GameManager.Instance.players[0].transform.position.y;
        for (int i = 0; i < GameManager.Instance.players.Count; i++) {
            Vector3 pos = GameManager.Instance.players[i].transform.position;
            if (pos.x > xMax)
            {
                xMax = pos.x;
            }
            else if (pos.x < xMin) {
                xMin = pos.x;
            }

            if (pos.y > yMax)
            {
                yMax = pos.y;
            }
            else if (pos.y < yMin) {
                yMin = pos.y;
            }
        }

        rect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        rect.size.Set(rect.size.x * (1 - paddingPercentage), rect.size.y * (1 - paddingPercentage));
        
        Debug.DrawLine(rect.min, rect.min + Vector2.right * rect.width,Color.green);
        Debug.DrawLine(rect.min, rect.min + Vector2.up * rect.height, Color.green);
        Debug.DrawLine(rect.max, rect.max + Vector2.left * rect.width, Color.green);
        Debug.DrawLine(rect.max, rect.max + Vector2.down * rect.height, Color.green);
        Vector3 temp = Vector3.back * 10;
        temp += (Vector3)rect.center;
        transform.position = Vector3.Lerp(transform.position, temp, Time.deltaTime*scalar);

        if (GameManager.currentPlayersCount > 1)
        {
            float adjustedOrtho = cam.orthographicSize * (1 - paddingPercentage);
            float camHeight = 2f * adjustedOrtho;
            float camWidth = camHeight * cam.aspect;
            float orthoChange = 0;

            if (rect.width > rect.height * cam.aspect)
            {
                float newCamWidth = Mathf.Lerp(camWidth, rect.width, Time.deltaTime * scalar);
                newCamWidth /= cam.aspect;
                newCamWidth *= .5f;
                orthoChange = adjustedOrtho - newCamWidth;
            }
            else {
                orthoChange = Mathf.Lerp(adjustedOrtho, rect.height / 2f, Time.deltaTime * scalar);
                orthoChange = adjustedOrtho - orthoChange;
            }
            cam.orthographicSize -= orthoChange;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minOrthoSize, maxOrthoSize);
        }
        else {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, defaultOrthoSize, Time.deltaTime * scalar);
        }
        //float rectOrthoHeight = rect.height * .5f;
    }
}
