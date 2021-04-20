using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;

[Serializable]
public class GameState
{
  public string id;
  public string name;
  public User[] users;
}

[Serializable]
public class User
{
  public string id;
  public string name;
  public string position;
  public string rotation;
}

[Serializable]
public class PlayerPosition
{
  public string position;
  public string rotation;
  public string type;
}

public class Client
{
  public string id;
  public GameObject playerObject;
  public InterpolateMovement interpolateMovement;
}

public class Connection : MonoBehaviour
{
  WebSocket websocket;
  public GameObject player;
  public GameObject otherPlayerPrefab;
  public GameObject onlineText;

  public Vector3 lastPosition;

  public Dictionary<string, Client> Clients = new Dictionary<string, Client>();

  private int maxRetries = 10;
  private int retries = 0;


  // Start is called before the first frame update
  void Start()
  {
    Connect();
  }
  async void Connect()
  {
    retries += 1;
    if (maxRetries < retries)
    {
      return;
    }

    websocket = new WebSocket("wss://durable-world.signalnerve.workers.dev/websocket");

    websocket.OnOpen += () =>
    {
      Debug.Log("Connection open!");
    };

    websocket.OnError += (e) =>
    {
      Debug.Log("Error! " + e);
      Connect();
    };

    websocket.OnClose += (e) =>
    {
      Debug.Log("Connection closed!" + e);
      Connect();
    };

    websocket.OnMessage += (bytes) =>
    {
      var payload = System.Text.Encoding.UTF8.GetString(bytes);
      GameState gameState = JsonUtility.FromJson<GameState>(payload);

      Debug.Log(payload);
      foreach (var user in gameState.users)
      {
        try
        {
          if (user.id == gameState.id)
          {
            continue;
          }

          Client client;
          if (!Clients.TryGetValue(user.id, out client))
          {
            client = CreateClient(user);
          }

          var rt = user.rotation.Split(","[0]); // gets 3 parts of the vector into separate strings
          var rtx = float.Parse(rt[0]);
          var rty = float.Parse(rt[1]);
          var rtz = float.Parse(rt[2]);
          var newRot = Quaternion.Euler(rtx, rty, rtz);
          client.interpolateMovement.endRotation = newRot;

          var pt = user.position.Split(","[0]); // gets 3 parts of the vector into separate strings
          var ptx = float.Parse(pt[0]);
          var pty = float.Parse(pt[1]);
          var ptz = float.Parse(pt[2]);
          var newPos = new Vector3(ptx, pty, ptz);
          client.interpolateMovement.endPosition = newPos;
        }
        catch (Exception e)
        {
          Debug.Log(e);
        }
      }

      TMPro.TextMeshProUGUI text = onlineText.GetComponent<TMPro.TextMeshProUGUI>();
      text.text = $"Online: {gameState.users.Length + 1}\nPlaying as {gameState.name}";
    };

    // Keep sending messages at every 0.2 seconds
    InvokeRepeating("UpdatePosition", 0.0f, 0.2f);

    // waiting for messages
    await websocket.Connect();
  }

  Client CreateClient(User user)
  {
    var newClient = new Client();
    newClient.id = user.id;
    var otherPlayer = Instantiate(otherPlayerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    otherPlayer.name = user.id;

    TMPro.TextMeshPro text = otherPlayer.GetComponentInChildren<TMPro.TextMeshPro>();
    text.text = user.name;

    newClient.playerObject = otherPlayer;
    newClient.interpolateMovement = otherPlayer.GetComponent<InterpolateMovement>();
    Clients.Add(user.id, newClient);
    return newClient;
  }

  void Update()
  {
#if !UNITY_WEBGL || UNITY_EDITOR
    websocket.DispatchMessageQueue();
#endif
  }

  async void UpdatePosition()
  {
    if (websocket.State == WebSocketState.Open)
    {
      var currentPos = player.transform.position;
      if (currentPos == lastPosition)
      {
        return;
      }

      PlayerPosition playerPosition = new PlayerPosition();
      playerPosition.position = $"{currentPos.x},{currentPos.y},{currentPos.z}";
      var currentRot = player.transform.rotation;
      playerPosition.rotation = $"{currentRot.eulerAngles.x},{currentRot.eulerAngles.y},{currentRot.eulerAngles.z}";
      playerPosition.type = "POSITION_UPDATED";
      await websocket.SendText(JsonUtility.ToJson(playerPosition));
      lastPosition = currentPos;
    }
  }

  private async void OnApplicationQuit()
  {
    await websocket.Close();
  }

}