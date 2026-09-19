import api from "./api";

export const getBanners = async () => {
    const response = await api.get("/Banner");

    return response.data;
};

export const getBanner = async (id) => {
    const response = await api.get(`/Banner/${id}`);

    return response.data;
};

export const addBanner = async (bannerData) => {
    const response = await api.post(
        "/Banner",
        bannerData
    );

    return response.data;
};

export const updateBanner = async (id, bannerData) => {
    const response = await api.put(
        `/Banner/${id}`,
        bannerData
    );

    return response.data;
};

export const deleteBanner = async (id) => {
    const response = await api.delete(`/Banner/${id}`);

    return response.data;
};