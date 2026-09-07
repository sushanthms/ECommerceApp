import { useLocation } from "react-router-dom";
import "./Sidebar.css";

function Sidebar({ menuOpen, setMenuOpen, onCartClick, role }) {

    const location = useLocation();

    return (
        <aside className={menuOpen ? "sidebar open" : "sidebar close"}>

            <div className="sidebar-header">
                <button className="sidebar-toggle" onClick={() => setMenuOpen(prev => !prev)}>☰</button>
                <h2 className="sidebar-title">Dashboard</h2>
            </div>

            <ul className="sidebar-list">

                {role === "Admin" ? (
                    <>
                        <li><a href="/admin" className={location.pathname === "/admin" ? "active" : ""}>Home</a></li>
                        <li><a href="/admin/orders" className={location.pathname === "/admin/orders" ? "active" : ""}>Manage Orders</a></li>
                        <li><a href="/admin/users" className={location.pathname === "/admin/users" ? "active" : ""}>Manage Users</a></li>
                        <li><a href="#">Manage Products</a></li>
                        <li><a href="#">Settings</a></li>
                    </>
                ) : (
                    <>
                        <li><a href="/home" className={location.pathname === "/home" ? "active" : ""}>Home</a></li>
                        <li><a href="#">My Orders</a></li>
                        <li><a href="#" onClick={onCartClick} className={location.pathname === "/cart" ? "active" : ""}>My Cart</a></li>
                        <li><a href="#">Offers</a></li>
                        <li><a href="#">My Profile</a></li>
                        <li><a href="#">Settings</a></li>
                    </>
                )}

            </ul>

        </aside>
    );
}

export default Sidebar;