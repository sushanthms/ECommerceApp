import { useNavigate } from "react-router-dom";
import "./Header.css";

function Header({ menuOpen, setMenuOpen, darkMode, setDarkMode }) {

    const navigate = useNavigate();

    const token = localStorage.getItem("token");
    const userData = localStorage.getItem("user");

    const user = userData ? JSON.parse(userData) : null;

    const handleLogout = () => {

        localStorage.removeItem("token");
        localStorage.removeItem("user");

        navigate("/login");
    };

    return (
        <header>
            <h1>Web Page</h1>
            <button className="mobile-dashboard-toggle" onClick={() => setMenuOpen(prev => !prev)}>☰ Dashboard</button>

            <nav>   
                <a href="#">Home</a>
                <a href="#">Products</a>
                <a href="#">Blog</a>
                <a href="#">Community</a>
                {!token ? (

                    <button className="login-btn" onClick={() => navigate("/login")}>Login</button>

                ) : (
                    <>
                        <span className="user-name">{user?.name}</span>

                        <button className="logout-btn" onClick={handleLogout}>Logout</button>
                    </>

                )}
            </nav>
            
            <button className="theme-toggle" onClick={() => setDarkMode(prev => !prev)}>{darkMode ? "☀️" : "🌙"}</button>
        </header>
    );
}

export default Header;