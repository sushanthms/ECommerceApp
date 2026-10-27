import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getPopularProducts } from "../Services/ProductService.js";
import WishlistButton from "./WishListButton.jsx";
import "./RecommendedProducts.css";

function PopularProducts({ openLoginPopup }) {
    const navigate = useNavigate();
    const [products, setProducts] = useState([]);

    useEffect(() => {
        getPopularProducts(10)
            .then(setProducts)
            .catch((error) => console.error("Error loading popular products:", error));
    }, []);

    if (products.length === 0) return null;

    return (
        <section className="recommended-section">
            <h2>Popular products</h2>

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
                            <WishlistButton productId={product.id} onLoginRequired={() => openLoginPopup?.()} />
                        </div>
                        <h3>{product.name}</h3>
                        <strong>₹{product.price}</strong>
                    </div>
                ))}
            </div>
        </section>
    );
}

export default PopularProducts;