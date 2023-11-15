import axios, { AxiosRequestConfig, AxiosInstance } from "axios";
//axios 1.3.4
export class HTTPSingleton {
  private static instance: HTTPSingleton;
  private static config: AxiosRequestConfig = {
    baseURL: "https://localhost:44307/",
    headers: {
      "Content-Type": "application/json",
      "Access-Control-Allow-Origin": "*",
      // Lỗi CORS do dòng dưới không hiểu vì sao bật thì bị
      // "Access-Control-Allow-Origin": "*",
      // "Access-Control-Allow-Methods": "GET, POST, PATCH, PUT, DELETE, OPTIONS",
    },
    timeout: 1000000,
  };
  private HTTP: AxiosInstance;
  private constructor() {
    this.HTTP = axios.create(HTTPSingleton.config);
  }
  static getInstance() {
    if (!HTTPSingleton.instance) {
      HTTPSingleton.instance = new HTTPSingleton();
    }
    return HTTPSingleton.instance;
  }
  getHTTP(): AxiosInstance {
    return this.HTTP;
  }
}
export const HTTP: AxiosInstance = HTTPSingleton.getInstance().getHTTP();
