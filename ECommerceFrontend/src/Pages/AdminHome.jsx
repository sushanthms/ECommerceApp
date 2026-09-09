import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Header from "../Header.jsx";
import Sidebar from "../Sidebar.jsx";
import AdminBanner from "../Components/AdminBanner.jsx";

import "./AdminHome.css";

function AdminHome({ showToast }) {

    const navigate = useNavigate();

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const userData = localStorage.getItem("user");

    const user = userData ? JSON.parse(userData) : null;

    useEffect(() => {
        document.documentElement.setAttribute("data-theme",darkMode ? "dark" : "light");
    }, [darkMode]);

    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="Admin"/>

            <div className="page-layout">

                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} role="Admin"/>

                <main className="main-content">

                    <div className="welcome-section">
                        <h2>Welcome, {user?.name}!</h2>
                    </div>

                            <div className="admin-actions">
                                <button className="view-products-btn" onClick={() => navigate("/admin/products")}>Manage Products</button>
                                <button className="view-orders-btn" onClick={() => navigate("/admin/orders")}>View Orders</button>
                            </div>
                            
                    <AdminBanner />
                </main>

            </div>

        </>
    );
}

export default AdminHome;