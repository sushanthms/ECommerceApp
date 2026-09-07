import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import "./Header.css";

function Header({ menuOpen, setMenuOpen, darkMode, setDarkMode, role, onCartClick }) {

    const navigate = useNavigate();

    const token = localStorage.getItem("token");
    const userData = localStorage.getItem("user");

    const user = userData ? JSON.parse(userData) : null;

    useEffect(() => {
        document.documentElement.setAttribute(
            "data-theme",
            darkMode ? "dark" : "light"
        );

        localStorage.setItem("theme", darkMode ? "dark" : "light");
    }, [darkMode]);

    const handleLogout = () => {
        localStorage.removeItem("token");
        localStorage.removeItem("user");

        navigate("/login");
    };

    return (
        <header>
            <h1>Web Page</h1>

            

            <nav>
                {role === "Admin" ? (<a href="#" onClick={(e) => { e.preventDefault(); navigate("/admin"); }}>Home</a>) : (<a href="#" onClick={(e) => { e.preventDefault(); navigate("/home"); }}>Home</a>)}
                {role === "User" && (<a href="#" onClick={(e) => { e.preventDefault(); navigate("/products"); }}>Products</a>)}
                {role === "User" && (<a href="#" onClick={(e) => { e.preventDefault(); navigate("/community"); }}>Community</a>)}

                {role === "User" && (
                    <a href="#" onClick={(e) => { e.preventDefault();onCartClick();}}>🛒 Cart</a>
                )}

                {!token ? (
                    <button className="login-btn" onClick={() => navigate("/login")}>Login</button>
                ) : (
                    <>
                        <span className="user-name">{user?.name}</span>
                        <button className="logout-btn" onClick={handleLogout}>Logout</button>
                    </>
                )}
            </nav>

            <button className="theme-toggle" onClick={() => setDarkMode(prev => !prev)} > {darkMode ? "☀️" : "🌙"}</button>
        </header>
    );
}

export default Header;