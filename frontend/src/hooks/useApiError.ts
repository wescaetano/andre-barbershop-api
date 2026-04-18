const labelMessages: Record<string, string> = {
  NOT_FOUND: 'Registro não encontrado.',
  FORBIDDEN: 'Sem permissão para esta ação.',
  INVALID_MODEL: 'Dados inválidos. Verifique os campos.',
  ALREADY_EXISTS: 'Registro já cadastrado.',
  INVALID_CREDENTIALS: 'E-mail ou senha incorretos.',
  BAD_REQUEST: 'Requisição inválida.',
  SUCCESSFUL: 'Operação realizada com sucesso.',
  SUCCESSFULCREATION: 'Criado com sucesso.',
}

export function useApiError() {
  const getMessage = (responseLabel?: string, fallback?: string): string => {
    if (!responseLabel) return fallback ?? 'Erro inesperado.'
    return labelMessages[responseLabel] ?? fallback ?? 'Erro inesperado.'
  }
  return { getMessage }
}
