import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { authApi } from "../api/authApi";
import { useAuth } from "../auth/AuthContext";

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError("");

    try {
      const res: any = await authApi.login({ email, password });

      // ✅ Support multiple possible response shapes
      const token =
        res?.token ??
        res?.accessToken ??
        res?.data?.token ??
        res?.data?.accessToken;

      // ✅ Prevent "undefined" token being stored
      if (!token || token === "undefined") {
        console.log("Login response was:", res);
        throw new Error("Token not received from Identity API");
      }

      // ✅ IMPORTANT: your http.ts reads kc_token
      localStorage.setItem("kc_token", token);

      // If your AuthContext also stores it, keep this
      login(token);

      navigate("/products");
    } catch (err: any) {
      setError(err?.message ?? "Login failed");
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100">
      <form
        onSubmit={submit}
        className="bg-white p-8 rounded-xl shadow-md w-full max-w-md space-y-4"
      >
        <h1 className="text-2xl font-bold">Login</h1>

        {error && <div className="text-red-600 text-sm">{error}</div>}

        <input
          className="w-full border p-2 rounded"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />

        <input
          className="w-full border p-2 rounded"
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />

        <button className="w-full bg-blue-600 text-white p-2 rounded">
          Login
        </button>
      </form>
    </div>
  );
}
