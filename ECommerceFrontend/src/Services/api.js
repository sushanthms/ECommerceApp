import axios from "axios";
// withCredentials: true tells Axios/browser that credentials such as cookies should be included for cross-origin requests when the browser's cookie/CORS rules allow it.
// So browser automatically attaches refreshToken cookie
const api = axios.create({baseURL: import.meta.env.VITE_API_URL, withCredentials: true});
// request interceptor runs before Axios sends a request.
api.interceptors.request.use((config) => {
    let sessionId = sessionStorage.getItem("sessionId");

    if (!sessionId) {
        sessionId = crypto.randomUUID();
        sessionStorage.setItem("sessionId", sessionId);
    }

    config.headers["X-Session-Id"] = sessionId;

    const token = localStorage.getItem("token");
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
});

// holds the current running refresh call, so only one refresh happens at a time
let refreshPromise = null;

async function refreshAccessToken() {
    const response = await api.post("/auth/refresh", {});

    localStorage.setItem("token", response.data.token);
    localStorage.setItem("user", JSON.stringify({
        userId: response.data.userId,
        name: response.data.name,
        email: response.data.email,
        role: response.data.role
    }));

    return response.data.token;
}

api.interceptors.response.use(
    (response) => response,
    async (error) => {

        const original = error.config;

        const requestUrl = original?.url?.toLowerCase() ?? "";
        const isAuthCall = requestUrl.includes("/auth/login") || requestUrl.includes("/auth/refresh");

        if (isAuthCall) {
            return Promise.reject(error);
        }

        if (error.response?.status === 401 && !original._retry && !isAuthCall) {
            original._retry = true;

            try {
                if (!refreshPromise) {
                    refreshPromise = refreshAccessToken().finally(() => {
                        refreshPromise = null;
                    });
                }

                const newToken = await refreshPromise;
                original.headers.Authorization = `Bearer ${newToken}`;
                return api(original);   // retry the original request
            } catch {
                localStorage.removeItem("token");
                localStorage.removeItem("user");
                window.location.href = "/";
            }
        }

        return Promise.reject(error);
    }
);

export default api;