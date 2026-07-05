import api from "./axios";

export const profileApi = {
  getProfile: () => api.get("/profile"),
  updateProfile: (dto) => api.put("/profile", dto),
  changePassword: (dto) => api.put("/profile/change-password", dto),
  uploadPicture: (file) => {
    const formData = new FormData();
    formData.append("file", file);
    return api.post("/profile/picture", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });
  },
  deletePicture: () => api.delete("/profile/picture"),
  getPictureBlob: () => api.get("/profile/picture", { responseType: "blob" }),
};
