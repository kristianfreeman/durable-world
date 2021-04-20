import faker from 'faker';

export class Character {
  constructor(state, env) {
    this.state = state;
    this.env = env
  }

  async initialize() {
    let stored = await this.state.storage.get("state");
    this.value = stored || { users: [], websockets: [] }
  }

  async tick(skipKey) {
    const users = this.value.users.filter(user => user.id !== skipKey)
    this.value.websockets
      .forEach(
        ({ id, name, websocket }) => {
          websocket.send(
            JSON.stringify({
              id,
              name,
              users
            })
          )
        }
      )
  }

  async key(ip) {
    const text = new TextEncoder().encode(`${this.env.SECRET}-${ip}`)
    const digest = await crypto.subtle.digest(
      { name: "SHA-256", },
      text, // The data you want to hash as an ArrayBuffer
    )
    const digestArray = new Uint8Array(digest)
    return btoa(String.fromCharCode.apply(null, digestArray))
  }

  constructName() {
    function titleCase(str) {
      return str.toLowerCase().split(' ').map(function (word) {
        return word.replace(word[0], word[0].toUpperCase());
      }).join(' ');
    }

    return titleCase(faker.fake("{{commerce.color}} {{hacker.adjective}} {{hacker.abbreviation}}"))
  }

  async handleSession(websocket, ip) {
    websocket.accept()

    try {
      let currentState = this.value;
      const key = await this.key(ip)

      const name = this.constructName()
      let newUser = { id: key, name, position: '0.0,0.0,0.0', rotation: '0.0,0.0,0.0' }
      if (!currentState.users.find(user => user.id === key)) {
        currentState.users.push(newUser)
        currentState.websockets.push({ id: key, websocket })
      }

      this.value = currentState
      this.tick(key)

      websocket.addEventListener("message", async msg => {
        try {
          let { type, position, rotation } = JSON.parse(msg.data)
          switch (type) {
            case 'POSITION_UPDATED':
              let user = currentState.users.find(user => user.id === key)
              if (user) {
                user.position = position
                user.rotation = rotation
              }

              this.value = currentState
              this.tick(key)

              break;
            default:
              console.log(`Unknown type of message ${type}`)
              websocket.send(JSON.stringify({ message: "UNKNOWN" }))
              break;
          }
        } catch (err) {
          websocket.send(JSON.stringify({ error: err.toString() }))
        }
      })

      const closeOrError = async evt => {
        currentState.users = currentState.users.filter(user => user.id !== key)
        currentState.websockets = currentState.websockets.filter(user => user.id !== key)
        this.value = currentState
        this.tick(key)
      }

      websocket.addEventListener("close", closeOrError)
      websocket.addEventListener("error", closeOrError)
    } catch (err) {
      websocket.send(JSON.stringify({ message: err.toString() }))
    }
  }

  // Handle HTTP requests from clients.
  async fetch(request) {
    // Make sure we're fully initialized from storage.
    if (!this.initializePromise) {
      this.initializePromise = this.initialize().catch((err) => {
        // If anything throws during initialization then we need to be
        // sure sure that a future request will retry initialize().
        // Note that the concurrency involved in resetting this shared
        // promise on an error can be tricky to get right -- we don't
        // recommend customizing it.
        this.initializePromise = undefined;
        throw err
      });
    }
    await this.initializePromise;

    // Apply requested action.
    let url = new URL(request.url);

    switch (url.pathname) {
      case "/websocket":
        if (request.headers.get("Upgrade") != "websocket") {
          return new Response("Expected websocket", { status: 406 })
        }
        let ip = request.headers.get("CF-Connecting-IP");
        let pair = new WebSocketPair();
        console.log("Made websocket pair")
        await this.handleSession(pair[1], ip);
        return new Response(null, { status: 101, webSocket: pair[0] });
      case "/":
        // Just serve the current value. No storage calls needed!
        break;
      default:
        return new Response("Not found", { status: 404 });
    }

    return new Response(this.value);
  }
}
