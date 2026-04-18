import { forwardRef } from 'react'
import type { InputHTMLAttributes } from 'react'

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, className = '', id, ...props }, ref) => (
    <div className="flex flex-col gap-1">
      {label && (
        <label htmlFor={id} className="text-xs font-body font-medium text-text-secondary uppercase tracking-wider">
          {label}
        </label>
      )}
      <input
        ref={ref}
        id={id}
        className={`bg-bg-elevated border ${error ? 'border-red-500' : 'border-border'} text-text-primary placeholder-text-secondary px-3 py-2.5 text-sm font-body rounded-sm focus:outline-none focus:border-accent transition-colors ${className}`}
        {...props}
      />
      {error && <span className="text-xs text-red-500">{error}</span>}
    </div>
  ),
)
Input.displayName = 'Input'
