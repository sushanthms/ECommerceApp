import api from "./api";

export const getProducts = async (page = 1, pageSize = 20, category = "") => {
    const response = await api.get(
        `/Product?page=${page}&pageSize=${pageSize}&category=${encodeURIComponent(category)}`
    );

    return response.data;
};

export const getAdminProducts = async (page = 1, pageSize = 20) => {
    const response = await api.get(
        `/Product/admin?page=${page}&pageSize=${pageSize}`
    );

    return response.data;
};

export const addProduct = async (product) => {
    const response = await api.post(
        "/Product",
        product
    );

    return response.data;
};

// this upload function uploads csv files, individual images, and images folder
export const uploadProducts = async (file, imageFiles) => {

    const formData = new FormData();

    formData.append("file", file);

    imageFiles.forEach((image) => {
        formData.append("images", image);
    });

    const response = await api.post(
        "/Product/upload",
        formData
    );

    return response.data;
};

// runs during the individual + Add Product when we select images
export const uploadProductImages = async (productId, imageFiles) => {
    const formData = new FormData();

    imageFiles.forEach((file) => {
        formData.append("files", file);
    });

    const response = await api.post(
        `/Product/${productId}/images`,
        formData
    );

    return response.data;
};

export const updateProduct = async (id, product) => {
    const response = await api.put(
        `/Product/${id}`,
        product
    );

    return response.data;
};

export const searchProducts = async (search, page = 1, pageSize = 20, category = "") => {
    const response = await api.get(
        `/Product/search?search=${encodeURIComponent(search)}&page=${page}&pageSize=${pageSize}&category=${encodeURIComponent(category)}`
    );

    return response.data;
};

export const searchAdminProducts = async (search, page = 1, pageSize = 20) => {
    const response = await api.get(
        `/Product/admin/search?search=${encodeURIComponent(search)}&page=${page}&pageSize=${pageSize}`
    );

    return response.data;
};

export const hideProduct = async (id) => {
    const response = await api.put(
        `/Product/${id}/hide`,
        {}
    );

    return response.data;
};

export const restoreProduct = async (id) => {
    const response = await api.put(
        `/Product/${id}/restore`,
        {}
    );

    return response.data;
};

export const permanentDeleteProduct = async (id) => {
    const response = await api.delete(
        `/Product/${id}/permanent`
    );

    return response.data;
};

export const getCategories = async () => {
    const response = await api.get("/Product/categories");

    return response.data;
};

export const getProductById = async (id) => {
    const response = await api.get(`/Product/${id}`);

    return response.data;
};