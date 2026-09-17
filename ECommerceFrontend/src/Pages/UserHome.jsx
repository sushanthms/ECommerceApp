import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";

import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";
import { getBanners } from "../Services/BannerService.jsx";
import { searchProducts } from "../Services/ProductService.jsx";

import "./UserHome.css";

function UserHome({ showToast }) {

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const [search, setSearch] = useState("");
    const [searchResults, setSearchResults] = useState([]);
    const [banners, setBanners] = useState([]);

    const userData = localStorage.getItem("user");
    const user = userData ? JSON.parse(userData) : null;

    const token = localStorage.getItem("token");
    const role = token ? user?.role : null;

    const navigate = useNavigate();

    useEffect(() => {
        document.documentElement.setAttribute("data-theme", darkMode ? "dark" : "light");
    }, [darkMode]);

    const handleSearch = (value) => {
        setSearch(value);
    };

    useEffect(() => {
    const timer = setTimeout(async () => {
        try {
            if (search.trim() === "") {
                setSearchResults([]);
                return;
            }

            const data = await searchProducts(search);
            setSearchResults(data.products);
        } catch (error) {
            console.error("Error searching products:", error);
            setSearchResults([]);
        }
    }, 300);

    return () => {
        clearTimeout(timer);
    };
}, [search]);

    useEffect(() => {
    const loadBanners = async () => {
        try {
            const data = await getBanners();
            setBanners(data);
        } catch (error) {
            console.error("Error loading banners:", error);
        }
    };

    loadBanners();
}, []);

    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role={role} onCartClick={() => navigate("/cart")} search={search} setSearch={setSearch} onSearch={() => handleSearch(search)} />
            <div className="page-layout">

                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} onCartClick={() => navigate("/cart")} role={role}/>

                <main className="main-content">

                    <div className="welcome-section">
                        <h2>{user ? `Welcome, ${user.name}! 👋` : "Welcome to Smart Bazar! 👋"}</h2>
                    </div>

                    {search.trim() !== "" && (
                        <div className="search-results-section">
                            <h2>Search Results</h2>
                            {searchResults.length === 0 ? (
                                <p>No products found.</p>
                            ) : (
                            <div className="search-results-grid">
                                {searchResults.map((product) => (
                                    <div key={product.id} className="search-result-card" onClick={() => navigate(`/product/${product.id}`)}>
                                        <div className="search-result-image">
                                            {product.images?.length > 0 ? (
                                                <img src={`${import.meta.env.VITE_API_URL.replace("/api", "")}${product.images[0].imageUrl}`} alt={product.name} loading="lazy"/>
                                            ) : (
                                            <span className="product-fallback">📦</span>
                                            )}
                                            </div>
                                            <h3>{product.name}</h3>
                                            <strong>₹{product.price}</strong>
                                            <p>Category: {product.category}</p>

                                            <button
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    navigate(`/product/${product.id}`);
                                                }}
                                            >
                                                View Product
                                            </button>

                                        </div>
                                    ))}

                                </div>
                            )}

                        </div>
                    )}

                    <div className="user-banner-slider">
                        <div className="user-banner-track">
                            {[...banners, ...banners].map((banner, i) => (
                                <div className="user-banner" key={`${banner.id}-${i}`} onClick={() => navigate(banner.link)}>
                                    <img src={`${import.meta.env.VITE_API_URL.replace("/api", "")}${banner.imageUrl}`} alt={banner.title} />
                                    <div className="user-banner-content">
                                        <h2>{banner.title}</h2>
                                        <p>{banner.description}</p>
                                        <button>{banner.buttonText}</button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="categories-section">

                        <div className="section-heading">
                            <div>
                                <h2>Shop by Category</h2>
                                <p>Explore products from differentcategories.</p>
                            </div>
                        </div>


                        <div className="categories-grid">

                            <div className="category-card" onClick={() =>navigate("/products?category=Furniture")}>
                                <div className="category-icon">🛋️</div>
                                <h3>Furniture</h3>
                                <p>Explore furniture for your home.</p>
                            </div>

                            <div className="category-card" onClick={() =>navigate("/products?category=Beauty")}>
                                <div className="category-icon">👕</div>
                                <h3>Fashion</h3>
                                <p>Discover the latest fashion products.</p>
                            </div>

                            <div className="category-card" onClick={() =>navigate("/products?category=Electronics")}>
                                <div className="category-icon">📱</div>
                                <h3>Electronics</h3>
                                <p>Find useful electronic products.</p>
                            </div>

                            <div className="category-card" onClick={() =>navigate("/products?category=Home,Electronics,Beauty,Furniture")}>

                                <div className="category-icon">🏠</div>
                                <h3>Home</h3>
                                <p>Everything you need for your home.</p>
                            </div>
                        </div>

                    </div>


                    <div className="home-section">
                        <h2>Explore Our Products</h2>
                        <p>Browse our complete collection of products.</p>
                        <button onClick={() =>navigate("/products")}>View All Products</button>
                    </div>

                </main>

            </div>
        </>
    );
}

export default UserHome;