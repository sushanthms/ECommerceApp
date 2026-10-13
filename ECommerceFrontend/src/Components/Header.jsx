import { useEffect, useState } from "react";
import { Link, useNavigate, useSearchParams, useLocation } from "react-router-dom";
import { useAuth } from "../Context/AuthContext.jsx";
import { useView } from "../Context/ViewContext.jsx";
import { useNotifications } from "../Context/NotificationContext.jsx";
import "./Header.css";

function Header() {

    const navigate = useNavigate();// used when javascript redirects based on conditions
    const location = useLocation();

    const isHome = location.pathname === "/home";
    const searchPage = isHome ? "/home" : "/products";
    
    const { user, isLoggedIn, logout } = useAuth();
    const { menuOpen, setMenuOpen, darkMode, setDarkMode } = useView();
    const { unreadCount } = useNotifications();

    const [searchParams] = useSearchParams();
    const [search, setSearch] = useState(searchParams.get("search") || "");

    const firstLetter = user?.name?.charAt(0).toUpperCase();

    useEffect(() => {
        if (location.pathname !== "/products" && location.pathname !== "/home") return;

        const timer = setTimeout(() => {
            const query = search.trim();

            if (query === (searchParams.get("search") || "")) return;
// we are in the products page, browser history has products page, if we type smartphone, without replace:true
// browser history adds smartphone after products, clicking back moves to products page
// with replace true, when we type smartphone in the products page, browser history replaces products with /products/?search=smartphone
// clicking back goes to the page we were there before visiting to the  products page
            navigate(
                query ? `${location.pathname}?search=${encodeURIComponent(query)}` : location.pathname,
                { replace: true }
            );
        }, 400);

        return () => clearTimeout(timer);
    }, [search]);

    const handleSearch = (e) => {
        e.preventDefault();
        const query = search.trim();
        navigate(query ? `${searchPage}?search=${encodeURIComponent(query)}` : searchPage);
    };

    const handleLogout = () => {
        logout();
        navigate("/login");
    };

    return (
        <header className="site-header">

            <div className="header-left">
                <Link to="/" className="site-logo">Smart Bazar</Link>
                <button type="button" className="hamburger" aria-label="Toggle menu" aria-expanded={menuOpen} onClick={() => setMenuOpen(!menuOpen)}>☰</button>
            </div>

            <form className="header-search" role="search" onSubmit={handleSearch}>
                <input type="search" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Search products..." aria-label="Search products" maxLength={100}/>
                <button type="submit" aria-label="Search">🔍</button>
            </form>

            <nav className="site-nav">
                <Link to="/">Home</Link>
                <Link to="/products">Products</Link>

                {user?.role === "User" && (<Link to="/community">Community</Link>)}
                {user?.role === "User" && (<Link to="/cart">🛒 Cart</Link>)}
                {user?.role === "Admin" && (<Link to="/admin/notifications" className="bell">🔔{unreadCount > 0 && <span className="bell-badge">{unreadCount}</span>}</Link>)}
                <button className="theme-toggle" aria-label={darkMode ? "Switch to light mode" : "Switch to dark mode"} onClick={() => setDarkMode(!darkMode)}>{darkMode ? "☀️" : "🌙"}</button>

                <Link to="/profile" className="profile-link" aria-label="Profile">{firstLetter || "👤"}</Link>

                {isLoggedIn ? (<button type="button" className="logout-btn" onClick={handleLogout}>Logout</button>
                ) : (<button type="button" className="login-btn" onClick={() => navigate("/login")}>Login</button>
                )}

            </nav>

        </header>
    );
}

export default Header;