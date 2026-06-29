import axios from 'axios'

/**
 * Shared Axios instance for the BrewUp Chat API.
 * baseURL is injected from the VITE_API_BASE_URL environment variable.
 * No authentication headers are required (API is open).
 */
const chatApiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 15_000,
  headers: {
    'Content-Type': 'application/json',
  },
})

export default chatApiClient
