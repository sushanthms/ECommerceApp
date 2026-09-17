import {Link, useLocation } from "react-router-dom";
import "./Sidebar.css";

function Sidebar({ menuOpen, role }) {

    const location = useLocation();

    return (
        <aside className={menuOpen ? "sidebar open" : "sidebar close"}>

            <ul className="sidebar-list">

                {role === "Admin" ? (
                    <>
                        <li><Link to="/admin" className={location.pathname === "/admin" ? "active" : ""}>Home</Link></li>
                        <li><Link to="/admin/orders" className={location.pathname === "/admin/orders" ? "active" : ""}>Manage Orders</Link></li>
                        <li><Link to="#">Manage Users</Link></li>
                        <li><Link to="/admin/products">Manage Products</Link></li>
                        <li><Link to="/admin/reviews" className={location.pathname === "/admin/reviews" ? "active" : ""}>Manage Reviews</Link></li>
                        <li><Link to="#">Settings</Link></li>
                    </>
                ) : role === "User" ? (
                    <>
                        <li><Link to="/home" className={location.pathname === "/home" ? "active" : ""}>Home</Link></li>
                        <li><Link to="/products" className={location.pathname === "/products" ? "active" : ""}>Products</Link></li>
                        <li><Link to="/orders" className={location.pathname === "/orders" ? "active" : ""}>My Orders</Link></li>
                        <li><Link to="/cart" className={location.pathname === "/cart" ? "active" : ""}>My Cart</Link></li>
                        <li><Link to="#">Offers</Link></li>
                        <li><Link to="#">My Profile</Link></li>
                        <li><Link to="#">Settings</Link></li>
                    </>
                ) : (
                    <>
                        <li><Link to="/" className={location.pathname === "/" ? "active" : ""}>Home</Link></li>
                        <li><Link to="/products" className={location.pathname === "/products" ? "active" : ""}>Products</Link></li>
                    </>
                )}

            </ul>

        </aside>
    );
}

export default Sidebar;