import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import "./Header.css";

function Header({ darkMode, setDarkMode, role, menuOpen, setMenuOpen, onCartClick, search, setSearch, onSearch }) {

    const navigate = useNavigate();

    const token = localStorage.getItem("token");
    const userData = localStorage.getItem("user");

    const user = userData ? JSON.parse(userData) : null;

    const firstLetter = user?.name?.charAt(0).toUpperCase();

    useEffect(() => {
        document.documentElement.setAttribute("data-theme", darkMode ? "dark" : "light");

        localStorage.setItem("theme", darkMode ? "dark" : "light"
        );
    }, [darkMode]);

    const handleLogout = () => {
        localStorage.removeItem("token");
        localStorage.removeItem("user");
        navigate("/login");
    };

    return (
        <header className="site-header">

            <div className="header-left">
                <Link to="/" className="site-logo">Smart Bazar</Link>
                <button className="hamburger" onClick={() => setMenuOpen(!menuOpen)}>☰</button>
            </div>

            <div className="header-search">
                <input type="text" value={search} onChange={(e) => setSearch(e.target.value)}placeholder="Search products..."/>
                <button onClick={onSearch}>🔍</button>
            </div>

            <nav className="site-nav">
                <Link to="/products">Products</Link>

                {role === "User" && (<Link to="/community">Community</Link>)}
                {role === "User" && (<Link to="/cart">🛒 Cart</Link>)}
                <button className="theme-toggle" onClick={() => setDarkMode(!darkMode)}>{darkMode ? "☀️" : "🌙"}</button>

                {role ? (
                    <>
                        <Link to="/profile" className="profile-link">{firstLetter || "👤"}</Link>
                        <button className="logout-btn" onClick={handleLogout}>Logout</button>
                    </>
                ) : (
                    <>
                        <Link to="/profile" className="profile-link">{firstLetter || "👤"}</Link>
                        <button className="login-btn" onClick={() => navigate("/login")}>Login</button>
                    </>
                )}

            </nav>

        </header>
    );
}

export default Header;