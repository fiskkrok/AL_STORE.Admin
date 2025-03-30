// tailwind.config.js
const colors = require('tailwindcss/colors');

module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        // Primary color - Blue with scientific reasoning
        primary: {
          50: '#eff6ff',
          100: '#dbeafe',
          200: '#bfdbfe',
          300: '#93c5fd',
          400: '#60a5fa',
          500: '#3b82f6', // Base primary color
          600: '#2563eb',
          700: '#1d4ed8',
          800: '#1e40af',
          900: '#1e3a8a',
          950: '#172554',
        },
        // Neutral tones - Reduced contrast for eye comfort
        neutral: colors.slate,
        // Semantic colors
        success: colors.emerald,
        warning: colors.amber,
        error: colors.rose,
        info: colors.sky,
      },
      // Spacing system
      spacing: {
        // Using Tailwind's default spacing system, which is already
        // designed with a spacing scale based on a 4px (0.25rem) base
      },
      // Elevation system with scientifically tuned shadows
      boxShadow: {
        'subtle': '0 1px 2px 0 rgba(0, 0, 0, 0.05)',
        'floating': '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
        'prominent': '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
      },
      // Typography system
      fontFamily: {
        sans: [
          'Inter', // Primary font for better legibility
          'system-ui',
          'sans-serif'
        ],
        mono: [
          'JetBrains Mono', // Monospace for code
          'monospace'
        ],
      },
      fontSize: {
        // Follows modular scale with 1.25 ratio
        xs: ['0.75rem', { lineHeight: '1rem' }],
        sm: ['0.875rem', { lineHeight: '1.25rem' }],
        base: ['1rem', { lineHeight: '1.5rem' }],
        lg: ['1.125rem', { lineHeight: '1.75rem' }],
        xl: ['1.25rem', { lineHeight: '1.75rem' }],
        '2xl': ['1.5rem', { lineHeight: '2rem' }],
        '3xl': ['1.875rem', { lineHeight: '2.25rem' }],
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'), // For improved form elements
  ],
  // Dark mode support
  darkMode: 'class'
};