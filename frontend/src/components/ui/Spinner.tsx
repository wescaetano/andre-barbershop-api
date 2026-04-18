interface SpinnerProps { size?: 'sm' | 'md' | 'lg' }

export function Spinner({ size = 'md' }: SpinnerProps) {
  const sizes = { sm: 'w-3 h-3 border', md: 'w-5 h-5 border-2', lg: 'w-8 h-8 border-2' }
  return (
    <div
      className={`${sizes[size]} rounded-full border-white/20 border-t-white animate-spin`}
      role="status"
      aria-label="Carregando"
    />
  )
}
