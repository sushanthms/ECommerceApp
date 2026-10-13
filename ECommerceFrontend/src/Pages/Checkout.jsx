import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../Services/api.js";

import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";
import { getCart } from "../Services/CartService";
import { useToast } from "../Context/ToastContext.jsx";
import "./Checkout.css";

const loadRazorpay = () => {
    return new Promise((resolve) => {

        if (window.Razorpay) {// is razorpay window opened/loaded
            resolve(true);// Razorpay is already available, so loading is successful
            return;
        }

        const script = document.createElement("script");// creates an element. It doesn't mean the browser has loaded it yet.

        script.src = "https://checkout.razorpay.com/v1/checkout.js";

        script.onload = () => resolve(true);// onload is an event. Runs this code when the script has successfully finished loading. now our Promise finishes successfully.

        script.onerror = () => resolve(false);

        document.body.appendChild(script);// here the script is added to the webpage
    });
};

function Checkout() {

    const showToast = useToast();
    
    const [cartItems, setCartItems] = useState([]);
    const [loading, setLoading] = useState(false);
    const [errors, setErrors] = useState({});
    const [paymentMethod, setPaymentMethod] = useState("Cash on Delivery");
    const [showPayment, setShowPayment] = useState(false);
    const [paymentProcessing, setPaymentProcessing] = useState(false);

    const [savedAddresses, setSavedAddresses] = useState([]);
    const [selectedAddressId, setSelectedAddressId] = useState("");
    const [saveAddress, setSaveAddress] = useState(false);

    const navigate = useNavigate();

    const [customerDetails, setCustomerDetails] = useState({
        fullName: "",
        phone: "",
        address: "",
        city: "",
        state: "",
        pincode: ""
    });

    const [addressMode, setAddressMode] = useState("new");

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

    const loadSavedAddresses = async () => {
        try {
            const response = await api.get("/Order/addresses");
            const addresses = Array.isArray(response.data)
                ? response.data
                : [];

            setSavedAddresses(addresses);

            if (addresses.length > 0) {
                const defaultAddress = addresses.find(a => a.isDefault);
                const initialAddress = defaultAddress || addresses[0];

                setSelectedAddressId(String(initialAddress.id));
                setAddressMode("saved");
            }
        } catch (error) {
            console.error("Error loading saved addresses:", error);
        }
    };

    useEffect(() => {
        loadCart();
        loadSavedAddresses();
    }, []);

    const validateForm = () => {
    const newErrors = {};

    if (addressMode === "saved") {
        if (!selectedAddressId) {
            newErrors.addressSelection = "Please select an address.";
        }
    }
    else {
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
    }

    setErrors(newErrors);

    return Object.keys(newErrors).length === 0;
};

const getOrderDetails = () => {
    if (addressMode === "saved") {
        return {
            addressId: Number(selectedAddressId),
            saveAddress: false
        };
    }

    return {
        ...customerDetails,
        addressId: null,
        saveAddress
    };
};

// this is called when we do cash on delivery
    const placeOrder = async () => {
        try {
            setLoading(true);

            const response = await api.post("/Order", {
                ...getOrderDetails(),
                paymentMethod
            });

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

    const handlePayment = async () => {
    try {
        setPaymentProcessing(true);

        const loaded = await loadRazorpay();

        if (!loaded) {
            showToast("Could not load payment. Check your connection.", "error");
            setPaymentProcessing(false);
            return;
        }

        const paymentResponse = await api.post("/payment/create", getOrderDetails());

        const paymentData = paymentResponse.data;

        const options = {
            key: paymentData.keyId,
            amount: Math.round(paymentData.amount * 100),
            currency: paymentData.currency,
            name: "ECommerceApp",
            description: `Order Payment`,
            order_id: paymentData.razorpayOrderId,

// handler is a feature provided by the Razorpay Checkout JavaScript SDK. Handler is a property name defined by Razorpay's Checkout API.
            handler: async function (response) {
                try {
                    await api.post("/payment/verify", {
                        razorpayOrderId: response.razorpay_order_id,
                        razorpayPaymentId: response.razorpay_payment_id,
                        razorpaySignature: response.razorpay_signature,
                        order: {...getOrderDetails(), paymentMethod}
                    });

                    showToast("Payment successful and order placed.");
                    navigate("/home");  
                } catch (error) {
                    console.error("Payment verification failed:", error);
                    alert(error.response?.data?.message || "Payment verification failed.");
                } finally {
                    setPaymentProcessing(false);
                }
            },

            modal: {
                ondismiss: function () {
                    setPaymentProcessing(false);
                }
            }
        };

        const razorpay = new window.Razorpay(options);// the Razorpay SDK receives that entire options object.

        razorpay.open();

    } catch (error) {
        console.error("Payment error:", error);
        alert(error.response?.data?.message || "Unable to start payment.");
        setPaymentProcessing(false);
    }
};

    const handleChange = (e) => {
        const { name, value } = e.target;
        // const name = e.target.name;
        // const value = e.target.value;
        setCustomerDetails({...customerDetails, [name]: value});
    };

    const total = cartItems.reduce((sum, item) => sum + item.price * item.quantity, 0);

    return (
        <>
            <Header />

            <div className="page-layout">

                <Sidebar />

                <div className="checkout-page">

                    <h1>Checkout</h1>

                    {showPayment ? (
                        <div className="payment-page">
                            <h2>Online Payment</h2>
                            <p>Total Amount: ₹{total.toFixed(2)}</p>
                            <button type="button" className="place-order-btn" onClick={handlePayment} disabled={paymentProcessing}
                            >
                                {paymentProcessing
                                    ? "Opening Payment..."
                                    : `Pay ₹${total.toFixed(2)}`}
                            </button>

                            <button type="button" onClick={() => setShowPayment(false)} disabled={paymentProcessing}>Back</button>
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

                                <form onSubmit={handleSubmit}>

                                    <h2>Shipping Details</h2>

                                    {savedAddresses.length > 0 && (
                                        <div className="form-group">
                                            <label>
                                                <input type="radio" checked={addressMode === "saved"} onChange={() => { setAddressMode("saved"); setErrors({}); }}/>Use a saved address
                                            </label>

                                            <label>
                                                <input type="radio" checked={addressMode === "new"} onChange={() => { setAddressMode("new"); setErrors({}); }}/>Enter a new address
                                            </label>
                                        </div>
                                    )}

                                    {addressMode === "saved" && savedAddresses.length > 0 && (
                                        <div className="form-group">
                                            <label>Select Address</label>

                                            <select value={selectedAddressId} onChange={(e) => setSelectedAddressId(e.target.value)}>
                                                {savedAddresses.map((address) => (
                                                    <option key={address.id} value={address.id}>{address.fullName} — {address.line1}, {address.city}{address.isDefault ? " (Default)" : ""}</option>
                                                ))}
                                            </select>

                                            {errors.addressSelection && (
                                                <p className="error-message">{errors.addressSelection}</p>
                                            )}
                                        </div>
                                    )}

                            {addressMode === "new" && (
                                <>

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

                                    <div className="form-group">
                                        <label>
                                            <input type="checkbox" checked={saveAddress} onChange={(e) => setSaveAddress(e.target.checked)}/>Save this address for future orders
                                        </label>
                                    </div>
                                </>
                            )}

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