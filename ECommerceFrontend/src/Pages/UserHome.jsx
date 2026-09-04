import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";

import Header from "../Header.jsx";
import Sidebar from "../Sidebar.jsx";
import { getProducts, searchProducts } from "../Services/ProductService.jsx";
import { addToCart, getCart } from "../Services/CartService.jsx";

import "./UserHome.css"

function UserHome({showToast}) {// receives the showToast function that App passed. inside this file we can call showToast.
    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");
    const [products, setProducts] = useState([]);
    const [search, setSearch] = useState("");
    const [category, setCategory] = useState("");
    const [addedProducts, setAddedProducts] = useState([]);

    const userData = localStorage.getItem("user");
    const user = userData ? JSON.parse(userData) : null;

    const navigate = useNavigate();

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

    const handleSearch = (value) => {
        setSearch(value);
    };

    useEffect(() => {
    const timer = setTimeout(async () => {
        try {
            if (search.trim() === "") {
                const data = await getProducts();
                setProducts(data);
            } else {
                const data = await searchProducts(search);
                setProducts(data);
            }
        } catch (error) {
            console.error("Error searching products:", error);
        }
    }, 300);

    return () => {
        clearTimeout(timer);
    };
}, [search]);

    useEffect(() => {
        loadProducts();
    }, []);

   const handleAddToCart = async (productId) => {
    try {
        await addToCart(productId);
        showToast("Product added to cart Succesfully!");
        setAddedProducts((prev) => [...prev, productId]);
    } catch (error) {
        console.error("Error adding product to cart:", error);
        showToast(error.response?.data?.message || "Failed to add product to cart.");
    }
};

useEffect(() => {
    const loadCart = async () => {
        try {
            const data = await getCart();

            const productIds = data.map((item) => item.productId);

            setAddedProducts(productIds);
        } catch (error) {
            console.error("Error loading cart:", error);
        }
    };

    loadCart();
}, []);

    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="User" onCartClick={() => navigate("/cart")}/>
              
            <div className="page-layout">
                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} onCartClick={() => navigate("/cart")}/>
                
                <main className="main-content">

                    <div className="welcome-section">
                        <h2>Welcome, {user?.name}! 👋</h2>
                        <p>Discover products, manage your orders, and enjoy exclusive offers.</p>
                    </div>

                    <div className="search-section">
                        <input type="text" value={search} onChange={(e) => handleSearch(e.target.value)} placeholder="Search products or categories..."/>
                    </div>

                    <div className="filter-section">
                        <select value={category} onChange={(e) => setCategory(e.target.value)}>
                            <option value="">All Categories</option>

                            {[...new Set(products.map((product) => product.category))]
                                .filter(Boolean)
                                .map((cat) => (
                                    <option key={cat} value={cat}>{cat}</option>
                                ))}
                        </select>
                    </div>

                    <div className="featured-section">
                        <div className="section-heading">
                            <h2>Featured Products</h2>
                        </div>

                        {products.length === 0 ? (
                            <p>No products found.</p>
                        ) : (
                            <div className="product-grid">
                                {products.filter(
                                    (product) =>category === "" || product.category === category).map((product) => (
                                    <div key={product.id} className="product-card" onClick={() => navigate(`/product/${product.id}`)}>
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
                                            <span className="product-fallback" style={{ display: product.imageUrl ? "none" : "flex" }}>📦</span>
                                        </div>

                                        <h3>{product.name}</h3>
                                        <p>{product.description}</p>
                                        <strong>₹{product.price}</strong>
                                        <p>Stock: {product.stock}</p>
                                        <p>Category: {product.category}</p>

                                        <button onClick={(e) => {e.stopPropagation();
                                            if (addedProducts.includes(product.id)) {
                                                navigate("/cart");
                                            } else {
                                                handleAddToCart(product.id);
                                            }
                                            }}>
                                                {addedProducts.includes(product.id)? "Go to Cart": "Add to Cart"}
                                        </button>
                                        
                                        
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