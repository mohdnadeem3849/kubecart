import { http } from "./http";

export type Product = {
  id: number;
  name: string;
  price: number;
  imageUrl: string | null;
  categoryName: string;
  stock: number;
};

export const catalogApi = {
  getProducts: async (): Promise<Product[]> => {
    const data = await http<any[]>("/api/catalog/products");

    return data.map(p => ({
      id: p.Id,
      name: p.Name,
      price: p.Price,
      imageUrl: p.ImageUrl,
      categoryName: p.CategoryName,
      stock: p.Stock,
    }));
  }
};
