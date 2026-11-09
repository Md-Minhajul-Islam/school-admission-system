export interface AppRuntimeConfig {
  API_BASE_URL?: string;
}

declare global {
  interface Window {
    __APP_CONFIG__?: AppRuntimeConfig;
  }
}

const runtimeUrl = window.__APP_CONFIG__?.API_BASE_URL?.trim();
const buildTimeUrl = import.meta.env.VITE_API_BASE_URL?.trim();

export const API_BASE_URL = (
  runtimeUrl ||
  buildTimeUrl ||
  "http://localhost:8080"
).replace(/\/+$/, "");
