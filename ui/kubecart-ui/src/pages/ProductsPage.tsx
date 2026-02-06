import { useEffect, useMemo, useState } from "react";
import { catalogApi, type Product } from "../api/catalogApi";
import { useCart } from "../features/cart/cartStore";

// FRONTEND-ONLY IMAGE MAP (served from /public)
const productImages: Record<string, string[]> = {
  "iPhone 15": [
    "/images/products/iphone-15/1.jpg",
    "/images/products/iphone-15/2.jpg",
    "/images/products/iphone-15/3.jpg",
  ],

  "Gaming Keyboard": [
    "/images/products/keyboard/1.jpg",
    "/images/products/keyboard/2.jpg",
    "/images/products/keyboard/3.jpg",
  ],

  "Sneakers": [
    "/images/products/sneakers/1.jpg",
    "/images/products/sneakers/2.jpg",
    "/images/products/sneakers/3.jpg",
  ],

  "Cap": [
    "/images/products/cap/1.jpg",
    "/images/products/cap/2.jpg",
  ],

  "Jeans": [
    "/images/products/jeans/1.jpg",
    "/images/products/jeans/2.jpg",
    "/images/products/jeans/3.jpg",
  ],
};


function normalizeName(name: string) {
  return name.trim().toLowerCase();
}

export default function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const { add } = useCart();

  // keep per-product image index (for dots)
  const [activeIndex, setActiveIndex] = useState<Record<number, number>>({});

  useEffect(() => {
    catalogApi
      .getProducts()
      .then(setProducts)
      .catch((e) => setError(e?.message ?? "Failed to load products"))
      .finally(() => setLoading(false));
  }, []);

  const imagesByName = useMemo(() => {
    // make lookup tolerant (case differences)
    const map = new Map<string, string[]>();
    Object.entries(productImages).forEach(([k, v]) =>
      map.set(normalizeName(k), v)
    );
    return map;
  }, []);

  if (loading) return <div className="p-4">Loading products...</div>;
  if (error) return <div className="p-4 text-red-600">{error}</div>;

  return (
    <div className="p-6 grid grid-cols-1 md:grid-cols-3 gap-6">
      {products.map((p) => {
        const imgs = imagesByName.get(normalizeName(p.name)) ?? [];
        const idx = activeIndex[p.id] ?? 0;
        const src = imgs[idx] || "/placeholder.svg";

        return (
          <div
            key={p.id}
            className="border rounded-xl p-4 shadow hover:shadow-lg transition"
          >
            {/* Image */}
            <div className="relative">
              <img
                src={src}
                alt={p.name}
                className="h-48 w-full object-cover rounded bg-gray-50"
                onError={(e) => {
                  e.currentTarget.src = "/placeholder.svg";
                }}
              />

              {/* Dots (only if product has images) */}
              {imgs.length > 1 && (
                <div className="mt-2 flex justify-center gap-2">
                  {imgs.map((_, i) => (
                    <button
                      key={i}
                      type="button"
                      aria-label={`Image ${i + 1}`}
                      onClick={() =>
                        setActiveIndex((prev) => ({ ...prev, [p.id]: i }))
                      }
                      className={`h-2 w-2 rounded-full ${
                        i === idx ? "bg-blue-600" : "bg-gray-300"
                      }`}
                    />
                  ))}
                </div>
              )}
            </div>

            {/* Name */}
            <div className="mt-3 font-semibold text-lg">{p.name}</div>

            {/* Category */}
            <div className="text-sm text-gray-500">{p.categoryName}</div>

            {/* Price */}
            <div className="text-gray-800 font-medium mt-1">
              ${Number(p.price).toFixed(2)}
            </div>

            {/* Stock */}
            <div
              className={`text-sm font-medium mt-1 ${
                p.stock > 0 ? "text-green-600" : "text-red-600"
              }`}
            >
              {p.stock > 0 ? `In stock (${p.stock})` : "Out of stock"}
            </div>

            {/* Button */}
            <button
              onClick={() => add(p)}
              disabled={p.stock <= 0}
              className="mt-3 w-full bg-blue-600 disabled:bg-gray-400 text-white p-2 rounded"
            >
              Add to Cart
            </button>
          </div>
        );
      })}
    </div>
  );
}
