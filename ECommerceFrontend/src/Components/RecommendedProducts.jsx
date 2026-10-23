import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getRecommendations } from "../Services/ProductService.js";
import WishlistButton from "./WishListButton.jsx";
import "./RecommendedProducts.css";

function RecommendedProducts({ productId, openLoginPopup, title = "You may also like" }) {
    const navigate = useNavigate();
    const [products, setProducts] = useState([]);

    useEffect(() => {
        if (!productId) return;

        // if the user moves to another product before the request finishes, the old answer is ignored. Without it, a slow response could overwrite the new product’s row.
        let ignore = false;// avoids setting state from an old request

        getRecommendations(productId, 10)
            .then((data) => { if (!ignore) setProducts(data); })
            .catch((error) => console.error("Error loading recommendations:", error));

        return () => { ignore = true; };
    }, [productId]);

    if (products.length === 0) return null;// shows nothing, not an empty box

    return (
        <section className="recommended-section">
            <h2>{title}</h2>

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

export default RecommendedProducts;