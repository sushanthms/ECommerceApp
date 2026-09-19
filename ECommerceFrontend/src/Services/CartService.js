import api from "./api";

export const addToCart = async (productId, quantity) => {
    const response = await api.post(
        "/Cart",
        { productId: productId, quantity: quantity }
    );

    return response.data;
};

export const getCart = async () => {
    const response = await api.get("/Cart");

    return response.data;
};

export const updateCartItemQuantity = async (cartItemId, quantity) => {
    const response = await api.put(
        `/Cart/${cartItemId}`,
        { quantity: quantity }
    );

    return response.data;
};

export const removeFromCart = async (cartItemId) => {
    const response = await api.delete(`/Cart/${cartItemId}`);

    return response.data;
};