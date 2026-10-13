import { createContext, useContext, useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { useAuth } from "./AuthContext.jsx";
import { useToast } from "./ToastContext.jsx";
import api from "../Services/api.js";

const NotificationContext = createContext();// creates a Context object. a place where shared data can be provided

export function NotificationProvider({ children }) {
    const { user, isLoggedIn } = useAuth();
    const showToast = useToast();
    
    const [notifications, setNotifications] = useState([]);

    const isAdmin = isLoggedIn && user?.role === "Admin";

    useEffect(() => {
        if (!isAdmin) {
            setNotifications([]);
            return;
        }

        // loading the old notifications from the database
        api.get("/notifications").then((res) => setNotifications(res.data));

        const hubUrl = import.meta.env.VITE_API_URL.replace("/api", "") + "/notificationHub";

        // connection code runs when admin log in and stops when admin log out
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, { accessTokenFactory: () => localStorage.getItem("token") })
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveNotification", (n) => {
            setNotifications((prev) => [n, ...prev]);
            showToast(n.message, "warning");
        });

        let stopped = false;

        const startPromise = connection.start();

        startPromise
            .then(() => {
                console.log("SignalR connected");
            })
            .catch((err) => {
                if (!stopped) {
                    console.error("SignalR error", err);
                }
            });

        return () => {
            stopped = true;

            startPromise.finally(() => {
                connection.stop();
            });
        };

    }, [isAdmin]);

    const unreadCount = notifications.filter((n) => !n.isRead).length;

    const markRead = async (id) => {
        await api.put(`/notifications/${id}/read`);
        setNotifications((prev) =>
            prev.map((n) => (n.id === id ? { ...n, isRead: true } : n))
        );
    };

    const markAllRead = async () => {
        await api.put("/notifications/read-all");
        setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
    };

    return (// these values/methods are available for all the components inside it
        <NotificationContext.Provider value={{ notifications, unreadCount, markRead, markAllRead }}>
            {children}
        </NotificationContext.Provider>
    );
}

// export const {notifications, unreadCount, markRead, markAllRead} = useContext(NotificationContext);
export const useNotifications = () => useContext(NotificationContext);