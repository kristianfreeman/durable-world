using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;

[Serializable]
public class GameState
{
  public string id;
  public User[] users;
}

[Serializable]
public class User
{
  public string id;
  public string position;
}

[Serializable]
public class PlayerPosition
{
  public string position;
  public string type;
}

public class Client
{
  public string id;
  public GameObject playerObject;
}

public class Connection : MonoBehaviour
{
  WebSocket websocket;
  public GameObject player;
  public GameObject otherPlayerPrefab;

  public Dictionary<string, Client> Clients = new Dictionary<string, Client>();

  // Start is called before the first frame update
  async void Start()
  {
    websocket = new WebSocket("wss://durable-world.signalnerve.workers.dev/websocket");

    websocket.OnOpen += () =>
    {
      Debug.Log("Connection open!");
    };

    websocket.OnError += (e) =>
    {
      Debug.Log("Error! " + e);
    };

    websocket.OnClose += (e) =>
    {
      Debug.Log("Connection closed!" + e);
    };

    websocket.OnMessage += (bytes) =>
    {
      var payload = System.Text.Encoding.UTF8.GetString(bytes);
      GameState gameState = JsonUtility.FromJson<GameState>(payload);

      foreach (var user in gameState.users)
      {
        if (user.id == gameState.id) { continue; }

        try
        {
          Client client;

          if (!Clients.TryGetValue(user.id, out client))
          {
            client = CreateClient(user);
            Debug.Log("Created client");
          }
          var pt = user.position.Split(","[0]); // gets 3 parts of the vector into separate strings
          var x = float.Parse(pt[0]);
          var y = float.Parse(pt[1]);
          var z = float.Parse(pt[2]);
          var newPos = new Vector3(x, y, z);
          client.playerObject.transform.position = newPos;
        }
        catch (Exception e)
        {
          Debug.Log(e);
        }
      }
    };

    // Keep sending messages at every second
    InvokeRepeating("UpdatePosition", 0.0f, 0.5f);

    // waiting for messages
    await websocket.Connect();
  }

  Client CreateClient(User user)
  {
    var newClient = new Client();
    newClient.id = user.id;
    var otherPlayer = Instantiate(otherPlayerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    newClient.playerObject = otherPlayer;
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
      PlayerPosition playerPosition = new PlayerPosition();
      var currentPos = player.transform.position;
      playerPosition.position = $"{currentPos.x},{currentPos.y},{currentPos.z}";
      playerPosition.type = "POSITION_UPDATED";
      await websocket.SendText(JsonUtility.ToJson(playerPosition));
    }
  }

  private async void OnApplicationQuit()
  {
    await websocket.Close();
  }

}