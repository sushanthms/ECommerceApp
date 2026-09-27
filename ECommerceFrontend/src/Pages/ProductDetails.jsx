import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getProductById } from "../Services/ProductService";
import { addToCart, getCart, updateCartItemQuantity  } from "../Services/CartService";
import { addReview, getProductReviews, deleteOwnReview } from "../Services/ReviewService";
import "./ProductDetails.css";
import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";

function ProductDetails({showToast, openLoginPopup}) {

    const { id } = useParams();
    const navigate = useNavigate();

    const token = localStorage.getItem("token");
    const userData = localStorage.getItem("user");
    const user = userData ? JSON.parse(userData) : null;

    const isLoggedIn = !!token;
    const role = token ? user?.role : null;

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const [product, setProduct] = useState(null);
    const [originalStock, setOriginalStock] = useState(0);
    const [currentImageIndex, setCurrentImageIndex] = useState(0);
    const [addedToCart, setAddedToCart] = useState(false);
    const [cartItem, setCartItem] = useState(null);
    const [quantity, setQuantity] = useState(1);

    const [reviews, setReviews] = useState([]);
    const [reviewsPage, setReviewsPage] = useState(1);
    const [totalReviews, setTotalReviews] = useState(0);
    const [averageRating, setAverageRating] = useState(0);
    const [loadingMoreReviews, setLoadingMoreReviews] = useState(false);
    const REVIEW_PAGE_SIZE = 10;
    const [rating, setRating] = useState(5);
    const [comment, setComment] = useState("");

    const [saving, setSaving] = useState(false);// true while a save request is running

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

                const existingItem = cart.find(item => item.productId === Number(id));

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
}, [id, isLoggedIn, user?.userId]);

