import { forwardRef } from 'react'
import type { ButtonHTMLAttributes } from 'react'
import { Spinner } from './Spinner'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'ghost' | 'danger'
  size?: 'sm' | 'md' | 'lg'
  loading?: boolean
  fullWidth?: boolean
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ variant = 'primary', size = 'md', loading, fullWidth, children, className = '', disabled, ...props }, ref) => {
    const base = 'font-display font-bold uppercase tracking-wider transition-colors duration-150 flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed'
    const variants = {
      primary: 'bg-accent hover:bg-accent-hover text-white',
      ghost: 'bg-transparent border border-border text-text-primary hover:border-accent hover:text-accent',
      danger: 'bg-transparent border border-red-600 text-red-500 hover:bg-red-600 hover:text-white',
    }
    const sizes = {
      sm: 'px-3 py-1.5 text-xs rounded-sm',
      md: 'px-5 py-2.5 text-sm rounded-sm',
      lg: 'px-6 py-3.5 text-base rounded-sm',
    }
    return (
      <button
        ref={ref}
        disabled={disabled || loading}
        className={`${base} ${variants[variant]} ${sizes[size]} ${fullWidth ? 'w-full' : ''} ${className}`}
        {...props}
      >
        {loading && <Spinner size="sm" />}
        {children}
      </button>
    )
  },
)
Button.displayName = 'Button'
