import { http } from "./http";

export type LoginRequest = {
  email: string;
  password: string;
};

export type LoginResponse = {
  token: string;
};

export const authApi = {
  login: (data: LoginRequest) =>
    http<LoginResponse>("/api/auth/login", {
      method: "POST",
      body: JSON.stringify(data),
    }),
};
