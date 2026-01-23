import { createContext, type FC, type PropsWithChildren, useContext } from 'react';
import { createZylanceApi } from '@/Lib/ZylanceApi';

const zylanceApi = createZylanceApi();

const ZylanceContext = createContext(zylanceApi);

export const useZylance = () => useContext(ZylanceContext);

export const ZylanceProvider: FC<PropsWithChildren> = ({ children }) => {
  return (
    <ZylanceContext.Provider value={zylanceApi}>
      {children}
    </ZylanceContext.Provider>
  );
};
