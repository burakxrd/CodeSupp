// src/utils/urlHelper.ts

/// <reference types="vite/client" />

// import.meta.env.VITE_API_BASE_URL string döner ama garanti olsun diye string tipini belirtiyoruz.
export const API_BASE_URL: string = import.meta.env.VITE_API_BASE_URL || "http://localhost:5270";

/**
 * Veritabanındaki resim yolunu tam URL'e çevirir.
 * Örn: "uploads\products\resim.jpg" -> "https://api.site.com/uploads/products/resim.jpg"
 * @param path Veritabanından gelen dosya yolu
 */
export function getImageUrl(path: string | null | undefined): string | undefined {
    if (!path) return undefined;

    // Eğer zaten tam URL ise dokunma (Örn: Dış kaynaklı resim)
    if (path.startsWith('http')) {
        return path;
    }

    // Windows ters slashlarını (\) düz slash (/) yap ve baştaki slashları temizle
    const cleanPath = path.replace(/\\/g, '/').replace(/^\/+/, '');

    return `${API_BASE_URL}/${cleanPath}`;
}