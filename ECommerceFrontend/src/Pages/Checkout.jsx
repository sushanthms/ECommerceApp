import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";
import { getCart } from "../Services/CartService.jsx";
import "./Checkout.css";

const API_URL = `${import.meta.env.VITE_API_URL}/Order`;

function Checkout({ showToast }) {
    console.log("Checkout Page Rendered");
    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");
    const [cartItems, setCartItems] = useState([]);
    const [loading, setLoading] = useState(false);
    const [errors, setErrors] = useState({});
    const [paymentMethod, setPaymentMethod] = useState("Cash on Delivery");
    const [showPayment, setShowPayment] = useState(false);
    const [paymentProcessing, setPaymentProcessing] = useState(false);

    const [cardNumber, setCardNumber] = useState("");
    const [expiry, setExpiry] = useState("");
    const [cvv, setCvv] = useState("");

    const navigate = useNavigate();

    const [customerDetails, setCustomerDetails] = useState({
        fullName: "",
        phone: "",
        address: "",
        city: "",
        state: "",
        pincode: ""
    });

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

    const validateForm = () => {
        const newErrors = {};

        if (!customerDetails.fullName.trim()) {
            newErrors.fullName = "Full name is required.";
        }

        if (!/^\d{10}$/.test(customerDetails.phone)) {
            newErrors.phone = "Phone number must be 10 digits.";
        }

        if (!customerDetails.address.trim()) {
            newErrors.address = "Address is required.";
        }

        if (!customerDetails.city.trim()) {
            newErrors.city = "City is required.";
        }

        if (!customerDetails.state.trim()) {
            newErrors.state = "State is required.";
        }

        if (!/^\d{6}$/.test(customerDetails.pincode)) {
            newErrors.pincode = "Pincode must be 6 digits.";
        }

        setErrors(newErrors);

        return Object.keys(newErrors).length === 0;
    };

    const placeOrder = async () => {
        try {
            setLoading(true);

            const token = localStorage.getItem("token");

            const response = await axios.post(
                API_URL,
                {
                    ...customerDetails,
                    paymentMethod
                },
                {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                }
            );

            showToast(response.data.message);

            navigate("/home");
        } catch (error) {
            console.error("Order error:", error);
            alert(error.response?.data?.message || "Failed to place order.");
        } finally {
            setLoading(false);
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        if (paymentMethod === "Online Payment") {
            setShowPayment(true);
            return;
        }

        await placeOrder();
    };

    const handlePayment = async (e) => {
        e.preventDefault();

        if (cardNumber.length !== 16) {
            alert("Card number must be 16 digits.");
            return;
        }

        if (!/^\d{2}\/\d{2}$/.test(expiry)) {
            alert("Expiry must be in MM/YY format.");
            return;
        }

        if (cvv.length !== 3) {
            alert("CVV must be 3 digits.");
            return;
        }

        setPaymentProcessing(true);

        setTimeout(async () => {
            try {
                const token = localStorage.getItem("token");

                const response = await axios.post(
                    `${API_URL}/Pay`,
                    {
                        ...customerDetails,
                        paymentMethod: "Online Payment"
                    },
                    {
                        headers: {
                            Authorization: `Bearer ${token}`
                        }
                    }
                );

                showToast(response.data.message);
                navigate("/home");
            } catch (error) {
                console.error("Payment error:", error);
                alert(error.response?.data?.message || "Payment failed.");
            } finally {
                setPaymentProcessing(false);
            }
        }, 1500);
    };

    const handleChange = (e) => {
        const { name, value } = e.target;
        // this is 
        // const name = e.target.name;
        // const value = e.target.value;
        setCustomerDetails({...customerDetails, [name]: value});
    };

    const total = cartItems.reduce((sum, item) => sum + item.price * item.quantity, 0);

    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="User" onCartClick={() => navigate("/cart")}/>

            <div className="page-layout">

                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} onCartClick={() => navigate("/cart")}/>

                <div className="checkout-page">

                    <h1>Checkout</h1>

                    {showPayment ? (
                        <div className="payment-page">
                            <h2>Online Payment</h2>
                            <p>Total Amount: ₹{total.toFixed(2)}</p>

                            <form onSubmit={handlePayment}>

                                <div className="form-group">
                                    <label>Card Number</label>
                                    <input type="text" value={cardNumber} onChange={(e) => setCardNumber(e.target.value.replace(/\D/g, "").slice(0, 16))} placeholder="Enter 16 digit card number"/>
                                </div>

                                <div className="form-row">
                                    <div className="form-group">
                                        <label>Expiry</label>
                                        <input type="text" value={expiry} onChange={(e) => setExpiry(e.target.value.slice(0, 5))} placeholder="MM/YY"/>
                                    </div>
                                    <div className="form-group">
                                        <label>CVV</label>
                                        <input type="password" value={cvv} onChange={(e) => setCvv(e.target.value.replace(/\D/g, "").slice(0, 3))} placeholder="CVV"/>
                                    </div>
                                </div>

                                <button type="submit" className="place-order-btn" disabled={paymentProcessing}>
                                    {paymentProcessing ? "Processing Payment..." : `Pay ₹${total.toFixed(2)}`}
                                </button>

                                <button type="button" onClick={() => setShowPayment(false)} disabled={paymentProcessing}>Back</button>

                            </form>

                        </div>
                    ) : loading ? (
                        <p>Loading...</p>
                    ) : cartItems.length === 0 ? (
                        <div>
                            <p>Your cart is empty.</p>
                            <button onClick={() => navigate("/home")}>Continue Shopping</button>
                        </div>
                    ) : (
                        <div className="checkout-content">

                            <div className="checkout-form">

                                <h2>Shipping Details</h2>

                                <form onSubmit={handleSubmit}>

                                    <div className="form-group">
                                        <label>Full Name</label>
                                        <input type="text" name="fullName" value={customerDetails.fullName} onChange={handleChange} placeholder="Enter your full name"/>
                                        {errors.fullName && (<p className="error-message">{errors.fullName}</p>)}
                                    </div>

                                    <div className="form-group">
                                        <label>Phone Number</label>
                                        <input type="tel" name="phone" value={customerDetails.phone} onChange={handleChange} placeholder="Enter your phone number"/>
                                        {errors.phone && (<p className="error-message">{errors.phone}</p>)}
                                    </div>

                                    <div className="form-group">
                                        <label>Address</label>
                                        <textarea name="address" value={customerDetails.address} onChange={handleChange} placeholder="Enter your address" rows="4"/>
                                        {errors.address && (<p className="error-message">{errors.address}</p>)}
                                    </div>

                                    <div className="form-row">

                                        <div className="form-group">
                                            <label>City</label>
                                            <input type="text" name="city" value={customerDetails.city} onChange={handleChange} placeholder="Enter city"/>
                                            {errors.city && (<p className="error-message">{errors.city}</p>)}
                                        </div>

                                        <div className="form-group">
                                            <label>State</label>
                                            <input type="text" name="state" value={customerDetails.state} onChange={handleChange} placeholder="Enter state"/>
                                            {errors.state && (<p className="error-message">{errors.state}</p>)}
                                        </div>

                                    </div>

                                    <div className="form-group">
                                        <label>Pincode</label>
                                        <input type="text" name="pincode" value={customerDetails.pincode} onChange={handleChange} placeholder="Enter pincode"/>
                                        {errors.pincode && (<p className="error-message">{errors.pincode}</p>)}
                                    </div>

                                    <div className="payment-section">

                                        <h2>Payment Method</h2>

                                        <label>
                                            <input type="radio" value="Cash on Delivery" checked={paymentMethod === "Cash on Delivery"} onChange={(e) => setPaymentMethod(e.target.value)}/>
                                            Cash on Delivery
                                        </label>

                                        <label>
                                            <input type="radio" value="Online Payment" checked={paymentMethod === "Online Payment"} onChange={(e) => setPaymentMethod(e.target.value)}/>
                                            Online Payment
                                        </label>

                                    </div>

                                    <div className="checkout-items">

                                        <h2>Order Summary</h2>

                                        {cartItems.map((item) => (
                                            <div key={item.id} className="checkout-item">

                                                <div>
                                                    <h3>{item.name}</h3>
                                                    <p>Quantity: {item.quantity}</p>
                                                </div>

                                                <p>₹{(item.price * item.quantity).toFixed(2)}</p>

                                            </div>
                                        ))}

                                        <div className="checkout-bottom">

                                            <button type="submit" className="place-order-btn">{paymentMethod === "Online Payment" ? "Pay Now" : "Place Order"}</button>

                                            <div className="checkout-total">
                                                <h2>Total: ₹{total.toFixed(2)}</h2>
                                            </div>

                                        </div>

                                    </div>

                                </form>

                            </div>

                        </div>
                    )}

                </div>

            </div>
        </>
    );
}

export default Checkout;