import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import Header from "../Header.jsx";
import Sidebar from "../Sidebar.jsx";
import { getCart, updateCartItemQuantity, removeFromCart } from "../Services/CartService.jsx";
import "./Cart.css";

function Cart({showToast}) {
    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");
    const [cartItems, setCartItems] = useState([]);
    const [loading, setLoading] = useState(false);

    const navigate = useNavigate();

    const loadCart = async () => {
        setLoading(true);

        try {
            const data = await getCart();
            setCartItems(data);
        } catch (error) {
            console.error("Error loading cart:", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadCart();
    }, []);

    const handleQuantityChange = async (cartItemId, newQuantity) => {
        if (newQuantity < 1) {
            return;
        }

        try {
            await updateCartItemQuantity(cartItemId, newQuantity);
            await loadCart();
        } catch (error) {
            console.error("Error updating quantity:", error);
            alert(error.response?.data || "Failed to update quantity.");
        }
    };

    const handleRemove = async (cartItemId) => {
        try {
            await removeFromCart(cartItemId);
            showToast("Item removed from the Cart");
            await loadCart();
        } catch (error) {
            console.error("Error removing item:", error);
            alert(error.response?.data || "Failed to remove item.");
        }
    };

    const total = cartItems.reduce((sum, item) => sum + item.price * item.quantity,0);
    // 0 is the initial value of the sum.
    return (
        <>
        <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="User" onCartClick={() => navigate("/cart")}/>
    <div className="page-layout">
        <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} onCartClick={() => navigate("/cart")}/>

        <div className="cart-page">
    
            <h1>🛒 My Cart</h1>

            {loading ? (
                <p>Loading...</p>
            ) : cartItems.length === 0 ? (
                <div>
                    <p>Your cart is empty.</p>
                    <button className="continue-btn" onClick={() => navigate("/home")}>Continue Shopping</button>
                </div>
            ) : (
                <>
                    <div className="cart-items">

                        {cartItems.map((item) => (
                            <div key={item.id} className="cart-item" onClick={() => navigate(`/product/${item.productId}`)}>

                                <div className="cart-item-image">{item.imageUrl ? (
                                    <img src={item.imageUrl} alt={item.name} onError={(e) => {
                                        e.target.style.display = "none";
                                        e.target.nextSibling.style.display = "block";// next sibling means {item.imageUrl && <span style={{ display: "none" }}>📦</span>}
                                    }}/>
                                ) : (<span>📦</span>// this displays when there is no imageUrl. when image url is broken this line does not get executed
// about next line(next span). when image url is broken it gives error and the next sibling/line is made as display block.
// we wrote it none because when imageurl is present and correct span should not get displayed.
// in next line imageUrl is present so we check item.imageUrl &&. span is made block by previous line. and imageUrl is present and broken so below span is displayed
                                )}
                                {item.imageUrl && <span style={{ display: "none" }}>📦</span>}
                                </div>

                                <div className="cart-item-info">
                                    <h3>{item.name}</h3>
                                    <p>₹{item.price}</p>
                                </div>

                                <div className="cart-item-controls">

                                    <button disabled={item.quantity === 1} onClick={(e) =>{e.stopPropagation(); handleQuantityChange(item.id, item.quantity - 1)}}>-</button>

                                    <span>{item.quantity}</span>

                                    <button onClick={(e) =>{e.stopPropagation(); handleQuantityChange(item.id,item.quantity + 1)}}>+</button>

                                    <button onClick={(e) =>{e.stopPropagation(); handleRemove(item.id)}}>Remove</button>

                                </div>

                            </div>
                        ))}

                    </div>

                    <div className="cart-summary">
                        <h2>Total: ₹{total.toFixed(2)}</h2>
                        <button onClick={() => navigate("/checkout")}>Checkout</button>
                    </div>
                </>
            )}

        </div>
        </div>
        </>
    );
}

export default Cart;