import axios from "axios";

const API_URL = `${import.meta.env.VITE_API_URL}/Cart`;

const authHeader = () => {
    const token = localStorage.getItem("token");
    return { Authorization: `Bearer ${token}` };
};

export const addToCart = async (productId) => {
    const response = await axios.post(
        API_URL,
        { productId: productId, quantity: quantity },
        { headers: authHeader() }
    );

    return response.data;
};

export const getCart = async () => {
    const response = await axios.get(API_URL, {
        headers: authHeader()
    });

    return response.data;
};

export const updateCartItemQuantity = async (cartItemId, quantity) => {
    const response = await axios.put(
        `${API_URL}/${cartItemId}`,
        { quantity: quantity },
        { headers: authHeader() }
    );

    return response.data;
};

export const removeFromCart = async (cartItemId) => {
    const response = await axios.delete(`${API_URL}/${cartItemId}`, {
        headers: authHeader()
    });

    return response.data;
};