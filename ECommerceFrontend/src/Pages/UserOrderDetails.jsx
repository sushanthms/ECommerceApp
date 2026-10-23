import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import api from "../Services/api.js";
import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";
import RecommendedProducts from "../Components/RecommendedProducts.jsx";
import { cancelOrder } from "../Services/OrderService";
import "./UserOrderDetails.css";

function UserOrderDetails() {

    const { id } = useParams();

    const [order, setOrder] = useState(null);
    const [loading, setLoading] = useState(true);

    const [paying, setPaying] = useState(false);

    const [editingAddress, setEditingAddress] = useState(false);
    const [savedAddresses, setSavedAddresses] = useState([]);
    const [addressMode, setAddressMode] = useState("new");
    const [selectedAddressId, setSelectedAddressId] = useState("");
    const [saveAddress, setSaveAddress] = useState(false);
    const [addressErrors, setAddressErrors] = useState({});
    const [newAddress, setNewAddress] = useState({
        fullName: "", phone: "", address: "", city: "", state: "", pincode: ""
    });

    
const fetchOrderDetails = async () => {
    try {
        const response = await api.get(`/Order/User/${id}`);
        setOrder(response.data);
    } catch (error) {
        console.error("Error fetching order details:", error);
        return false;
    } finally {
        setLoading(false);
    }
};

useEffect(() => {
    fetchOrderDetails();
}, [id]);

const handleCancelOrder = async () => {
    const confirmCancel = window.confirm("Are you sure you want to cancel this order?");

    if (!confirmCancel) {
        return;
    }

    try {
        await cancelOrder(order.id);

        setOrder({
            ...order,
            status: "Cancelled"
        });

        alert("Order cancelled successfully.");
    } catch (error) {
        console.error("Error cancelling order:", error);

        alert(error.response?.data?.message || "Failed to cancel order.");
    }
};

const handleCompletePayment = async () => {
    try {
        setPaying(true);

        const paymentResponse = await api.post(`/payment/create/${order.id}`);

        const paymentData = paymentResponse.data;

        const options = {
            key: paymentData.keyId,
            amount: Math.round(paymentData.amount * 100),
            currency: paymentData.currency,
            name: "ECommerceApp",
            description: `Order #${order.id}`,
            order_id: paymentData.razorpayOrderId,

            handler: async function (response) {
                try {
                    await api.post(`/payment/verify`,
                        {
                            razorpayOrderId: response.razorpay_order_id,
                            razorpayPaymentId: response.razorpay_payment_id,
                            razorpaySignature: response.razorpay_signature
                        }
                    );

                    setOrder({ ...order, paymentStatus: "Paid" });
                    alert("Payment successful.");
                } catch (error) {
                    alert(error.response?.data?.message || "Payment verification failed.");
                } finally {
                    setPaying(false);
                }
            },

            modal: {
                ondismiss: function () {
                    setPaying(false);
                }
            }
        };

        const razorpay = new window.Razorpay(options);
        razorpay.open();

    } catch (error) {
        alert(error.response?.data?.message || "Unable to start payment.");
        setPaying(false);
    }
};

const openEditAddress = async () => {
    setEditingAddress(true);
    setAddressErrors({});

    try {
        const res = await api.get("/Order/Addresses");
        const list = Array.isArray(res.data) ? res.data : [];
        setSavedAddresses(list);

        if (list.length > 0) {
            const def = list.find(a => a.isDefault) || list[0];
            setSelectedAddressId(String(def.id));
            setAddressMode("saved");
        } else {
            setAddressMode("new");
        }
    } catch (error) {
        console.error("Error loading addresses:", error);
    }
};

const validateNewAddress = () => {
    const e = {};
    if (!newAddress.fullName.trim()) e.fullName = "Full name is required.";
    if (!/^\d{10}$/.test(newAddress.phone)) e.phone = "Phone number must be 10 digits.";
    if (!newAddress.address.trim()) e.address = "Address is required.";
    if (!newAddress.city.trim()) e.city = "City is required.";
    if (!newAddress.state.trim()) e.state = "State is required.";
    if (!/^\d{6}$/.test(newAddress.pincode)) e.pincode = "Pincode must be 6 digits.";
    setAddressErrors(e);
    return Object.keys(e).length === 0;
};

const handleSaveAddress = async () => {
    if (addressMode === "new" && !validateNewAddress()) return;

    const payload = addressMode === "saved"
        ? { addressId: Number(selectedAddressId), saveAddress: false }
        : { ...newAddress, addressId: null, saveAddress };

    try {
        await api.put(`/Order/User/${order.id}/address`, payload);

        // reloading the order so the screen shows the new copy of the address
        const refreshed = await fetchOrderDetails();

        if (refreshed) {
            setEditingAddress(false);
            alert("Delivery address updated.");
        } else {
            alert("Address updated, but failed to refresh order details.");
        }
        
    } catch (error) {
        alert(error.response?.data?.message || "Failed to update address.");
    }
};

    if (loading) {
        return (
            <div className="order-details-page">
                <p>Loading order...</p>
            </div>
        );
    }

    if (!order) {
        return (
            <div className="order-details-page">
                <p>Order not found.</p>
            </div>
        );
    }

    const orderItems = order.orderItems || [];

    const canModifyOrder = order.status === "Pending" || order.status === "Processing";

    return (
        <>
            <Header />

            <div className="page-layout">

                <Sidebar/>

                <div className="order-details-page">

                    <div className="order-top">
                        <div>
                            <h2>Order Details</h2>
                            <p>Order #{order.id} •{" "}{new Date(order.orderDate).toLocaleDateString()}</p>
                        </div>

                        <span className={`order-details-status ${order.status.toLowerCase()}`}>{order.status}</span>
                    </div>

                    <div className="order-actions">
                        <Link to="/orders" className="back-orders-button">Back to Orders</Link>
                        {canModifyOrder && (<button className="cancel-order-button" onClick={handleCancelOrder}>Cancel Order</button>)}</div>

                    <div className="order-details-card">
                        <h3>Items in this order</h3>

                        <div className="order-items">
                            {orderItems.map((item) => (
                                <div className="order-detail-item" key={item.id}>

                                    <div className="order-item-left">

                                        {item.productImage ? (
                                            <img src={`${import.meta.env.VITE_API_URL.replace("/api", "")}${item.productImage}`} alt={item.productName} className="order-detail-image" />
                                        ) : (
                                            <div className="order-detail-image-fallback">📦</div>
                                        )}

                                        <div className="order-item-info">
                                            <h4>{item.productName}</h4>
                                            <p>Quantity: {item.quantity}</p>
                                            <p>Price: ₹{item.price}</p>
                                        </div>

                                    </div>
                                    <strong>₹{Math.round(item.price * item.quantity)}</strong>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="order-details-card">
                        <h3>Delivery Address</h3>

                        <div className="order-address">
                            <p className="delivery-name">{order.deliveryName}</p>
                            <p>{order.phone}</p>
                            <p>{order.address}</p>
                            <p>{order.city}, {order.state} - {order.pincode}</p>
                        </div>
                    </div>

                    {canModifyOrder && !editingAddress && (
                        <button className="change-orders-address" onClick={openEditAddress}>Change Address</button>
                    )}

                    {editingAddress && (
                        <div className="form-group">
                            {savedAddresses.length > 0 && (
                                <>
                                    <label>
                                        <input type="radio" checked={addressMode === "saved"} onChange={() => { setAddressMode("saved"); setAddressErrors({}); }} />Use a saved address
                                    </label>
                                    <label>
                                        <input type="radio" checked={addressMode === "new"} onChange={() => { setAddressMode("new"); setAddressErrors({}); }} />Enter a new address
                                    </label>
                                </>
                            )}

                            {addressMode === "saved" && savedAddresses.length > 0 && (
                                <select value={selectedAddressId} onChange={(e) => setSelectedAddressId(e.target.value)}>
                                    {savedAddresses.map((a) => (
                                        <option key={a.id} value={a.id}>
                                            {a.fullName} — {a.line1}, {a.city}{a.isDefault ? " (Default)" : ""}
                                        </option>
                                    ))}
                                </select>
                            )}

                            {addressMode === "new" && (
                                <>
                                    {["fullName", "phone", "address", "city", "state", "pincode"].map((field) => (
                                        <div key={field}>
                                            <input type="text" placeholder={field} value={newAddress[field]} onChange={(e) => setNewAddress({ ...newAddress, [field]: e.target.value })}/>
                                            {addressErrors[field] && <p className="error-message">{addressErrors[field]}</p>}
                                        </div>
                                    ))}
                                    <label>
                                        <input type="checkbox" checked={saveAddress} onChange={(e) => setSaveAddress(e.target.checked)} />Save this address for future orders
                                    </label>
                                </>
                            )}

                            <div>
                                <button onClick={handleSaveAddress}>Save</button>
                                <button onClick={() => setEditingAddress(false)}>Cancel</button>
                            </div>
                        </div>
                    )}

                    <div className="order-details-card">
                        <h3>Order Summary</h3>

                        <div className="summary-row">
                            <span>Items</span>
                            <span>{orderItems.length}</span>
                        </div>
                        <div className="summary-row total-row">
                            <span>Ordered Date</span>
                            <span>{new Date(order.orderDate).toLocaleDateString()}</span>
                        </div>
                        <div className="summary-row total-row">
                            <span>Order Status</span>
                            <span>{order.status.toLowerCase()}</span>
                        </div>
                        <div className="summary-row total-row">
                            <span>Payment Method</span>
                            <span>{order.paymentMethod}</span>
                        </div>

                        <div className="summary-row total-row">
                            <span>Payment Status</span>
                            <span>{order.paymentStatus}</span>
                        </div>
                        <div className="summary-row total-row">
                            <span>Total Amount</span>
                            <strong>₹{Math.round(order.totalAmount)}</strong>
                        </div>
                    </div>

                    {orderItems.length > 0 && (<RecommendedProducts productId={orderItems[0].productId}title="Customers also bought"/>)}

                    {order.paymentMethod === "Online Payment" && order.paymentStatus === "Pending" && (
                        <button className="place-order-btn" onClick={handleCompletePayment} disabled={paying}>
                            {paying ? "Processing..." : "Complete Payment"}
                        </button>
                    )}

                </div>

            </div>
        </>
    );
}

export default UserOrderDetails;