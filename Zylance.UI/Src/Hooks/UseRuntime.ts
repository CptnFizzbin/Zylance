export interface Runtime {
  isDesktop: boolean
  isWeb: boolean
  isMobile: boolean
}

export const useRuntime = (): Runtime => {
  // TODO: Implement actual runtime detection logic
  return {
    isDesktop: true,
    isWeb: false,
    isMobile: false,
  }
}

export const useIsDesktop = (): boolean => {
  return useRuntime().isDesktop
}
