import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../Context/AuthContext.jsx";
import { getRecentProducts } from "../Services/ProductService";
import "./RecommendedProducts.css";

function RecentProducts() {
    const navigate = useNavigate();
    const { user, isLoggedIn } = useAuth();
    const [products, setProducts] = useState([]);

    const isUser = isLoggedIn && user?.role === "User";

    useEffect(() => {
        if (!isUser) {
            setProducts([]);
            return;
        }

        getRecentProducts(10)
            .then(setProducts)
            .catch((error) => console.error("Error loading recent products:", error));
    }, [isUser, user?.userId]);

    if (products.length === 0) return null;

    return (
        <section className="recommended-section">
            <h2>Recently viewed</h2>

            <div className="recommended-row">
                {products.map((product) => (
                    <div key={product.id} className="recommended-card" onClick={() => navigate(`/product/${product.id}`)}>
                        <div className="recommended-image">
                            {product.images?.length > 0 ? (
                                <img
                                    src={`${import.meta.env.VITE_API_URL.replace("/api", "")}${product.images[0].imageUrl}`}
                                    alt={product.name}
                                    loading="lazy"
                                />
                            ) : (
                                <span>📦</span>
                            )}
                        </div>
                        <h3>{product.name}</h3>
                        <strong>₹{product.price}</strong>
                    </div>
                ))}
            </div>
        </section>
    );
}

export default RecentProducts;