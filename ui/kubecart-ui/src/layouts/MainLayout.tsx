import { Outlet, Link } from "react-router-dom";
import { useCart } from "../features/cart/cartStore";

export default function MainLayout() {
  const { totalItems } = useCart();

  return (
    <div className="min-h-screen">
      <header className="border-b">
        <div className="mx-auto max-w-6xl px-6 py-4 flex items-center justify-between">
          <Link to="/products" className="text-2xl font-bold text-blue-600">
            KubeCart
          </Link>

          <nav className="flex items-center gap-6">
            <Link to="/products" className="hover:underline">
              Products
            </Link>

            <Link to="/cart" className="relative hover:underline">
              Cart
              {totalItems > 0 && (
                <span className="absolute -top-2 -right-4 text-xs bg-red-600 text-white px-2 py-0.5 rounded-full">
                  {totalItems}
                </span>
              )}
            </Link>

            <Link to="/orders" className="hover:underline">
              Orders
            </Link>
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-6 py-6">
        <Outlet />
      </main>
    </div>
  );
}
