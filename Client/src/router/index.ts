/// <reference types="vite/client" />
import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '../stores/auth' 
import { watch } from 'vue' 

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean;
    guestOnly?: boolean;
  }
}

const routes: RouteRecordRaw[] = [
  { 
    path: '/', 
    name: 'home', 
    component: () => import('../views/HomeView.vue') 
  },
  { 
    path: '/login', 
    name: 'login', 
    component: () => import('../views/LoginView.vue'),
    meta: { guestOnly: true } 
  },
  { 
    path: '/register', 
    name: 'register', 
    component: () => import('../views/RegisterView.vue'),
    meta: { guestOnly: true }
  },
  {
    path: '/dashboard',
    component: () => import('../views/DashboardView.vue'),
    meta: { requiresAuth: true }, 
    children: [
      { 
        path: '', 
        name: 'dashboard-home', 
        component: () => import('../views/dashboard/DashboardHomeView.vue') 
      },
      { 
        path: 'products', 
        name: 'products', 
        component: () => import('../views/dashboard/ProductsView.vue') 
      },
      { 
        path: 'customers', 
        name: 'customers', 
        component: () => import('../views/dashboard/CustomersView.vue') 
      },
      { 
        path: 'sales', 
        name: 'sales', 
        component: () => import('../views/dashboard/SalesView.vue') 
      },
      { 
        path: 'finance', 
        name: 'finance', 
        component: () => import('../views/dashboard/FinanceView.vue') 
      },
      { 
        path: 'settings', 
        name: 'settings', 
        component: () => import('../views/dashboard/SettingsView.vue') 
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition;
    } else {
      return { top: 0 };
    }
  },

  routes
})

// --- GLOBAL NAVIGATION GUARD ---
router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore();
  
  if (authStore.isLoading) {
    await new Promise<void>(resolve => {
        const unwatch = watch(() => authStore.isLoading, (isStillLoading) => {
            if (!isStillLoading) {
                unwatch();
                resolve();
            }
        });

        if (!authStore.isLoading) {
            unwatch();
            resolve();
        }
    });
  }

  const isAuthenticated = authStore.isAuthenticated;

  if (to.meta.requiresAuth && !isAuthenticated) {
    next({ name: 'login', query: { redirect: to.fullPath } });
  } 
  else if (to.meta.guestOnly && isAuthenticated) {
    next({ name: 'dashboard-home' });
  } 
  else {
    next();
  }
});

export default router;