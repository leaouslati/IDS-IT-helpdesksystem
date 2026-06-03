/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: "class",
  content: ["./index.html", "./src/**/*.{vue,js,ts,jsx,tsx}"],
  theme: {
    extend: {
      colors: {
        sidebar: "#1A1D2E",
        primary: "#14B8A6",
        background: "#F1F5F9",
        dark: "#0F172A",
      },
    },
  },
  plugins: [],
};
