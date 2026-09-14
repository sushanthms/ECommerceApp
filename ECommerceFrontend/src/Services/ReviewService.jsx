import axios from "axios";

const API_URL = `${import.meta.env.VITE_API_URL}/Review`;

const authHeader = () => {
    const token = localStorage.getItem("token");
    return { Authorization: `Bearer ${token}` };
};

export const addReview = async (productId, rating, comment) => {
    const response = await axios.post(
        API_URL,
        {
            productId: productId,
            rating: rating,
            comment: comment
        },
        { headers: authHeader() }
    );

    return response.data;
};

export const getProductReviews = async (productId) => {
    const response = await axios.get(
        `${API_URL}/product/${productId}`
    );

    return response.data;
};

export const getAllReviews = async () => {
    const response = await axios.get(
        `${API_URL}/admin`,
        { headers: authHeader() }
    );

    return response.data;
};

export const deleteReview = async (id) => {
    const response = await axios.delete(
        `${API_URL}/admin/${id}`,
        { headers: authHeader() }
    );

    return response.data;
};