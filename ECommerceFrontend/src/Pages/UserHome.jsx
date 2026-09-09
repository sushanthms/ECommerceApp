import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";

import Header from "../Header.jsx";
import Sidebar from "../Sidebar.jsx";
import { searchProducts } from "../Services/ProductService.jsx";
import { getBanners } from "../Services/BannerService.jsx";

import "./UserHome.css";

function UserHome({ showToast }) {

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const [search, setSearch] = useState("");
    const [searchResults, setSearchResults] = useState([]);
    const [banners, setBanners] = useState([]);

    const userData = localStorage.getItem("user");
    const user = userData ? JSON.parse(userData) : null;

    const navigate = useNavigate();

    useEffect(() => {
        document.documentElement.setAttribute(
            "data-theme",
            darkMode ? "dark" : "light"
        );
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
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="User"onCartClick={() => navigate("/cart")}/>

            <div className="page-layout">

                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} onCartClick={() => navigate("/cart")} role="User"/>

                <main className="main-content">

                    <div className="welcome-section">
                        <h2>Welcome, {user?.name}! 👋</h2>
                    </div>

                    <div className="home-search-section">

                        <h2>What are you looking for?</h2>

                        <div className="home-search-box">
                            <input type="text" value={search} onChange={(e) =>handleSearch(e.target.value)} placeholder="Search for products..."/>
                            <button onClick={() => handleSearch(search)}>🔍 Search</button>
                            {search && (<button className="clear-search-btn" onClick={() => {setSearch("");setSearchResults([]);}}>✕ Clear</button>)}
                        </div>

                    </div>

                    {search.trim() !== "" && (

                        <div className="search-results-section">

                            <h2>Search Results</h2>

                            {searchResults.length === 0 ? (
                                <p>No products found.</p>
                            ) : (
                                <div className="search-results-grid">

                                    {searchResults.map((product) => (

                                        <div key={product.id} className="search-result-card" onClick={() =>navigate(`/product/${product.id}`)}>
                                            <div className="search-result-image">
                                                {product.imageUrl ? (

                                                    <img src={product.imageUrl} alt={product.name} onError={(e) => {
                                                            e.target.style.display ="none";
                                                            e.target.nextSibling.style.display ="flex";
                                                        }}
                                                    />
                                                ) : null}

                                                <span className="product-fallback" style={{display: product.imageUrl ? "none" : "flex"}}>📦</span>

                                            </div>


                                            <h3>{product.name}</h3>
                                            <strong>₹{product.price}</strong>
                                            <p>Category:{" "}{product.category}</p>
                                            <button onClick={(e) => {e.stopPropagation(); navigate(`/product/${product.id}`);}}>View Product</button>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    )}

                    <div className="user-banner-slider">
                        <div className="user-banner-track">
                            {[...banners, ...banners].map((banner, index) => (
                                <div className="user-banner" key={`${banner.id}-${index}`} onClick={() => navigate(banner.link)}>
                                    <img src={banner.imageUrl} alt={banner.title} />

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

                            <div className="category-card" onClick={() =>navigate("/products?category=Fashion")}>
                                <div className="category-icon">👕</div>
                                <h3>Fashion</h3>
                                <p>Discover the latest fashion products.</p>
                            </div>

                            <div className="category-card" onClick={() =>navigate("/products?category=Electronics")}>
                                <div className="category-icon">📱</div>
                                <h3>Electronics</h3>
                                <p>Find useful electronic products.</p>
                            </div>

                            <div className="category-card" onClick={() =>navigate("/products?category=Home%20%26%20Kitchen")}>

                                <div className="category-icon">🏠</div>
                                <h3>Home & Kitchen</h3>
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