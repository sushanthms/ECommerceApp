import { createContext, useContext, useEffect, useState } from "react";
import api from "../Services/api.js";

const AuthContext = createContext();// creating a shared container called AuthContext

export function AuthProvider({ children }) {

// true while the first verify runs (only if a saved login exists)
const [loading, setLoading] = useState(() => !!localStorage.getItem("token"));

useEffect(() => {
    if (!localStorage.getItem("token")) return;

    api.get("/auth/verify")
        .then((res) => {
            // the server's role (from the signed JWT) replaces the edited one
            setUser((prev) => {
                if (!prev) return prev;
                const updated = { ...prev, role: res.data.role };
                localStorage.setItem("user", JSON.stringify(updated));
                return updated;
            });
        })
        .catch(() => {
            localStorage.removeItem("token");
            localStorage.removeItem("user");
            setUser(null);
        })
        .finally(() => setLoading(false));
}, []);

    const [user, setUser] = useState(() => {
        const storedUser = localStorage.getItem("user");
        const token = localStorage.getItem("token");

        if (!storedUser || !token) {
            return null;
        }
        try {
            return JSON.parse(storedUser);
        } catch {
            localStorage.removeItem("user");
            return null;
        }
    });

    const login = (userData, token) => {
        localStorage.setItem("token", token);
        localStorage.setItem("user", JSON.stringify(userData));

        setUser(userData);
    };

    const logout = async () => {
        try {
            await api.post("/auth/logout");
        }
        catch {
            // even if the API call fails, still log out happens in the client
        }

        localStorage.removeItem("token");
        localStorage.removeItem("user");

        setUser(null);
    };

    const isLoggedIn = user !== null;

    return (
        <AuthContext.Provider value={{ user, isLoggedIn, loading, login, logout }}>
            {loading ? <div>Loading...</div> : children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    return useContext(AuthContext);
}
// we will use the AuthContext by writing like this const auth = useContext(AuthContext);
// by the export function we can use like this
// const auth = useAuth();