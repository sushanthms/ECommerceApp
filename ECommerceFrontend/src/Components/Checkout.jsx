import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

import Header from "../Header.jsx";
import { getCart } from "../Services/CartService.jsx";
import "./Checkout.css";

const API_URL = `${import.meta.env.VITE_API_URL}/Order`;

function Checkout({showToast}) {
    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");
    const [cartItems, setCartItems] = useState([]);
    const [loading, setLoading] = useState(false);
    const [errors, setErrors] = useState({});

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

    const validateForm = () => {
    const newErrors = {};// key value pair, fullName: "Full name is required."

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

            return Object.keys(newErrors).length === 0;// newErrors is an Object, so we we wrote Object.keys(newErrors).length === 0, it means if there is no error, then it returns true, otherwise it returns false.
};

const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) {// true or false
        return;
    }

    try {
        setLoading(true);// shows loading then performs order posting, then navigates to home.

        const token = localStorage.getItem("token");

        const response = await axios.post(
            API_URL,
            customerDetails,
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

        alert(
            error.response?.data?.message ||
            "Failed to place order."
        );

    } finally {
        setLoading(false);
    }
};

    const [customerDetails, setCustomerDetails] = useState({
    fullName: "",
    phone: "",
    address: "",
    city: "",
    state: "",
    pincode: ""
});
// this function updates the states as we write. this applies for full customer details properties not name. name means the variable that hold the key here
const handleChange = (e) => {
    const { name, value } = e.target;

    setCustomerDetails({...customerDetails, [name]: value});
};  

    const total = cartItems.reduce(
        (sum, item) => sum + item.price * item.quantity,
        0
    );

    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="User" onCartClick={() => navigate("/cart")}/>

            <div className="checkout-page">

                <h1>Checkout</h1>

                {loading ? (
                    <p>Loading...</p>
                ) : cartItems.length === 0 ? (
                    <div>
                        <p>Your cart is empty.</p>

                        <button onClick={() => navigate("/home")}>
                            Continue Shopping
                        </button>
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
            <input type="tel" name="phone" value={customerDetails.phone} onChange={handleChange} placeholder="Enter your phone number" />
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
            <input type="text" name="pincode" value={customerDetails.pincode} onChange={handleChange}placeholder="Enter pincode"/>
            {errors.pincode && (<p className="error-message">{errors.pincode}</p>)}
        </div>
        <button type="submit" className="place-order-btn">Place Order</button>

    </form>

</div>

                        <div className="checkout-items">

                            <h2>Order Summary</h2>

                            {cartItems.map((item) => (
                                <div key={item.id} className="checkout-item">
                                    <div>
                                        <h3>{item.name}</h3>
                                        <p>Quantity: {item.quantity}</p>
                                    </div>

                                    <p>
                                        ₹{(item.price * item.quantity).toFixed(2)}
                                    </p>
                                </div>
                            ))}

                            <div className="checkout-total">
                                <h2>Total: ₹{total.toFixed(2)}</h2>
                            </div>

                        </div>

                    </div>
                )}

            </div>
        </>
    );
}

export default Checkout;