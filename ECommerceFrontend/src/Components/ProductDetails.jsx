import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getProducts } from "../Services/ProductService.jsx";
import { addToCart, getCart } from "../Services/CartService.jsx";
import "./ProductDetails.css";
import Header from "../Header.jsx";

function ProductDetails() {
    const { id } = useParams();
    const navigate = useNavigate();

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");
    const [product, setProduct] = useState(null);
    const [addedToCart, setAddedToCart] = useState(false);

    useEffect(() => {
        const loadProduct = async () => {
            try {
                const products = await getProducts();

                const foundProduct = products.find(
                    (p) => p.id === Number(id)
                );

                setProduct(foundProduct);
            } catch (error) {
                console.error("Error loading product:", error);
            }
        };

        loadProduct();
    }, [id]);

    useEffect(() => {
        const checkCart = async () => {
            try {
                const cart = await getCart();

                const exists = cart.some(
                    (item) => item.productId === Number(id)
                );

                setAddedToCart(exists);
            } catch (error) {
                console.error("Error checking cart:", error);
            }
        };

        checkCart();
    }, [id]);

    const handleAddToCart = async () => {
        try {
            await addToCart(product.id);

            setAddedToCart(true);
        } catch (error) {
            console.error("Error adding product to cart:", error);

            alert(
                error.response?.data?.message ||
                error.response?.data ||
                "Failed to add product to cart."
            );
        }
    };

    if (!product) {
        return <p>Product not found.</p>;
    }

    return (
         <>
        <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="User" onCartClick={() => navigate("/cart")}/>
    
        <div className="product-details-page">

            <button
                className="back-btn"
                onClick={() => navigate("/home")}
            >
                ← Back to Products
            </button>

            <div className="product-details">

                <div className="product-details-image">
                    {product.imageUrl ? (
                        <img
                            src={product.imageUrl}
                            alt={product.name}
                        />
                    ) : (
                        <span>📦</span>
                    )}
                </div>

                <div className="product-details-info">

                    <h1>{product.name}</h1>

                    <p className="product-category">
                        Category: {product.category}
                    </p>

                    <p className="product-description">
                        {product.description}
                    </p>

                    <h2>₹{product.price}</h2>

                    <p>
                        Stock available: {product.stock}
                    </p>

                    <button
                        className="add-cart-btn"
                        onClick={() => {
                            if (addedToCart) {
                                navigate("/cart");
                            } else {
                                handleAddToCart();
                            }
                        }}
                    >
                        {addedToCart ? "Go to Cart" : "Add to Cart"}
                    </button>

                </div>

            </div>

        </div>
        </>
    );
}

export default ProductDetails;