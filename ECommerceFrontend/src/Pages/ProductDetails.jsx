import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getProductById } from "../Services/ProductService";
import { addToCart, getCart, updateCartItemQuantity  } from "../Services/CartService";
import { addReview, getProductReviews } from "../Services/ReviewService";
import "./ProductDetails.css";
import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";

function ProductDetails({showToast, openLoginPopup}) {

    const { id } = useParams();
    const navigate = useNavigate();

    const token = localStorage.getItem("token");
    const userData = localStorage.getItem("user");
    const user = userData ? JSON.parse(userData) : null;
    console.log(user);

    const isLoggedIn = !!token;
    const role = token ? user?.role : null;

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const [product, setProduct] = useState(null);
    const [originalStock, setOriginalStock] = useState(0);
    const [currentImageIndex, setCurrentImageIndex] = useState(0);
    const [addedToCart, setAddedToCart] = useState(false);
    const [cartItem, setCartItem] = useState(null);

    const [quantity, setQuantity] = useState(() => {
        if (!isLoggedIn) return 1;

        const savedQuantity = localStorage.getItem(`productQuantity_${user?.userId}_${id}`);

        return savedQuantity ? Number(savedQuantity) : 1;
    });

    const [reviews, setReviews] = useState([]);
    const [rating, setRating] = useState(5);
    const [comment, setComment] = useState("");

    useEffect(() => {
    const loadProduct = async () => {
        try {
            const data = await getProductById(id);
            setProduct(data);
            setOriginalStock(data.stock);
            setCurrentImageIndex(0);
        } catch (error) {
            console.error("Error loading product:", error);
        }
    };

    loadProduct();
}, [id]);

    useEffect(() => {
    if (!isLoggedIn) {
        setAddedToCart(false);
        setCartItem(null);
        setQuantity(1);
        return;
    }

    const checkCart = async () => {
        try {
            const cart = await getCart();

            const existingItem = cart.find(
                item => item.productId === Number(id)
            );

            if (existingItem) {
                setAddedToCart(true);
                setCartItem(existingItem);

                const savedQuantity = localStorage.getItem(`productQuantity_${user?.userId}_${id}`);

                setQuantity(savedQuantity ? Number(savedQuantity) : existingItem.quantity);
            } else {
                setAddedToCart(false);
                setCartItem(null);

                const savedQuantity = localStorage.getItem(`productQuantity_${user?.userId}_${id}`);

                setQuantity(savedQuantity ? Number(savedQuantity) : 1);
            }

        } catch (error) {
            console.error("Error checking cart:", error);
        }
    };

    checkCart();
}, [id, isLoggedIn, user?.userId]);

useEffect(() => {
    const loadReviews = async () => {
        try {
            const data = await getProductReviews(id);
            setReviews(data);
        } catch (error) {
            console.error("Error loading reviews:", error);
        }
    };

    loadReviews();
}, [id]);

    const addToCartAfterLogin = async () => {
    try {
        const data = await addToCart(product.id, quantity);

        setProduct(prev => ({
            ...prev,
            stock: data.stock
        }));

        setOriginalStock(data.stock);

        // Gets the updated cart item so we have its actual cartItem.id
        const cart = await getCart();

        const updatedCartItem = cart.find(
            item => item.productId === Number(id)
        );

        setCartItem(updatedCartItem);
        setAddedToCart(true);

        showToast("Added to cart", "success");

    } catch (error) {
        showToast(
            error.response?.data?.message || "Failed to add product to cart.",
            "error"
        );
    }
};

const handleAddToCart = async () => {
    if (!isLoggedIn) {
        openLoginPopup(addToCartAfterLogin);
        return;
    }

    await addToCartAfterLogin();
};

const handleUpdateCart = async () => {
    if (!cartItem) {
        showToast("Cart item not found.", "error");
        return;
    }

    try {
        const data = await updateCartItemQuantity(
            cartItem.id,
            quantity
        );

        setProduct(prev => ({
            ...prev,
            stock: data.stock
        }));

        setOriginalStock(data.stock);

        setCartItem(prev => ({
            ...prev,
            quantity: quantity
        }));

        showToast("Cart updated successfully", "success");

    } catch (error) {
        showToast(error.response?.data?.message || "Failed to update cart.","error");
    }
};

const buyNowAfterLogin = async () => {
    try {
        if (cartItem) {
            const data = await updateCartItemQuantity(cartItem.id,quantity);

            setProduct(prev => ({...prev, stock: data.stock}));

            setOriginalStock(data.stock);

        } else {
            const data = await addToCart(product.id,quantity);

            setProduct(prev => ({...prev,stock: data.stock}));

            setOriginalStock(data.stock);
        }

        navigate("/cart");

    } catch (error) {
        showToast(error.response?.data?.message || "Unable to process Buy Now.","error");
    }
};

const handleBuyNow = async () => {
    if (!isLoggedIn) {
        openLoginPopup(buyNowAfterLogin);
        return;
    }

    await buyNowAfterLogin();
};

const handleSubmitReview = async () => {

    if (!isLoggedIn) {
        openLoginPopup();
        return;
    }

    if (!comment.trim()) {
        showToast("Please enter a review.", "warning");
        return;
    }

    try {
        const newReview = await addReview(product.id, rating, comment);

        setReviews([newReview, ...reviews]);// makes a new array, puts the newreview first(prepends) then puts all the old reviews after it

        setRating(5);// makes star selector back to 5
        setComment("");// clears the textarea

        showToast("Review added successfully", "success");

    } catch (error) {
        showToast(error.response?.data || "Failed to add review.","error");
    }
};

useEffect(() => {
    if (!isLoggedIn) return;

    localStorage.setItem(`productQuantity_${user?.userId}_${id}`,quantity);
}, [quantity, id, isLoggedIn, user?.userId]);

    if (!product) {
        return <p>Product not found.</p>;
    }

    const averageRating = reviews.length > 0 ? reviews.reduce((sum, review) => sum + review.rating, 0) / reviews.length: 0;

    const handleQuantityChange = (newQuantity) => {

    const cartQuantity = cartItem?.quantity || 0;

    const maxQuantity = cartQuantity + originalStock;

    if (newQuantity < 1 || newQuantity > maxQuantity) {
        return;
    }

    setQuantity(newQuantity);
};

const cartQuantity = cartItem?.quantity || 0;

const availableStock = originalStock - (quantity - cartQuantity);// for temporary calculation

const images = product.images || [];

const handlePreviousImage = () => {
    setCurrentImageIndex((currentImageIndex - 1 + images.length) % images.length);
};

const handleNextImage = () => {
    setCurrentImageIndex((currentImageIndex + 1) % images.length);
};

    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role={role} onCartClick={() => navigate("/cart")} />
            <div className="page-layout">
                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} onCartClick={() => navigate("/cart")} role={role}/>
           
            <div className="product-details-page">

                <button className="back-btn" onClick={() => navigate("/products")}>← Back to Products</button>

                <div className="product-details">

                    <div className="product-details-gallery">

                            <div className="product-details-image">

                                {images.length > 0 ? (
                                    <img
                                        src={`${import.meta.env.VITE_API_URL.replace("/api", "")}${images[currentImageIndex].imageUrl}`}
                                        alt={product.name}
                                    />
                                ) : (
                                    <span>📦</span>
                                )}

                            </div>

                            {images.length > 1 && (
                                <div className="image-navigation">

                                    <button onClick={handlePreviousImage}>
                                        ← Previous
                                    </button>

                                    <span>
                                        {currentImageIndex + 1} / {images.length}
                                    </span>

                                    <button onClick={handleNextImage}>
                                        Next →
                                    </button>

                                </div>
                            )}

                            {images.length > 1 && (
                                <div className="image-thumbnails">

                                    {images.map((image, index) => (
                                        <img
                                            key={image.id}
                                            src={`${import.meta.env.VITE_API_URL.replace("/api", "")}${image.imageUrl}`}
                                            alt={`${product.name} ${index + 1}`}
                                            className={index === currentImageIndex ? "active-thumbnail" : ""}
                                            onClick={() => setCurrentImageIndex(index)}
                                        />
                                    ))}

                                </div>
                            )}

                        </div>

                    <div className="product-details-info">

                        <h1>{product.name}</h1>

                        <div className="rating">
                            {"★".repeat(Math.round(averageRating))}
                            {"☆".repeat(5 - Math.round(averageRating))}
                            <span>{reviews.length > 0 ? `${averageRating.toFixed(1)} | ${reviews.length} Ratings` : "No Ratings"}</span>
                        </div>

                        <p className="product-description">{product.description}</p>

                        <div className="price">₹{product.price}<span>10% off</span></div>

                        <p className="tax">Inclusive of all taxes</p>

                        <p className="stock">✓ {availableStock > 0 ? `${availableStock} more items available` : "Out of Stock"}</p>

                        <div className="quantity"><span>Quantity:</span>

                            <button disabled={quantity === 1} onClick={() =>handleQuantityChange((quantity - 1))}>−</button>

                            <span>{quantity}</span>

                           <button disabled={availableStock === 0} onClick={() => handleQuantityChange(quantity + 1)}>+</button>
                        </div>

                        <div className="product-buttons">

                            <button className="add-cart-btn"
                            onClick={() => {
                                if (addedToCart) {
                                    handleUpdateCart();
                                } else {
                                    handleAddToCart();
                                }
                                }}
                            >
                                🛒 {addedToCart ? "Update Cart" : "Add to Cart"}
                            </button>

                            <button className="buy-btn" onClick={handleBuyNow}>Buy Now</button>

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
                            <strong>{availableStock > 0? "In Stock": "Out of Stock"}</strong>
                        </div>

                    </div>

                </div>

                <div className="product-section">

                    <h2>Customer Reviews</h2>
                    {isLoggedIn ? (
                        <div className="review-form">
                            <h3>Write a Review</h3>
                                <div className="star-input">
                                    {[1, 2, 3, 4, 5].map((star) => (// star is a specific fixed number for that button. 1 means the number for the first button
                                    // the arrow function () => setRating(star) "remembers" whatever star was at the time it was created, even after the loop has finished. Button 1's onClick is () => setRating(1)
                                    // Each button effectively "hard-codes" its own number into its click handler, even though they were all generated by the same line of code.
                                    // Now if we click 3rd star, As part of that re-run, .map() executes again — 5 brand new iterations happen (5 brand new closures get created, replacing the old ones), and all 5 ternaries get evaluated again, fresh, but now with rating = 3 this time:
                                        <button key={star} type="button" onClick={() => setRating(star)}>{star <= rating ? "★" : "☆"}</button>
                                    ))}
                                </div>

                            <textarea value={comment} onChange={(e) => setComment(e.target.value)} placeholder="Write your review..."/>
                            <button onClick={handleSubmitReview}>Submit Review</button>

                            </div>
                    ):(
                        <p>Please <button onClick={() => openLoginPopup()}>login</button> to write a review.</p>
                        )}

                            {reviews.length === 0 ? (
                                <p>No reviews yet.</p>
                            ) : (
                                reviews.map((review) => (
                                    <div className="review" key={review.id}>

                                        <div>
                                            {"★".repeat(review.rating)}
                                            {"☆".repeat(5 - review.rating)}
                                        </div>

                                        <strong>{review.userName}</strong>
                                        <p>{review.comment}</p>

                                    </div>
                                ))
                            )}

                        </div>
                        </div>
            </div>
        </>
    );
}

export default ProductDetails;