import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Identity.Api
      "/api/auth": {
        target: "https://localhost:7046",
        changeOrigin: true,
        secure: false,
      },

      // Catalog.Api
      "/api/catalog": {
        target: "https://localhost:7221",
        changeOrigin: true,
        secure: false,
      },

      // Orders.Api
      "/api/orders": {
        target: "https://localhost:7034",
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
