import type { Config } from 'tailwindcss'

export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        bg: {
          base: '#0D0D0D',
          surface: '#1A1A1A',
          elevated: '#242424',
        },
        accent: {
          DEFAULT: '#E63329',
          hover: '#FF4540',
        },
        text: {
          primary: '#F5F5F5',
          secondary: '#888888',
        },
        border: '#2E2E2E',
      },
      fontFamily: {
        display: ['"Barlow Condensed"', 'sans-serif'],
        body: ['"DM Sans"', 'sans-serif'],
      },
      borderRadius: {
        sm: '2px',
        DEFAULT: '4px',
      },
    },
  },
  plugins: [],
} satisfies Config
