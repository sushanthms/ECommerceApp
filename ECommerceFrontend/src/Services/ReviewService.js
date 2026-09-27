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

export const getProductReviews = async (productId, page = 1, pageSize = 10) => {
    const response = await api.get(
        `/Review/product/${productId}?page=${page}&pageSize=${pageSize}`
    );

    return response.data;// { reviews, totalCount, page, pageSize }
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

export const deleteOwnReview = async (id) => {
    const response = await api.delete(
        `/Review/${id}`
    );

    return response.data;
};