useEffect(() => {
    const loadReviews = async () => {
        try {
            const data = await getProductReviews(id, 1, REVIEW_PAGE_SIZE);
            setReviews(data.reviews);
            setTotalReviews(data.totalCount);
            setAverageRating(data.averageRating); 
            setReviewsPage(1);
        } catch (error) {
            console.error("Error loading reviews:", error);
        }
    };

    loadReviews();
}, [id]);

    const addToCartAfterLogin = async () => {
    try {
        const data = await addToCart(product.id, quantity);
        // setProduct stored the product's data when the loadproduct useffect was run. So previous state was not null.
        setProduct(prev => ({...prev, stock: data.stock}));

        setOriginalStock(data.stock);

        // Gets the updated cart item so we have its actual cartItem.id
        const cart = await getCart();
        // Number(id) id means the id of the product we are in ProductDetails page
        const updatedCartItem = cart.find(item => item.productId === Number(id));

        setCartItem(updatedCartItem);// stores the data of the product in cart. id, productId, quantity, 
        setAddedToCart(true);

        showToast("Added to cart", "success");

    } catch (error) {
        showToast(error.response?.data?.message || "Failed to add product to cart.","error"
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

const buyNowAfterLogin = async () => {
    try {
        if (!cartItem) {
            await addToCart(product.id, quantity);
        } else if (cartItem.quantity !== quantity) {
            await updateCartItemQuantity(cartItem.id, quantity);
        }
        navigate("/cart");
    } catch (error) {
        showToast(error.response?.data?.message || "Unable to process Buy Now.", "error");
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

        setReviews(prev => [newReview, ...prev]);
        setTotalReviews(prev => prev + 1);
        setAverageRating(prev => (prev * totalReviews + rating) / (totalReviews + 1));

        setRating(5);
        setComment("");

        showToast("Review added successfully", "success");

    } catch (error) {
        showToast(error.response?.data?.message || "Failed to add review.", "error");
    }
};

const handleDeleteReview = async (reviewId) => {
    const confirmed = window.confirm("Are you sure you want to delete this review?");
    if (!confirmed) return;

    try {
        await deleteOwnReview(reviewId);
        const data = await getProductReviews(id, 1, REVIEW_PAGE_SIZE);
        setReviews(data.reviews);
        setTotalReviews(data.totalCount);
        setAverageRating(data.averageRating);
        setReviewsPage(1);
        showToast("Review deleted", "success");
    } catch (error) {
        showToast(error.response?.data?.message || "Failed to delete review.", "error");
    }
};

const handleLoadMoreReviews = async () => {
    setLoadingMoreReviews(true);
    try {
        const nextPage = reviewsPage + 1;
        const data = await getProductReviews(id, nextPage, REVIEW_PAGE_SIZE);

        setReviews(prev => [...prev, ...data.reviews]);// appending below, not replacing
        setReviewsPage(nextPage);
    } catch (error) {
        showToast("Failed to load more reviews.", "error");
    } finally {
        setLoadingMoreReviews(false);
    }
};

useEffect(() => {
    // Runs when the product is in cart, and quantity shown on the screen is different to exisiting quantity in the cart
    // cartItem starts as null (from useState(null))
    if (!addedToCart || !cartItem || quantity === cartItem.quantity) return;

    const timer = setTimeout(async () => {
        setSaving(true);
        try {
            const data = await updateCartItemQuantity(cartItem.id, quantity);
            setOriginalStock(data.stock);
            setCartItem({ ...cartItem, quantity });// the old quantity is updated wiith the new quantity. if the key and value variable is same we can write it once
        } catch (error) {
            showToast(error.response?.data?.message || "Failed to update cart.", "error");
            setQuantity(cartItem.quantity);// goes back to the last saved quantity
        } finally {
            setSaving(false);
        }
    }, 600);

    return () => clearTimeout(timer);// a new click cancels the waiting timer
}, [quantity, cartItem, addedToCart]);

    if (!product) {
        return <p>Product not found.</p>;
    }

const handleQuantityChange = (newQuantity) => {
    const maxQuantity = (cartItem?.quantity || 0) + originalStock;
    if (newQuantity < 1 || newQuantity > maxQuantity) return;
    setQuantity(newQuantity);// the screen changes at once, the effect above does the saving
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
                                    <button onClick={handlePreviousImage}>← Previous</button>
                                    <span>{currentImageIndex + 1} / {images.length}</span>
                                    <button onClick={handleNextImage}>Next →</button>
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
                            <span>{totalReviews > 0 ? `${averageRating.toFixed(1)} | ${totalReviews} Ratings` : "No Ratings"}</span>
                        </div>

                        <p className="product-description">{product.description}</p>

                        <div className="price">₹{product.price}<span>10% off</span></div>

                        <p className="tax">Inclusive of all taxes</p>

                        <p className="stock">✓ {availableStock > 0 ? `${availableStock} more items available` : "Out of Stock"}</p>

                        <div className="quantity"><span>Quantity:</span>

                            <button disabled={quantity === 1 || saving} onClick={() => handleQuantityChange(quantity - 1)}>−</button>
                            <span>{quantity}</span>
                            <button disabled={availableStock === 0 || saving} onClick={() => handleQuantityChange(quantity + 1)}>+</button>
                            {addedToCart && <small>{saving || quantity !== cartItem?.quantity ? "Saving..." : "Saved in cart"}</small>}
                        </div>

                        <div className="product-buttons">
                            {addedToCart ? (
                                <button className="add-cart-btn" onClick={() => navigate("/cart")}>🛒 Go to Cart</button>
                            ) : (
                                <button className="add-cart-btn" onClick={handleAddToCart}>🛒 Add to Cart</button>
                            )}

                            <button className="buy-btn" disabled={saving} onClick={handleBuyNow}>Buy Now</button>
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
                                <>
                                    {reviews.map((review) => (
                                        <div className="review" key={review.id}>
                                            <div>
                                                {"★".repeat(review.rating)}
                                                {"☆".repeat(5 - review.rating)}
                                            </div>
                                            <strong>{review.userName}</strong>
                                            <p>{review.comment}</p>
                                            {isLoggedIn && review.userId === user?.userId && (
                                                <button onClick={() => handleDeleteReview(review.id)}>Delete</button>
                                            )}
                                        </div>
                                    ))}

                                    {reviews.length < totalReviews && (
                                        <button onClick={handleLoadMoreReviews} disabled={loadingMoreReviews}>
                                            {loadingMoreReviews ? "Loading..." : `Load More (${totalReviews - reviews.length} more)`}
                                        </button>
                                    )}
                                </>
                            )}

                        </div>
                        </div>
            </div>
        </>
    );
}

export default ProductDetails;