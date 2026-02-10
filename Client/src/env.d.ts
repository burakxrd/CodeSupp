/// <reference types="vite/client" />

// 1. Env değişkenlerinin tipini buraya tanımlıyoruz
interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string;}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

// 2. Vue bileşenlerinin tanımı (Mevcut kodun)
declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/ban-types
  const component: DefineComponent<{}, {}, any>
  export default component
}