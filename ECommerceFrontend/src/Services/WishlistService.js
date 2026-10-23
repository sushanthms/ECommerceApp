import api from "./api.js";

export const getWishlistIds = async () => {
    const response = await api.get("/wishlist/ids");
    return response.data;
};

export const getWishlist = async (page = 1, pageSize = 10) => {
    const response = await api.get("/wishlist", { params: { page, pageSize } });
    return response.data;
};

export const addToWishlist = (productId) => api.post(`/wishlist/${productId}`);

export const removeFromWishlist = (productId) => api.delete(`/wishlist/${productId}`);