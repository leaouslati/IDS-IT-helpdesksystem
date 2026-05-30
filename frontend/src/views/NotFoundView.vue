<template>
  <div class="min-h-screen bg-[#F1F5F9] flex items-center justify-center">
    <div class="text-center">
      <h1 class="text-6xl font-bold text-[#14B8A6]">404</h1>
      <p class="text-xl font-medium text-[#0F172A] mt-4">Page Not Found</p>
      <p class="text-gray-500 mt-2">
        The page you are looking for doesn't exist.
      </p>
      <router-link
        to="/login"
        class="mt-6 inline-block px-6 py-2 bg-[#14B8A6] text-white rounded-lg hover:bg-teal-600"
      >
        Go to Login
      </router-link>
    </div>
  </div>
</template>
