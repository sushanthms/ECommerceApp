import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getProducts } from "../Services/ProductService.jsx";
import { addToCart, getCart, updateCartItemQuantity } from "../Services/CartService.jsx";
import "./ProductDetails.css";
import Header from "../Header.jsx";
import Sidebar from "../Sidebar.jsx";

function ProductDetails({showToast}) {

    const { id } = useParams();
    const navigate = useNavigate();

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const [product, setProduct] = useState(null);
    const [addedToCart, setAddedToCart] = useState(false);
    const [cartItem, setCartItem] = useState(null);
    const [quantity, setQuantity] = useState(1);

    useEffect(() => {
        const loadProduct = async () => {// loads the product whose ID is in the URL.
            try {
                const products = await getProducts();

                const foundProduct = products.find(
                    p => p.id === Number(id)
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

            const existingItem = cart.find(
                item => item.productId === Number(id)
            );

            if (existingItem) {
                setAddedToCart(true);
                setCartItem(existingItem);
                setQuantity(existingItem.quantity);
            } else {
                setAddedToCart(false);
                setCartItem(null);
                setQuantity(1);
            }

        } catch (error) {
            console.error("Error checking cart:", error);
        }
    };

    checkCart();
}, [id]);

    const handleAddToCart = async () => {
        try {
            await addToCart(product.id, quantity);
            setAddedToCart(true);
            showToast("Added to cart", "success");
        } catch (error) {
            alert(
                error.response?.data?.message ||
                "Failed to add product to cart."
            );
        }
    };

    if (!product) {
        return <p>Product not found.</p>;
    }

    const handleQuantityChange = async (newQuantity) => {

    if (newQuantity < 1 || newQuantity > product.stock) {
        return;
    }

    if (!cartItem) {    
        setQuantity(newQuantity);
        return;
    }

    try {

        await updateCartItemQuantity(
            cartItem.id,
            newQuantity
        );

        setQuantity(newQuantity);

        setCartItem({
            ...cartItem,
            quantity: newQuantity
        });// updates the cartItem state with the new quantity

    } catch (error) {

        console.error("Error updating quantity:", error);

        alert(
            error.response?.data?.message ||
            "Failed to update quantity."
        );
    }
};
    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="User" onCartClick={() => navigate("/cart")} />
            <div className="page-layout">
                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} onCartClick={() => navigate("/cart")}/>
           
            <div className="product-details-page">

                <button className="back-btn" onClick={() => navigate("/home")} >← Back to Products</button>

                <div className="product-details">

                    <div className="product-details-image">
                        {product.imageUrl ? (
                            <img src={product.imageUrl} alt={product.name}/>
                        ) : (
                        <span>📦</span>
                        )}
                    </div>

                    <div className="product-details-info">

                        <h1>{product.name}</h1>

                        <div className="rating">★★★★★<span>4.5 | 128 Ratings</span></div>

                        <p className="product-description">{product.description}</p>

                        <div className="price">₹{product.price}<span>10% off</span></div>

                        <p className="tax">Inclusive of all taxes</p>

                        <p className="stock">✓ {product.stock - (cartItem?.quantity || 0)} more items available</p>

                        <div className="quantity"><span>Quantity:</span>

                            <button disabled={quantity === 1} onClick={() =>handleQuantityChange((quantity - 1))}>−</button>

                            <span>{quantity}</span>

                           <button disabled={quantity === product.stock} onClick={() => handleQuantityChange(quantity + 1)}>+</button>
                        </div>

                        <div className="product-buttons">

                            <button className="add-cart-btn" onClick={() => {
                                if (addedToCart) {
                                    navigate("/cart");
                                } else {
                                    handleAddToCart();
                                    }
                                }}
                            >🛒
                            {addedToCart ? "Go to Cart" : "Add to Cart"}
                            </button>

                            <button className="buy-btn" onClick={async () => {
                                if (!addedToCart) {
                                        await addToCart(product.id, quantity);
                                    }
                                    navigate("/cart");
                                }}
                                >Buy Now
                            </button>

                        </div>

                        <div className="delivery-info">

                            <div>
                                <strong>🚚 Free Delivery</strong>
                                <p>Fast delivery to your doorstep</p>
                            </div>

                            <div>
                                <strong>🔄 Easy Returns</strong>
                                <p>Easy return and replacement</p>
                            </div>

                            <div>
                                <strong>🔒 Secure Payment</strong>
                                <p>100% secure payment</p>
                            </div>

                        </div>

                    </div>
                </div>

                <div className="product-section">

                    <h2>Product Details</h2>

                    <p>{product.description}</p>

                    <div className="product-info">

                        <div>
                            <span>Price</span>
                            <strong>₹{product.price}</strong>
                        </div>

                        <div>
                            <span>Availability</span>
                            <strong>{product.stock > 0? "In Stock": "Out of Stock"}</strong>
                        </div>

                    </div>

                </div>

                <div className="product-section">

                    <h2>Customer Reviews</h2>

                    <div className="review">

                        <div>★★★★★</div>
                        <strong>Great product</strong>
                        <p>Good quality product and works as expected.</p>

                    </div>
                </div>
            </div>
            </div>
        </>
    );
}

export default ProductDetails;