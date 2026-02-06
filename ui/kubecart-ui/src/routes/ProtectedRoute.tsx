import { Navigate, Outlet } from "react-router-dom";

function hasToken() {
  return !!localStorage.getItem("kc_token");
}

export default function ProtectedRoute() {
  if (!hasToken()) return <Navigate to="/login" replace />;
  return <Outlet />;
}
