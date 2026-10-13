import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";

import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";
import { getCart, updateCartItemQuantity, removeFromCart } from "../Services/CartService";
import { useToast } from "../Context/ToastContext.jsx";
import "./Cart.css";

function Cart() {

    const showToast = useToast();

    const [cartItems, setCartItems] = useState([]);
    const [loading, setLoading] = useState(false);

    const timers = useRef({});// one waiting timer per cart item

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

    const handleQuantityChange = (cartItemId, newQuantity) => {
        if (newQuantity < 1) return;

            setCartItems(prev =>// makes a copy with the new quantity
                prev.map(item => item.id === cartItemId ? { ...item, quantity: newQuantity } : item)
            );

        clearTimeout(timers.current[cartItemId]);// cancels the old timer for this item

        timers.current[cartItemId] = setTimeout(async () => {// timers.current remembers the values inside it. 10: timer123, 20: timer456
            try {// runs this code after 600ms. in between 600ms, if we click the quantity change button again then handleQuantityChange function runs again and previous timer is cancelled
                const data = await updateCartItemQuantity(cartItemId, newQuantity);

                setCartItems(prev =>// creating a new object and updating, ...item copies all properties from item.
                    prev.map(item => item.id === cartItemId ? { ...item, stock: data.stock } : item)
                );
            } catch (error) {
                alert(error.response?.data?.message || "Failed to update quantity.");
                await loadCart();
            }
        }, 600);
};

    const handleRemove = async (cartItemId) => {
        clearTimeout(timers.current[cartItemId]);
        try {
            await removeFromCart(cartItemId);
            showToast("Item removed from the Cart");
            await loadCart();
        } catch (error) {
            console.error("Error removing item:", error);
            alert(error.response?.data || "Failed to remove item.");
        }
    };

    const total = cartItems.reduce((sum, item) => sum + item.price * item.quantity, 0);
    return (
        <>
        <Header />
    <div className="page-layout">
        <Sidebar />

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