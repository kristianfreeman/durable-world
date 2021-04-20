// In order for the workers runtime to find the class that implements
// our Durable Object namespace, we must export it from the root module.
export { Character } from './character.mjs'

const DEBUG = false

import Template from './template.js'

export default {
  async fetch(request, env) {
    try {
      return await handleRequest(request, env)
    } catch (e) {
      return new Response(e.message)
    }
  },
}

async function handleRequest(request, env) {
  const url = new URL(request.url)
  if (DEBUG && url.pathname === "/") return Template()

  let id = env.CHARACTER.idFromName("signalnerve")
  let obj = env.CHARACTER.get(id)
  let resp = await obj.fetch(request)
  return resp
}
