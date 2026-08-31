import { useState, useEffect } from "react";

import Header from "../Header.jsx";
import Sidebar from "../Sidebar.jsx";
import { getProducts } from "../Services/ProductService.jsx";

import "./UserHome.css"

function UserHome() {
    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(false);
    const [products, setProducts] = useState([]);

    const userData = localStorage.getItem("user");
    const user = userData ? JSON.parse(userData) : null;

    useEffect(() => {
        document.documentElement.setAttribute("data-theme", darkMode ? "dark" : "light");
    }, [darkMode]);

    const loadProducts = async () => {
        try {
            const data = await getProducts();
            setProducts(data);
        } catch (error) {
            console.error("Error loading products:", error);
        }
    };

    useEffect(() => {
        loadProducts();
    }, []);

    return (
        <>
            <Header
                menuOpen={menuOpen}
                setMenuOpen={setMenuOpen}
                darkMode={darkMode}
                setDarkMode={setDarkMode}
            />

            <div className="page-layout">
                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} />

                <main className="main-content">

                    <div className="welcome-section">
                        <h2>Welcome, {user?.name}! 👋</h2>
                        <p>Discover products, manage your orders, and enjoy exclusive offers.</p>
                    </div>

                    <div className="quick-actions">
                        <div className="action-card">
                            <h3>🛍️ Shop Products</h3>
                            <p>Browse our latest products.</p>
                            <button>Shop Now</button>
                        </div>
                        <div className="action-card">
                            <h3>📦 My Orders</h3>
                            <p>View and track your orders.</p>
                            <button>View Orders</button>
                        </div>
                        <div className="action-card">
                            <h3>🛒 My Cart</h3>
                            <p>Check the items in your cart.</p>
                            <button>View Cart</button>
                        </div>
                        <div className="action-card">
                            <h3>🎁 Offers</h3>
                            <p>Explore today's special offers.</p>
                            <button>View Offers</button>
                        </div>
                    </div>

                    <div className="featured-section">
                        <div className="section-heading">
                            <h2>Featured Products</h2>
                            <a href="#">View All</a>
                        </div>

                        {products.length === 0 ? (
                            <p>No products found.</p>
                        ) : (
                            <div className="product-grid">
                                {products.map((product) => (
                                    <div key={product.id} className="product-card">
                                        <div className="product-image">
                                            {product.imageUrl ? (
                                                <img
                                                    src={product.imageUrl}
                                                    alt={product.name}
                                                    onError={(e) => {
                                                        e.target.style.display = "none";
                                                        e.target.nextSibling.style.display = "flex";
                                                    }}
                                                />
                                            ) : null}
                                            <span
                                                className="product-fallback"
                                                style={{ display: product.imageUrl ? "none" : "flex" }}
                                            >
                                                📦
                                            </span>
                                        </div>

                                        <h3>{product.name}</h3>
                                        <p>{product.description}</p>
                                        <strong>₹{product.price}</strong>
                                        <p>Stock: {product.stock}</p>
                                        <p>Category: {product.category}</p>
                                        <button>Add to Cart</button>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>

                </main>
            </div>
        </>
    );
}

export default UserHome;