import api from "./api";

export const addReview = async (productId, rating, comment) => {
    const response = await api.post(
        "/Review",
        {
            productId: productId,
            rating: rating,
            comment: comment
        }
    );

    return response.data;
};

export const getProductReviews = async (productId) => {
    const response = await api.get(
        `/Review/product/${productId}`
    );

    return response.data;
};

export const getAllReviews = async () => {
    const response = await api.get(
        "/Review/admin"
    );

    return response.data;
};

export const deleteReview = async (id) => {
    const response = await api.delete(
        `/Review/admin/${id}`
    );

    return response.data;
};