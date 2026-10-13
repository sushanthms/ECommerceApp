import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";
import { useNotifications } from "../Context/NotificationContext.jsx";
import "./AdminNotifications.css";

function AdminNotifications() {
    const { notifications, markRead, markAllRead } = useNotifications();

    return (
        <>
        <Header />

            <div className="page-layout">

                <Sidebar />
        <div className="notifications-page">

            <div className="notifications-header">
                <h2>Notifications</h2>
                <button className="mark-all-button" onClick={markAllRead}>Mark all as read</button>
            </div>

            {notifications.length === 0 && (<p className="no-notifications">No notifications.</p>)}

            {notifications.length > 0 && (
                <div className="notifications-list">
                    {notifications.map((n) => (
                        <div key={n.id} className={`notification-item ${!n.isRead ? "unread" : ""}`} onClick={() => !n.isRead && markRead(n.id)}>
                            <span className="notification-message">{n.message}</span>
                            <span className="notification-date">{new Date(n.createdAt).toLocaleString()}</span>
                        </div>
                    ))}
                </div>
            )}
        </div>
        </div>
        </>
    );
}

export default AdminNotifications;